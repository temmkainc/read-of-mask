// The school's furniture used like the rest of the game's: chairs, benches and seat rows to sit on
// (Look Closer, like the sofa in Level 07's stairwell - a bench seats you in its nearest free place,
// and nobody sits where a kid already does) and cabinet doors that open (like the closets in
// Level 06's flat). SchoolProps and SchoolBuilder call these while making the props and benches.
// Interactable things are never marked static: static batching would merge a door into the room's
// mesh, so it couldn't swing, and the outline needs the object's own mesh.

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SchoolInteractables
{
    private const string CabinetDoorSfx = "Assets/_Project/Data/Audio/SFX/CabinetDoor/CabinetDoor_";

    // how far above a seat a kid-sized sitter's eyes are
    public const float EyeAboveSeat = 1.15f;

    /// <summary>Makes <paramref name="go"/> (which has a collider) a seat - a chair has one place, a
    /// bench several. Each place is a sitter's eyes (local to it; they look along +z turned by its
    /// yaw); sitting picks the free one nearest the player.</summary>
    public static void MakeSeat(GameObject go, (Vector3 eyes, float yaw)[] places, float hintHeight)
    {
        var pivot = new GameObject("SeatPlace");
        pivot.transform.SetParent(go.transform, false);
        var point = new GameObject("LookPoint");
        point.transform.SetParent(pivot.transform, false);
        var look = point.AddComponent<InteractionLookPoint>();

        AddOutline(go, 7f);
        var seat = new SerializedObject(go.AddComponent<BenchInteractable>());
        seat.FindProperty("<CameraSnapPoint>k__BackingField").objectReferenceValue = look;
        seat.FindProperty("_place").objectReferenceValue = pivot.transform;
        var eyes = seat.FindProperty("_places");
        var yaws = seat.FindProperty("_yaws");
        eyes.arraySize = yaws.arraySize = places.Length;
        for (int i = 0; i < places.Length; i++)
        {
            eyes.GetArrayElementAtIndex(i).vector3Value = places[i].eyes;
            yaws.GetArrayElementAtIndex(i).floatValue = places[i].yaw;
        }
        seat.FindProperty("_hintLabel").stringValue = "Sit";
        seat.FindProperty("_hintYOffset").floatValue = hintHeight;
        seat.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Places all along a bench (long along x, from -<paramref name="half"/> to +<paramref name="half"/>),
    /// a quarter of a metre apart, so wherever the kids on it leave room, there's a place.</summary>
    public static (float x, float z, float yaw)[] AlongBench(float half, float z, float yaw)
    {
        const float step = 0.25f;
        int count = Mathf.FloorToInt(2f * half / step + 0.001f) + 1;
        var places = new (float, float, float)[count];
        for (int i = 0; i < count; i++)
            places[i] = (-half + i * step, z, yaw);
        return places;
    }

    /// <summary>
    /// Marks the places a kid sits in (or too close to) as taken, so the player never sits inside one;
    /// a seat with every place taken can't be used at all. Worked out afresh from where the kids are
    /// each time - called once the school is furnished and again once the kids are placed.
    /// </summary>
    public static void MarkTakenSeats(Transform level)
    {
        const float reach = 0.75f;   // a place this close to a sitting kid is theirs - two kids' shoulders
        var sitters = level.GetComponentsInChildren<SchoolFigure>(true)
            .Where(f => f.CurrentActivity == SchoolFigure.Activity.Sit || f.CurrentActivity == SchoolFigure.Activity.SitPlay)
            .Select(f => new Vector2(f.transform.position.x, f.transform.position.z)).ToList();

        foreach (var seat in level.GetComponentsInChildren<BenchInteractable>(true))
        {
            var so = new SerializedObject(seat);
            var eyes = so.FindProperty("_places");
            var taken = so.FindProperty("_taken");
            taken.arraySize = eyes.arraySize;
            for (int i = 0; i < eyes.arraySize; i++)
            {
                var at = seat.transform.TransformPoint(eyes.GetArrayElementAtIndex(i).vector3Value);
                taken.GetArrayElementAtIndex(i).boolValue = sitters.Any(s => Vector2.Distance(s, new Vector2(at.x, at.z)) < reach);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    /// <summary>Makes <paramref name="door"/>, pivoted on its hinge, a cabinet door that swings
    /// <paramref name="openAngle"/> degrees round Y, one way only.</summary>
    public static void MakeCabinetDoor(GameObject door, float openAngle, float hintHeight)
    {
        door.layer = LayerMask.NameToLayer("Interactable");
        AddOutline(door, 10f);
        var so = new SerializedObject(door.AddComponent<DoorInteractable>());
        so.FindProperty("_transformToRotate").objectReferenceValue = door.transform;
        so.FindProperty("_rotationAxis").enumValueIndex = (int)DoorInteractable.Axis.Y;
        so.FindProperty("_openAnglePush").floatValue = openAngle;
        so.FindProperty("_openAnglePull").floatValue = openAngle;
        so.FindProperty("_isDependableFromSide").boolValue = false;
        so.FindProperty("_duration").floatValue = 0.6f;
        so.FindProperty("_withSfx").boolValue = true;
        so.FindProperty("_customSfxOpen").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(CabinetDoorSfx + "Open.asset");
        so.FindProperty("_customSfxClose").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(CabinetDoorSfx + "Close.asset");
        so.FindProperty("_hintYOffset").floatValue = hintHeight;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>True for anything the player can use - it has to stay non-static.</summary>
    public static bool IsInteractable(GameObject go) => go.GetComponentInParent<IInteractable>() != null;

    // the game's black outline, shown while the player looks at it (HighlightManager turns it on)
    private static void AddOutline(GameObject go, float width)
    {
        var outline = go.AddComponent<Outline>();
        var so = new SerializedObject(outline);
        var mode = so.FindProperty("outlineMode");
        mode.enumValueIndex = System.Array.IndexOf(mode.enumNames, "OutlineAndAdditiveHidden");
        so.FindProperty("outlineColor").colorValue = Color.black;
        so.FindProperty("outlineWidth").floatValue = width;
        so.ApplyModifiedPropertiesWithoutUndo();
        outline.enabled = false;
    }
}
#endif
