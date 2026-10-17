using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Moves Outline's precomputed smooth normals out of scenes/prefabs (inline per component) into
/// shared OutlineSmoothNormals assets (one per mesh). Also used to rebake after a model changes.
/// </summary>
public static class OutlineSmoothNormalsMigration {
  static readonly string[] OutlineProperties = { "precomputeOutline", "bakeKeys", "bakeValues", "sharedBakes" };

  [MenuItem("Tools/QuickOutline/Move Precomputed Normals To Shared Assets")]
  static void MigrateMenu() => Debug.Log(Run(forceRebake: false));

  [MenuItem("Tools/QuickOutline/Rebake Shared Normals (after model changes)")]
  static void RebakeMenu() => Debug.Log(Run(forceRebake: true));

  /// <summary>Processes every prefab under Assets/_Project and every enabled build scene.</summary>
  public static string Run(bool forceRebake) {
    if (Application.isPlaying) {
      return "[OutlineSmoothNormals] Exit Play mode first.";
    }

    for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
      if (EditorSceneManager.GetSceneAt(i).isDirty) {
        return "[OutlineSmoothNormals] Save or discard changes in the open scenes first.";
      }
    }

    var setup = EditorSceneManager.GetSceneManagerSetup();
    var report = new StringBuilder("[OutlineSmoothNormals] ");
    var uncovered = new Dictionary<Mesh, int>();

    var prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" })
      .Select(AssetDatabase.GUIDToAssetPath)
      .Where(p => AssetDatabase.LoadAssetAtPath<GameObject>(p).GetComponentsInChildren<Outline>(true).Length > 0)
      .ToList();

    // Two passes so nested prefab instances end up inheriting from their already-migrated source
    // prefab instead of keeping their own overridden copy.
    int prefabChanges = 0;
    for (int pass = 0; pass < 2; pass++) {
      foreach (var path in prefabPaths) {
        var root = PrefabUtility.LoadPrefabContents(path);
        int changed = Process(root.GetComponentsInChildren<Outline>(true), forceRebake && pass == 0, uncovered);
        if (changed > 0) {
          PrefabUtility.SaveAsPrefabAsset(root, path);
          prefabChanges += changed;
        }
        PrefabUtility.UnloadPrefabContents(root);
      }
    }
    report.Append($"prefabs with Outline: {prefabPaths.Count}, component updates: {prefabChanges}; ");

    foreach (var buildScene in EditorBuildSettings.scenes.Where(s => s.enabled)) {
      var scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
      var outlines = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Outline>(true)).ToList();
      int changed = Process(outlines, forceRebake, uncovered);
      if (changed > 0) {
        EditorSceneManager.SaveScene(scene);
      }
      report.Append($"{scene.name}: {outlines.Count} Outline, {changed} updated; ");
    }

    AssetDatabase.SaveAssets();
    if (setup.Length > 0) {
      EditorSceneManager.RestoreSceneManagerSetup(setup);
    }

    int maxVerts = uncovered.Count > 0 ? uncovered.Values.Max() : 0;
    report.Append($"meshes left to runtime compute (not assets): {uncovered.Count}, largest {maxVerts} vertices.");
    return report.ToString();
  }

  static int Process(IEnumerable<Outline> outlines, bool forceRebake, Dictionary<Mesh, int> uncovered) {
    int changed = 0;

    foreach (var outline in outlines) {

      // Prefab instances: drop per-instance copies and inherit from the source prefab
      if (PrefabUtility.IsPartOfPrefabInstance(outline)) {
        var serialized = new SerializedObject(outline);
        foreach (var name in OutlineProperties) {
          var property = serialized.FindProperty(name);
          if (property != null && property.prefabOverride) {
            PrefabUtility.RevertPropertyOverride(property, InteractionMode.AutomatedAction);
            changed++;
          }
        }
      }

      var meshes = outline.GetComponentsInChildren<MeshFilter>(true)
        .Select(mf => mf.sharedMesh)
        .Where(m => m != null)
        .Distinct()
        .ToList();

      foreach (var mesh in meshes.Where(m => !EditorUtility.IsPersistent(m))) {
        uncovered[mesh] = mesh.vertexCount;
      }

      bool missing = meshes.Any(m => EditorUtility.IsPersistent(m) && !outline.HasSharedBakeFor(m));
      if (forceRebake || missing || outline.HasInlineBakeData || !outline.PrecomputeOutline) {
        var serialized = new SerializedObject(outline);
        serialized.FindProperty("precomputeOutline").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        outline.Bake(forceRebake);
        changed++;
      }
    }

    return changed;
  }
}
