//
//  OutlineSmoothNormals.cs
//  QuickOutline (read-of-mask addition)
//
//  Precomputed smooth normals for one mesh, stored once and shared by every Outline that uses
//  that mesh, instead of being serialized inline into each Outline component (which duplicated
//  the data per component and made up over half of the chapter scene files).
//

using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;

[PreferBinarySerialization]
public class OutlineSmoothNormals : ScriptableObject {
  public const string GeneratedFolder = "Assets/_Project/Generated/OutlineSmoothNormals";

  [SerializeField]
  private Mesh mesh;

  [SerializeField]
  private Vector3[] normals;

  public Mesh Mesh => mesh;
  public Vector3[] Normals => normals;

  public bool Matches(Mesh candidate) {
    return candidate != null && mesh == candidate && normals != null && normals.Length == candidate.vertexCount;
  }

  /// <summary>
  /// Averages the normals of vertices that share a position (same result as the original
  /// QuickOutline LINQ GroupBy version, without the per-vertex allocations).
  /// </summary>
  public static List<Vector3> Compute(Mesh mesh) {
    var vertices = mesh.vertices;
    var smoothNormals = new List<Vector3>(mesh.normals);
    var groups = new Dictionary<Vector3, List<int>>(vertices.Length);

    for (int i = 0; i < vertices.Length; i++) {
      if (!groups.TryGetValue(vertices[i], out var indices)) {
        indices = new List<int>(1);
        groups.Add(vertices[i], indices);
      }
      indices.Add(i);
    }

    foreach (var indices in groups.Values) {

      // Skip single vertices
      if (indices.Count == 1) {
        continue;
      }

      // Calculate the average normal
      var smoothNormal = Vector3.zero;

      foreach (var index in indices) {
        smoothNormal += smoothNormals[index];
      }

      smoothNormal.Normalize();

      // Assign smooth normal to each vertex
      foreach (var index in indices) {
        smoothNormals[index] = smoothNormal;
      }
    }

    return smoothNormals;
  }

#if UNITY_EDITOR
  /// <summary>Returns the shared asset for a mesh asset, creating or refreshing it when needed. Null for meshes that aren't assets (e.g. scene-embedded ProBuilder meshes).</summary>
  public static OutlineSmoothNormals GetOrCreate(Mesh mesh, bool forceRebake = false) {
    if (mesh == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string guid, out long localId) || string.IsNullOrEmpty(guid)) {
      return null;
    }

    var path = $"{GeneratedFolder}/{Sanitize(mesh.name)}__{guid.Substring(0, 8)}_{localId}.asset";
    var existing = AssetDatabase.LoadAssetAtPath<OutlineSmoothNormals>(path);

    if (existing != null && existing.Matches(mesh) && !forceRebake) {
      return existing;
    }

    var asset = existing != null ? existing : CreateInstance<OutlineSmoothNormals>();
    asset.mesh = mesh;
    asset.normals = Compute(mesh).ToArray();

    if (existing == null) {
      Directory.CreateDirectory(GeneratedFolder);
      AssetDatabase.CreateAsset(asset, path);
    } else {
      EditorUtility.SetDirty(asset);
    }

    return asset;
  }

  private static string Sanitize(string name) {
    foreach (var c in Path.GetInvalidFileNameChars()) {
      name = name.Replace(c, '_');
    }
    return name;
  }
#endif
}
