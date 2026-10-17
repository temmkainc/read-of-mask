//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]

public class Outline : MonoBehaviour {
  private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

  public enum Mode {
    OutlineAll,
    OutlineVisible,
    OutlineHidden,
    OutlineAndSilhouette,
    OutlineAndAdditiveHidden,
    SilhouetteOnly
  }

  public Mode OutlineMode {
    get { return outlineMode; }
    set {
      outlineMode = value;
      needsUpdate = true;
    }
  }

  public Color OutlineColor {
    get { return outlineColor; }
    set {
      outlineColor = value;
      needsUpdate = true;
    }
  }

  public float OutlineWidth {
    get { return outlineWidth; }
    set {
      outlineWidth = value;
      needsUpdate = true;
    }
  }

  [Serializable]
  private class ListVector3 {
    public List<Vector3> data;
  }

  [SerializeField]
  private Mode outlineMode;

  [SerializeField]
  private Color outlineColor = Color.white;

  [SerializeField, Range(0f, 100f)]
  private float outlineWidth = 2f;

  [Header("Optional")]

  [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
  + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
  private bool precomputeOutline;

  [SerializeField, HideInInspector]
  private List<Mesh> bakeKeys = new List<Mesh>();

  [SerializeField, HideInInspector]
  private List<ListVector3> bakeValues = new List<ListVector3>();

  // read-of-mask: precomputed normals live in shared OutlineSmoothNormals assets (one per mesh)
  // instead of bakeKeys/bakeValues above, which are kept only so un-migrated objects still work.
  [SerializeField, HideInInspector]
  private List<OutlineSmoothNormals> sharedBakes = new List<OutlineSmoothNormals>();

  private Renderer[] renderers;
  private Material outlineMaskMaterial;
  private Material outlineFillMaterial;

  private bool needsUpdate;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        outlineFillMaterial.SetFloat("_Glossiness", 0f);
        outlineFillMaterial.SetFloat("_Metallic", 0f);
        outlineFillMaterial.EnableKeyword("_EMISSION");
        outlineFillMaterial.SetColor("_EmissionColor", outlineColor);

        LoadSmoothNormals();
        needsUpdate = true;
    }

    void OnEnable() {
    foreach (var renderer in renderers) {

      // Append outline shaders
      var materials = renderer.sharedMaterials.ToList();

      materials.Add(outlineMaskMaterial);
      materials.Add(outlineFillMaterial);

      renderer.materials = materials.ToArray();
    }
  }

  void OnValidate() {

    // Update material properties
    needsUpdate = true;

    // Clear cache when baking is disabled or corrupted
    if (!precomputeOutline && (bakeKeys.Count != 0 || sharedBakes.Count != 0) || bakeKeys.Count != bakeValues.Count) {
      bakeKeys.Clear();
      bakeValues.Clear();
      sharedBakes.Clear();
    }

#if UNITY_EDITOR
    // Generate smooth normals when baking is enabled (asset creation isn't allowed inside OnValidate)
    if (precomputeOutline && bakeKeys.Count == 0 && sharedBakes.Count == 0 && !Application.isPlaying) {
      UnityEditor.EditorApplication.delayCall += () => {
        if (this != null && precomputeOutline && sharedBakes.Count == 0) {
          Bake();
        }
      };
    }
#endif
  }

  void Update() {
    if (needsUpdate) {
      needsUpdate = false;

      UpdateMaterialProperties();
    }
  }

  void OnDisable() {
    foreach (var renderer in renderers) {

      // Remove outline shaders
      if(renderer == null) {
        continue;
      }
      var materials = renderer.sharedMaterials.ToList();

      materials.Remove(outlineMaskMaterial);
      materials.Remove(outlineFillMaterial);

      renderer.materials = materials.ToArray();
    }
  }

  void OnDestroy() {

    // Destroy material instances
    Destroy(outlineMaskMaterial);
    Destroy(outlineFillMaterial);
  }

#if UNITY_EDITOR
  /// <summary>Points this Outline at shared smooth-normal assets for every mesh under it, replacing any inline data.</summary>
  public void Bake(bool forceRebake = false) {
    sharedBakes.Clear();
    bakeKeys.Clear();
    bakeValues.Clear();

    // Generate smooth normals for each mesh
    var bakedMeshes = new HashSet<Mesh>();

    foreach (var meshFilter in GetComponentsInChildren<MeshFilter>(true)) {

      // Skip duplicates
      if (!bakedMeshes.Add(meshFilter.sharedMesh)) {
        continue;
      }

      // Meshes that aren't assets (e.g. scene-embedded ProBuilder meshes) are small and computed at runtime
      var shared = OutlineSmoothNormals.GetOrCreate(meshFilter.sharedMesh, forceRebake);
      if (shared != null) {
        sharedBakes.Add(shared);
      }
    }

    UnityEditor.EditorUtility.SetDirty(this);
    if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this)) {
      UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
  }

  // Used by the migration tool to verify coverage.
  public bool HasSharedBakeFor(Mesh mesh) => FindSharedBake(mesh) != null;
  public bool HasInlineBakeData => bakeKeys.Count != 0 || bakeValues.Count != 0;
  public bool PrecomputeOutline => precomputeOutline;
#endif

  OutlineSmoothNormals FindSharedBake(Mesh mesh) {
    for (int i = 0; i < sharedBakes.Count; i++) {
      if (sharedBakes[i] != null && sharedBakes[i].Matches(mesh)) {
        return sharedBakes[i];
      }
    }
    return null;
  }

  void LoadSmoothNormals() {

    // Retrieve or generate smooth normals
    foreach (var meshFilter in GetComponentsInChildren<MeshFilter>()) {
      var mesh = meshFilter.sharedMesh;

      // Skip if smooth normals have already been adopted
      if (mesh == null || !registeredMeshes.Add(mesh)) {
        continue;
      }

      // Store smooth normals in UV3: shared precomputed data first, then legacy inline data, then compute
      var shared = FindSharedBake(mesh);
      if (shared != null) {
        mesh.SetUVs(3, shared.Normals);
      } else {
        var index = bakeKeys.IndexOf(mesh);
        mesh.SetUVs(3, (index >= 0) ? bakeValues[index].data : OutlineSmoothNormals.Compute(mesh));
      }

      // Combine submeshes
      var renderer = meshFilter.GetComponent<Renderer>();

      if (renderer != null) {
        CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
      }
    }

    // Clear UV3 on skinned mesh renderers
    foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>()) {

      // Skip if UV3 has already been reset
      if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh)) {
        continue;
      }

      // Clear UV3
      skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

      // Combine submeshes
      CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
    }
  }

  void CombineSubmeshes(Mesh mesh, Material[] materials) {

    // Skip meshes with a single submesh
    if (mesh.subMeshCount == 1) {
      return;
    }

    // Skip if submesh count exceeds material count
    if (mesh.subMeshCount > materials.Length) {
      return;
    }

    // Append combined submesh
    mesh.subMeshCount++;
    mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
  }

  void UpdateMaterialProperties() {

    // Apply properties according to mode
    outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

    switch (outlineMode) {
      case Mode.OutlineAll:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.OutlineVisible:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.OutlineHidden:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.OutlineAndSilhouette:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.OutlineAndAdditiveHidden:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always); // draw everywhere
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

            case Mode.SilhouetteOnly:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
        outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
        break;
    }
  }
}
