using System.Linq;
using UnityEngine;

/// <summary>
/// Static batching is off for the project (the outline highlight doesn't survive it), so a level made
/// of many small pieces - a generated building - can batch its own fixed geometry when it begins:
/// every mesh under it except what moves, lights up or changes: interactables and their parts,
/// text, and anything an Animator drives (the kids, and the masks on their heads).
/// </summary>
public static class StaticGeometryBatcher
{
    /// <summary>Combines the fixed meshes under <paramref name="root"/>; returns how many.</summary>
    public static int Combine(GameObject root)
    {
        var fixedMeshes = root.GetComponentsInChildren<MeshRenderer>()
            .Where(r => r.TryGetComponent<MeshFilter>(out var filter) && filter.sharedMesh != null && filter.sharedMesh.isReadable)
            .Where(r => r.GetComponentInParent<IInteractable>() == null
                     && r.GetComponentInParent<Animator>() == null
                     && r.GetComponent<TMPro.TMP_Text>() == null)
            .Select(r => r.gameObject)
            .ToArray();
        StaticBatchingUtility.Combine(fixedMeshes, root);
        return fixedMeshes.Length;
    }
}
