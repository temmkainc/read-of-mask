// Tools → Read of Mask → Build School (Level 09)
// Blocks out Level 09, the school, right behind the dream gate in the Level 08 yard's north doorway
// (the one with the steps): you come through the gate straight into the entrance hall.
//
// Kid-sized on purpose - 7 m ceilings, an 8 m wide corridor, big rooms - so the player feels small:
//   south row:  kitchen + dining room (with a serving hatch and counter), entrance hall, gym
//               (12 m tall) with girls' and boys' changing rooms
//   middle:     the corridor, with a beamed ceiling
//   north row:  four classrooms, girls' and boys' toilets (cabins with doors, toilets, sinks)
// Door and window openings, doors (double doors into the canteen and gym, the kitchen's locked),
// signs beside the doors, floors (poured terrazzo, painted boards, gym strips, small tiles), the
// canteen's tables, and the things that come with the building in a post-Soviet school: white-painted
// double wooden windows with a vent pane and deep sills, a cast-iron radiator under every window,
// squat toilets with high cisterns, tiled toilet walls and long trough sinks. SchoolFurnisher then
// fills the rooms (it runs at the end of every build); SchoolProps makes the props for both.
//
// Everything it makes lives under Level_09/Environment and is rebuilt from scratch on every run,
// so move anything you add by hand out of there (or stop using the builder) before rebuilding.
// All sizes are world metres.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.ProBuilder;

public static class SchoolBuilder
{
    private const string LevelName = "Level_09";
    private const string EnvironmentName = "Environment";

    // heights
    private const float Floor = 0.432f;                 // the top step of the Level 08 yard doorway
    private const float Ceiling = Floor + 7f;           // kid-sized rooms (the apartment is 5.6 m)
    private const float RoofTop = Ceiling + 0.7f;
    private const float Parapet = Ceiling + 1.2f;
    private const float GymCeiling = Floor + 12f;
    private const float GymRoofTop = GymCeiling + 0.7f;
    private const float GymParapet = GymCeiling + 1.2f;
    private const float Footing = -1.5f;
    private const float RoomFloorThickness = 0.02f;     // room floors sit on the base slab

    // the building: exterior walls 1 m thick, interior walls 0.5 m
    private const float BX0 = -15f, BX1 = 81f, BZ0 = -45.78f, BZ1 = 4.22f;
    private const float Ext = 1f, Int = 0.5f;
    private const float IX0 = BX0 + Ext, IX1 = BX1 - Ext, IZ0 = BZ0 + Ext, IZ1 = BZ1 - Ext;

    // rows: the south row (dining, hall, gym), the corridor, the north row (classrooms, toilets)
    private const float RowAWall = -24.75f;             // centre of the wall between the south row and the corridor
    private const float RowCWall = -16.25f;             // centre of the wall between the corridor and the north row

    // the doorway in the Level 08 yard's north wall
    private const float GateX0 = 29.5f, GateX1 = 34.5f, GateTop = 4.49f;
    // the school's side of it, narrowed by hand to fit the double door hung in it
    private const float EntranceX0 = 29.926f, EntranceX1 = 33.979f;

    // openings
    private const float DoorWidth = 2.2f, DoorHeight = 3.85f;         // the door prefab's leaf is 2.18 x 3.845
    private const float DoorLeafCenter = 1.073f;                      // the door prefab's leaf centre, from its hinge
    private const float DoorPivotAboveFloor = 2.097f;
    private const float WideTop = Floor + 5f;
    private const float WindowWidth = 3.6f, SillY = Floor + 2.2f, WindowTopY = Floor + 5.8f;
    private const float SmallWindowWidth = 2.2f, SmallSillY = Floor + 4f;
    private const float GymWindowWidth = 4f, GymSillY = Floor + 7f, GymWindowTopY = Floor + 10.6f;

    // toilet cabins
    private const float CabinWidth = 3.2f, CabinDepth = 4.2f, CabinCount = 5;
    private const float PartitionBottom = Floor + 0.3f, PartitionTop = Floor + 4.5f, Partition = 0.08f;
    private const float CabinDoorWidth = 2f, CabinLeafCenter = 0.883f;

    private const string DoorPrefab = "Assets/_Project/Prefabs/SingleDoorFix.prefab";
    private const string CabinDoorPrefab = "Assets/_Project/Prefabs/ToiletCabinDoor.prefab";
    private const string DoorSprite = "Assets/_Project/Sprites/Props/Door_Aparment_2.png";

    // signs: a doodled plate with handwritten text above each door, pictograms on the toilet doors
    private const string SignFolder = "Assets/_Project/Sprites/School/";
    private const string SignFont = "Assets/TextMesh Pro/Resources/Fonts & Materials/Inkfree SDF.asset";
    private const float SignWidth = 1.5f, SignHeight = 0.58f, SignY = Floor + 2.75f;

    private static Material _exterior, _interior, _roof, _frame, _glass, _counter, _paint, _trim, _metal;
    private static Material _terrazzo, _boards, _gym, _smallTiles, _windowPaint;
    private static Component _outdoorSurface, _indoorSurface;
    private static StaticEditorFlags _static;

    private enum Axis { X, Z }   // which world axis a wall runs along

    private struct Opening
    {
        public float From, To, Bottom, Top;
        public Opening(float from, float to, float bottom, float top) { From = from; To = to; Bottom = bottom; Top = top; }
    }

    // which face of a wall a sign hangs on: the low side (smaller x / z) or the high side
    private enum Side { Low, High }

    // which faces of a wall get the painted band, rail and skirting
    private enum Trim { None, Low, High, Both }
    private const float DadoY = Floor + 2f;   // below the window sills

    private class DoorSpot
    {
        public Axis Axis;
        public float Plane, Center, T0, T1;
        public bool Double, Locked;
        public string Sign, Pictogram;
        public Side SignSide;
        public bool SignOnLeft;
        public float Width => Double ? DoorWidth * 2f : DoorWidth;
    }

    private static readonly List<(Axis axis, float plane, float center, float width, float sill, float top)> Windows = new();
    private static readonly List<DoorSpot> Doors = new();

    [MenuItem("Tools/Read of Mask/Build School (Level 09)")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[SchoolBuilder] Leave Play mode first.");
            return;
        }

        LoadMaterials();
        SchoolProps.BeginRun();
        var level = FindOrCreateLevel();

        var old = level.transform.Find(EnvironmentName);
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);

        var env = new GameObject(EnvironmentName).transform;
        env.SetParent(level.transform, false);
        Undo.RegisterCreatedObjectUndo(env.gameObject, "Build School");

        Windows.Clear();
        Doors.Clear();

        BuildFloors(Group(env, "Floors"));
        BuildExteriorWalls(Group(env, "ExteriorWalls"));
        BuildInteriorWalls(Group(env, "InteriorWalls"));
        BuildRoof(Group(env, "Roof"));
        BuildRelief(Group(env, "Relief"));
        BuildServingLine(Group(env, "DiningRoom_ServingLine"));
        BuildDiningTables(Group(env, "DiningRoom_Tables"));
        BuildToilets(Group(env, "Toilets"));

        var glazing = Group(env, "Windows");
        foreach (var w in Windows)
            BuildWindow(glazing, w.axis, w.plane, w.center, w.width, w.sill, w.top);
        var doors = Group(env, "Doors");
        foreach (var d in Doors)
            PlaceDoor(doors, d);

        PlaceRoomMarkers(Group(env, "Rooms"));
        SchoolFurnisher.Furnish(level);
        MarkOccluders(level.transform);

        EditorSceneManager.MarkSceneDirty(level.scene);
        Debug.Log("[SchoolBuilder] Level 09 school built under " + LevelName + "/" + EnvironmentName + ". " +
                  "Bake occlusion culling again (Tools/Read of Mask/Bake Occlusion Culling (Chapter 02)).");
    }

    // ── Occlusion culling ────────────────────────────────────────────────────
    // Only the building's opaque shell - walls, floors, roof - hides what's behind it; glass, text and
    // the furniture never do (they're still hidden behind walls themselves). Interactable things
    // aren't static at all, so they're culled as they move.
    private static void MarkOccluders(Transform level)
    {
        var env = level.Find(EnvironmentName);
        foreach (var r in level.GetComponentsInChildren<MeshRenderer>(true))
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
            if ((flags & StaticEditorFlags.OccludeeStatic) == 0)
                continue;
            bool opaque = r.sharedMaterials.All(m => m != null && m.renderQueue <= (int)UnityEngine.Rendering.RenderQueue.GeometryLast);
            bool shell = env != null && r.transform.IsChildOf(env) && opaque && r.GetComponent<TMPro.TMP_Text>() == null;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, shell ? flags | StaticEditorFlags.OccluderStatic : flags & ~StaticEditorFlags.OccluderStatic);
        }
    }

    /// <summary>Bakes Chapter 02's occlusion culling (all its levels), sized for the school's walls.
    /// Run it again after rebuilding the school.</summary>
    [MenuItem("Tools/Read of Mask/Bake Occlusion Culling (Chapter 02)")]
    public static void BakeOcclusion()
    {
        var level = GameObject.Find(LevelName);
        if (level != null)
            MarkOccluders(level.transform);
        StaticOcclusionCulling.smallestOccluder = 1.5f;   // a stretch of wall between two windows
        StaticOcclusionCulling.smallestHole = 0.25f;
        StaticOcclusionCulling.backfaceThreshold = 100f;
        StaticOcclusionCulling.GenerateInBackground();
        Debug.Log("[SchoolBuilder] Baking occlusion culling for the open scene...");
    }

    // ── Floors ───────────────────────────────────────────────────────────────
    private static void BuildFloors(Transform parent)
    {
        // a base slab under everything (seen only in the doorways, as the threshold), and each room's own floor on top
        float top = Floor - RoomFloorThickness;
        AddSurface(Box(parent, "BaseSlab", new Vector3(IX0, Footing, IZ0), new Vector3(IX1, top, IZ1), _terrazzo), _indoorSurface);
        AddSurface(Box(parent, "Threshold_Entrance", new Vector3(GateX0, Footing, BZ0), new Vector3(GateX1, top, IZ0), _terrazzo), _indoorSurface);

        void RoomFloor(string name, float x0, float z0, float x1, float z1, Material material) =>
            AddSurface(Box(parent, "Floor_" + name, new Vector3(x0, top, z0), new Vector3(x1, Floor, z1), material), _indoorSurface);

        float rowA1 = RowAWall - Int * 0.5f, corr0 = RowAWall + Int * 0.5f, corr1 = RowCWall - Int * 0.5f, rowC0 = RowCWall + Int * 0.5f;
        RoomFloor("Kitchen", IX0, IZ0, -4.25f, rowA1, _smallTiles);
        RoomFloor("DiningRoom", -3.75f, IZ0, 21.75f, rowA1, _terrazzo);
        RoomFloor("EntranceHall", 22.25f, IZ0, 41.75f, rowA1, _terrazzo);
        RoomFloor("Gym", 42.25f, IZ0, 71.75f, rowA1, _gym);
        RoomFloor("ChangingRoom_Girls", 72.25f, IZ0, IX1, -35f, _smallTiles);
        RoomFloor("ChangingRoom_Boys", 72.25f, -34.5f, IX1, rowA1, _smallTiles);
        RoomFloor("Corridor", IX0, corr0, IX1, corr1, _terrazzo);
        RoomFloor("Classroom_1", IX0, rowC0, 1.75f, IZ1, _boards);
        RoomFloor("Classroom_2", 2.25f, rowC0, 17.75f, IZ1, _boards);
        RoomFloor("Classroom_3", 18.25f, rowC0, 33.75f, IZ1, _boards);
        RoomFloor("Classroom_4", 34.25f, rowC0, 49.75f, IZ1, _boards);
        RoomFloor("Toilet_Girls", 50.25f, rowC0, 64.75f, IZ1, _smallTiles);
        RoomFloor("Toilet_Boys", 65.25f, rowC0, IX1, IZ1, _smallTiles);
    }

    // ── Exterior walls ───────────────────────────────────────────────────────
    private static void BuildExteriorWalls(Transform parent)
    {
        float southPlane = BZ0 + Ext * 0.5f, northPlane = BZ1 - Ext * 0.5f, westPlane = BX0 + Ext * 0.5f, eastPlane = BX1 - Ext * 0.5f;

        // south facade: the entrance from the Level 08 yard; kitchen and dining room windows, high gym windows
        var southWest = new List<Opening>
        {
            new Opening(EntranceX0, EntranceX1, Footing, GateTop),
            Window(Axis.X, southPlane, -9f),
        };
        foreach (float x in new[] { 1f, 7f, 13f, 19f })
            southWest.Add(Window(Axis.X, southPlane, x));
        Wall(parent, "Wall_South", Axis.X, BZ0, BZ0 + Ext, BX0, 41.75f, Footing, Parapet, southWest, _exterior, Trim.High);

        var southGym = new List<Opening>();
        foreach (float x in new[] { 47f, 53f, 59f, 65f })
            southGym.Add(Window(Axis.X, southPlane, x, GymWindowWidth, GymSillY, GymWindowTopY));
        Wall(parent, "Wall_South_Gym", Axis.X, BZ0, BZ0 + Ext, 41.75f, 72.25f, Footing, GymParapet, southGym, _exterior, Trim.High);
        Wall(parent, "Wall_South_East", Axis.X, BZ0, BZ0 + Ext, 72.25f, BX1, Footing, Parapet,
            new List<Opening> { Window(Axis.X, southPlane, 76f, SmallWindowWidth, SmallSillY) }, _exterior, Trim.High);

        // west facade: kitchen, the corridor's end, classroom 1
        Wall(parent, "Wall_West", Axis.Z, BX0, BX0 + Ext, IZ0, IZ1, Footing, Parapet, new List<Opening>
        {
            Window(Axis.Z, westPlane, -35f),
            Window(Axis.Z, westPlane, -20.5f),
            Window(Axis.Z, westPlane, -6f),
        }, _exterior, Trim.High);

        // east facade: equipment and changing rooms, the corridor's end, boys' toilet
        Wall(parent, "Wall_East", Axis.Z, BX1 - Ext, BX1, IZ0, IZ1, Footing, Parapet, new List<Opening>
        {
            Window(Axis.Z, eastPlane, -40f, SmallWindowWidth, SmallSillY),
            Window(Axis.Z, eastPlane, -30f, SmallWindowWidth, SmallSillY),
            Window(Axis.Z, eastPlane, -20.5f),
            Window(Axis.Z, eastPlane, -14.5f, SmallWindowWidth, SmallSillY),
        }, _exterior, Trim.Low);

        // north facade: three windows per classroom, small ones in the toilets
        var north = new List<Opening>();
        foreach (float x in new[] { -10.5f, -6f, -1.5f, 5.5f, 10f, 14.5f, 21.5f, 26f, 30.5f, 37.5f, 42f, 46.5f })
            north.Add(Window(Axis.X, northPlane, x));
        foreach (float x in new[] { 53f, 57f, 68.5f, 72.5f })
            north.Add(Window(Axis.X, northPlane, x, SmallWindowWidth, SmallSillY));
        Wall(parent, "Wall_North", Axis.X, BZ1 - Ext, BZ1, BX0, BX1, Footing, Parapet, north, _exterior, Trim.Low);
    }

    private static Opening Window(Axis axis, float plane, float center, float width = WindowWidth, float sill = SillY, float top = WindowTopY)
    {
        Windows.Add((axis, plane, center, width, sill, top));
        return new Opening(center - width * 0.5f, center + width * 0.5f, sill, top);
    }

    // ── Interior walls ───────────────────────────────────────────────────────
    private static void BuildInteriorWalls(Transform parent)
    {
        float a0 = RowAWall - Int * 0.5f, a1 = RowAWall + Int * 0.5f;
        float c0 = RowCWall - Int * 0.5f, c1 = RowCWall + Int * 0.5f;

        // south row | corridor: double doors into the dining room and the gym, the hall wide open, changing room door
        Wall(parent, "Wall_Corridor_South", Axis.X, a0, a1, IX0, 41.75f, Floor, Ceiling, new List<Opening>
        {
            DoorOpening(Axis.X, RowAWall, 8.5f, a0, a1, "Canteen", Side.High, isDouble: true),
            new Opening(24f, 40f, Floor, Floor + 6f),
        }, _interior, Trim.Both);
        Wall(parent, "Wall_Corridor_South_Gym", Axis.X, a0, a1, 41.75f, 72.25f, Floor, Ceiling,
            new List<Opening> { DoorOpening(Axis.X, RowAWall, 57.5f, a0, a1, "Gym", Side.High, isDouble: true) }, _interior, Trim.Both);
        Wall(parent, "Wall_Corridor_South_East", Axis.X, a0, a1, 72.25f, IX1, Floor, Ceiling, null, _interior, Trim.Both);

        // corridor | north row: a door into each classroom with its sign; the toilets just have the figures on their doors
        var classDoors = new List<Opening>
        {
            DoorOpening(Axis.X, RowCWall, -1f, c0, c1, "101\nMathematics", Side.Low),
            DoorOpening(Axis.X, RowCWall, 15f, c0, c1, "102\nChemistry", Side.Low),
            DoorOpening(Axis.X, RowCWall, 31f, c0, c1, "103\nHistory", Side.Low),
            DoorOpening(Axis.X, RowCWall, 47f, c0, c1, "104\nLiterature", Side.Low),
            DoorOpening(Axis.X, RowCWall, 55f, c0, c1, null, Side.Low, pictogram: "School_Sign_Girl"),
            DoorOpening(Axis.X, RowCWall, 70f, c0, c1, null, Side.Low, pictogram: "School_Sign_Boy"),
        };
        Wall(parent, "Wall_Corridor_North", Axis.X, c0, c1, IX0, IX1, Floor, Ceiling, classDoors, _interior, Trim.Both);

        // north row dividers
        foreach (float x in new[] { 2f, 18f, 34f, 50f, 65f })
            Wall(parent, $"Wall_NorthRow_{x:0}", Axis.Z, x - Int * 0.5f, x + Int * 0.5f, c1, IZ1, Floor, Ceiling, null, _interior, Trim.Both);

        // kitchen | dining room: the serving hatch, and the kitchen door - locked, the kitchen is staff only
        Wall(parent, "Wall_Kitchen_Dining", Axis.Z, -4.25f, -3.75f, IZ0, a0, Floor, Ceiling, new List<Opening>
        {
            new Opening(-42f, -30f, Floor + 1.6f, Floor + 3.8f),
            // the wall ends just past the door, so its sign goes on the left, between the door and the hatch
            DoorOpening(Axis.Z, -4f, -26.5f, -4.25f, -3.75f, "Kitchen\nStaff only", Side.High, locked: true, signOnLeft: true),
        }, _interior, Trim.Both);

        // dining room | hall | gym: double doors from the hall
        Wall(parent, "Wall_Dining_Hall", Axis.Z, 21.75f, 22.25f, IZ0, a0, Floor, Ceiling,
            new List<Opening> { DoorOpening(Axis.Z, 22f, -35f, 21.75f, 22.25f, "Canteen", Side.High, isDouble: true) }, _interior, Trim.Both);
        // the gym's sign on the left, clear of the cloakroom in the hall's corner
        Wall(parent, "Wall_Hall_Gym", Axis.Z, 41.75f, 42.25f, IZ0, a0, Floor, Ceiling,
            new List<Opening> { DoorOpening(Axis.Z, 42f, -35f, 41.75f, 42.25f, "Gym", Side.Low, isDouble: true, signOnLeft: true) }, _interior, Trim.Both);

        // gym | girls' and boys' changing rooms, a door into each from the gym
        Wall(parent, "Wall_Gym_ChangingRooms", Axis.Z, 71.75f, 72.25f, IZ0, a0, Floor, Ceiling, new List<Opening>
        {
            DoorOpening(Axis.Z, 72f, -40f, 71.75f, 72.25f, "Changing Room\nGirls", Side.Low),
            DoorOpening(Axis.Z, 72f, -29.5f, 71.75f, 72.25f, "Changing Room\nBoys", Side.Low),
        }, _interior, Trim.Both);
        Wall(parent, "Wall_ChangingRooms", Axis.X, -35f, -34.5f, 72.25f, IX1, Floor, Ceiling, null, _interior, Trim.Both);

        // the gym is taller: its walls carry on above the rest of the roof, as outside walls
        Box(parent, "Wall_Gym_West_Upper", new Vector3(41.75f, Ceiling, IZ0), new Vector3(42.25f, GymParapet, a0), _exterior);
        Box(parent, "Wall_Gym_East_Upper", new Vector3(71.75f, Ceiling, IZ0), new Vector3(72.25f, GymParapet, a0), _exterior);
        // (the north one is seen from inside the gym, so it's the school's plaster - set by hand)
        Box(parent, "Wall_Gym_North_Upper", new Vector3(41.75f, Ceiling, a0), new Vector3(72.25f, GymParapet, a1), _interior);
    }

    private static Opening DoorOpening(Axis axis, float plane, float center, float t0, float t1,
        string sign = null, Side signSide = Side.Low, bool isDouble = false, bool locked = false, string pictogram = null, bool signOnLeft = false)
    {
        var spot = new DoorSpot
        {
            Axis = axis, Plane = plane, Center = center, T0 = t0, T1 = t1,
            Double = isDouble, Locked = locked, Sign = sign, SignSide = signSide, Pictogram = pictogram, SignOnLeft = signOnLeft,
        };
        Doors.Add(spot);
        return new Opening(center - spot.Width * 0.5f, center + spot.Width * 0.5f, Floor, Floor + DoorHeight);
    }

    // ── Roof ─────────────────────────────────────────────────────────────────
    private static void BuildRoof(Transform parent)
    {
        float a1 = RowAWall + Int * 0.5f, a0 = RowAWall - Int * 0.5f;
        Box(parent, "Roof_North", new Vector3(IX0, Ceiling, a1), new Vector3(IX1, RoofTop, IZ1), _roof);
        Box(parent, "Roof_South_West", new Vector3(IX0, Ceiling, IZ0), new Vector3(41.75f, RoofTop, a1), _roof);
        Box(parent, "Roof_South_East", new Vector3(72.25f, Ceiling, IZ0), new Vector3(IX1, RoofTop, a1), _roof);
        Box(parent, "Roof_Gym", new Vector3(42.25f, GymCeiling, IZ0), new Vector3(71.75f, GymRoofTop, a0), _roof);
    }

    // ── Dining room serving line ─────────────────────────────────────────────
    private static void BuildServingLine(Transform parent)
    {
        // the counter in front of the hatch, with a tray rail; a work counter on the kitchen side
        Box(parent, "Counter", new Vector3(-3.75f, Floor, -42f), new Vector3(-2.45f, Floor + 1.5f, -28f), _counter);
        Box(parent, "Counter_Top", new Vector3(-3.75f, Floor + 1.5f, -42.1f), new Vector3(-2.25f, Floor + 1.62f, -27.9f), _metal);
        Box(parent, "Tray_Rail", new Vector3(-2.25f, Floor + 1.25f, -42.1f), new Vector3(-1.85f, Floor + 1.32f, -27.9f), _metal, collider: false);
        foreach (float z in new[] { -41.5f, -35f, -28.5f })
            Box(parent, $"Tray_Rail_Bracket_{z:0}", new Vector3(-2.45f, Floor + 1.15f, z - 0.05f), new Vector3(-1.85f, Floor + 1.25f, z + 0.05f), _metal, collider: false);
        Box(parent, "Kitchen_Counter", new Vector3(-5.6f, Floor, -42f), new Vector3(-4.25f, Floor + 1.5f, -28f), _counter);
        Box(parent, "Kitchen_Counter_Top", new Vector3(-5.7f, Floor + 1.5f, -42.1f), new Vector3(-4.25f, Floor + 1.62f, -27.9f), _metal);
    }

    // ── Dining room tables ───────────────────────────────────────────────────
    // Long canteen tables with a bench on each side, in rows facing the serving line. Kid-sized like
    // the rest of the school: the table top comes up to the player's chest.
    private static void BuildDiningTables(Transform parent)
    {
        const float tableLength = 5.2f, tableDepth = 1.5f, tableTop = Floor + 1.3f, top = 0.1f;
        const float benchDepth = 0.55f, benchTop = Floor + 0.8f, benchGap = 0.3f;
        int n = 0;
        foreach (float x in new[] { 6f, 15f })
        foreach (float z in new[] { -41.4f, -36.6f, -31.8f })
        {
            var table = Group(parent, $"Table_{++n}");
            float x0 = x - tableLength * 0.5f, x1 = x + tableLength * 0.5f, z0 = z - tableDepth * 0.5f, z1 = z + tableDepth * 0.5f;

            Box(table, "Top", new Vector3(x0, tableTop - top, z0), new Vector3(x1, tableTop, z1), _counter);
            Box(table, "Top_Edge", new Vector3(x0 - 0.02f, tableTop - top - 0.05f, z0 - 0.02f), new Vector3(x1 + 0.02f, tableTop - top, z1 + 0.02f), _metal, collider: false);
            foreach (float lx in new[] { x0 + 0.35f, x1 - 0.45f })
            {
                // a trestle at each end: two legs and a foot rail, the dark metal of the old canteens
                Box(table, "Leg", new Vector3(lx, Floor, z0 + 0.15f), new Vector3(lx + 0.1f, tableTop - top, z0 + 0.25f), _metal);
                Box(table, "Leg", new Vector3(lx, Floor, z1 - 0.25f), new Vector3(lx + 0.1f, tableTop - top, z1 - 0.15f), _metal);
                Box(table, "Foot_Rail", new Vector3(lx, Floor + 0.25f, z0 + 0.25f), new Vector3(lx + 0.1f, Floor + 0.33f, z1 - 0.25f), _metal, collider: false);
            }
            Box(table, "Stretcher", new Vector3(x0 + 0.45f, Floor + 0.25f, z - 0.04f), new Vector3(x1 - 0.45f, Floor + 0.33f, z + 0.04f), _metal, collider: false);

            foreach (float side in new[] { -1f, 1f })
            {
                float bz0 = side < 0 ? z0 - benchGap - benchDepth : z1 + benchGap;
                float bz1 = bz0 + benchDepth;
                var bench = Group(table, side < 0 ? "Bench_South" : "Bench_North");
                var seat = Box(bench, "Seat", new Vector3(x0 + 0.1f, benchTop - 0.08f, bz0), new Vector3(x1 - 0.1f, benchTop, bz1), _counter);
                // to sit on (Look Closer) anywhere along it the kids leave room, facing the table
                GameObjectUtility.SetStaticEditorFlags(seat, 0);
                var places = SchoolInteractables.AlongBench(2.2f, 0f, side < 0 ? 0f : 180f)
                    .Select(p => (new Vector3(p.x, 0.04f + SchoolInteractables.EyeAboveSeat, p.z), p.yaw)).ToArray();
                SchoolInteractables.MakeSeat(seat, places, 0.75f);
                foreach (float lx in new[] { x0 + 0.4f, x1 - 0.5f })
                    Box(bench, "Leg", new Vector3(lx, Floor, bz0 + 0.1f), new Vector3(lx + 0.1f, benchTop - 0.08f, bz1 - 0.1f), _metal);
            }
        }
    }

    // ── Toilets ──────────────────────────────────────────────────────────────
    private static void BuildToilets(Transform parent)
    {
        float rowC1 = RowCWall + Int * 0.5f;
        // cabins against the room's east wall; sinks on the west wall
        BuildToiletRoom(Group(parent, "Toilet_Girls"), westWall: 50.25f, eastWall: 64.75f, rowC1, doorX: 55f);
        BuildToiletRoom(Group(parent, "Toilet_Boys"), westWall: 65.25f, eastWall: IX1, rowC1, doorX: 70f);
    }

    private static void BuildToiletRoom(Transform parent, float westWall, float eastWall, float southWall, float doorX)
    {
        float front = eastWall - CabinDepth;
        float z1 = IZ1;

        var cabinDoorPrefab = CabinDoorPrefabAsset();
        var toilet = SchoolProps.Get("SquatToilet");

        for (int i = 0; i < CabinCount; i++)
        {
            float cz1 = z1 - i * CabinWidth, cz0 = cz1 - CabinWidth, cz = (cz0 + cz1) * 0.5f;
            var cabin = Group(parent, $"Cabin_{i + 1}");

            // side partition (the one on the south side of this cabin); the north wall closes the first
            Box(cabin, "Partition", new Vector3(front, PartitionBottom, cz0 - Partition * 0.5f), new Vector3(eastWall, PartitionTop, cz0 + Partition * 0.5f), _interior);

            // front: panels either side of the door, a header above it
            float d0 = cz - CabinDoorWidth * 0.5f, d1 = cz + CabinDoorWidth * 0.5f;
            Box(cabin, "Front_South", new Vector3(front - Partition * 0.5f, PartitionBottom, cz0), new Vector3(front + Partition * 0.5f, PartitionTop, d0), _interior);
            Box(cabin, "Front_North", new Vector3(front - Partition * 0.5f, PartitionBottom, d1), new Vector3(front + Partition * 0.5f, PartitionTop, cz1), _interior);
            Box(cabin, "Front_Header", new Vector3(front - Partition * 0.5f, Floor + 3.6f, d0), new Vector3(front + Partition * 0.5f, PartitionTop, d1), _interior);

            if (cabinDoorPrefab != null)
            {
                var door = (GameObject)PrefabUtility.InstantiatePrefab(cabinDoorPrefab, cabin);
                door.name = "CabinDoor";
                door.transform.SetPositionAndRotation(new Vector3(front, Floor, cz - CabinLeafCenter), Quaternion.Euler(0f, -90f, 0f));
            }

            // the squat pan in its tiled step, facing the cabin door, the cistern high on the wall behind it
            PlaceProp(cabin, toilet, new Vector3(eastWall - TileDepth, Floor, cz), -90f);
        }

        // a long trough of taps and a mirror over it on the west wall, in front of the tiles
        PlaceProp(parent, SchoolProps.Get("TroughSink"), new Vector3(westWall + TileDepth, Floor, z1 - 6.5f), 90f);
        PlaceProp(parent, SchoolProps.Get("Mirror_5"), new Vector3(westWall + TileDepth, Floor + 2.2f, z1 - 6.5f), 90f);

        TileWalls(parent, westWall, eastWall, southWall, doorX);
    }

    // The walls tiled to above the rail, over the paint, rail and skirting (but not over the door).
    private const float TileDepth = 0.08f, TilesTop = Floor + 2.15f;

    private static void TileWalls(Transform parent, float westWall, float eastWall, float southWall, float doorX)
    {
        const float t0 = TileDepth - 0.015f, t1 = TileDepth;
        float north = IZ1;
        float d0 = doorX - DoorWidth * 0.5f - 0.2f, d1 = doorX + DoorWidth * 0.5f + 0.2f;
        Box(parent, "Tiles_West", new Vector3(westWall + t0, Floor, southWall), new Vector3(westWall + t1, TilesTop, north), _smallTiles, collider: false);
        Box(parent, "Tiles_East", new Vector3(eastWall - t1, Floor, southWall), new Vector3(eastWall - t0, TilesTop, north), _smallTiles, collider: false);
        Box(parent, "Tiles_North", new Vector3(westWall, Floor, north - t1), new Vector3(eastWall, TilesTop, north - t0), _smallTiles, collider: false);
        Box(parent, "Tiles_South_West", new Vector3(westWall, Floor, southWall + t0), new Vector3(d0, TilesTop, southWall + t1), _smallTiles, collider: false);
        Box(parent, "Tiles_South_East", new Vector3(d1, Floor, southWall + t0), new Vector3(eastWall, TilesTop, southWall + t1), _smallTiles, collider: false);
    }

    private static GameObject PlaceProp(Transform parent, GameObject prefab, Vector3 position, float yaw)
    {
        if (prefab == null)
            return null;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, _static);
        return go;
    }

    // The cabin door: a variant of the game's door with a smaller, plain leaf, hinged at its root on the floor.
    private static GameObject CabinDoorPrefabAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CabinDoorPrefab);
        if (existing != null)
            return existing;

        var source = AssetDatabase.LoadAssetAtPath<GameObject>(DoorPrefab);
        if (source == null)
            return null;

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = "ToiletCabinDoor";
        instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var leaf = instance.GetComponentInChildren<DoorInteractable>(true).transform;
        leaf.name = "[Interactable]CabinDoor";
        leaf.localScale = new Vector3(1.8f, 3.3f, 0.05f);
        leaf.localPosition = new Vector3(CabinLeafCenter, 0.25f + 3.3f * 0.5f, 0f);
        foreach (var sprite in instance.GetComponentsInChildren<SpriteRenderer>(true))
            sprite.gameObject.SetActive(false);

        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, CabinDoorPrefab);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    // Empty markers in the middle of each room, to find your way round the hierarchy while furnishing.
    private static void PlaceRoomMarkers(Transform parent)
    {
        void Marker(string name, float x0, float z0, float x1, float z1)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3((x0 + x1) * 0.5f, Floor, (z0 + z1) * 0.5f);
        }

        Marker("Room_EntranceHall", 22.25f, IZ0, 41.75f, -25f);
        Marker("Room_DiningRoom", -3.75f, IZ0, 21.75f, -25f);
        Marker("Room_Kitchen", IX0, IZ0, -4.25f, -25f);
        Marker("Room_Gym", 42.25f, IZ0, 71.75f, -25f);
        Marker("Room_ChangingRoom_Girls", 72.25f, IZ0, IX1, -35f);
        Marker("Room_ChangingRoom_Boys", 72.25f, -34.5f, IX1, -25f);
        Marker("Room_Corridor", IX0, -24.5f, IX1, -16.5f);
        Marker("Room_Classroom_1", IX0, -16f, 1.75f, IZ1);
        Marker("Room_Classroom_2", 2.25f, -16f, 17.75f, IZ1);
        Marker("Room_Classroom_3", 18.25f, -16f, 33.75f, IZ1);
        Marker("Room_Classroom_4", 34.25f, -16f, 49.75f, IZ1);
        Marker("Room_Toilet_Girls", 50.25f, -16f, 64.75f, IZ1);
        Marker("Room_Toilet_Boys", 65.25f, -16f, IX1, IZ1);
    }

    // ── Building blocks ──────────────────────────────────────────────────────

    /// <summary>
    /// A wall running along <paramref name="axis"/> from <paramref name="from"/> to <paramref name="to"/>,
    /// <paramref name="t0"/>..<paramref name="t1"/> thick on the other horizontal axis, with rectangular
    /// openings cut out of it (positions along the wall, heights in world y).
    /// </summary>
    private static void Wall(Transform parent, string name, Axis axis, float t0, float t1, float from, float to, float bottom, float top,
        List<Opening> openings, Material material, Trim trim = Trim.None)
    {
        var group = Group(parent, name);
        var cuts = openings != null ? new List<Opening>(openings) : new List<Opening>();
        cuts.Sort((a, b) => a.From.CompareTo(b.From));

        int piece = 0;
        float cursor = from;
        foreach (var o in cuts)
        {
            if (o.From > cursor + 0.001f)
            {
                WallBox(group, $"{name}_{piece++}", axis, t0, t1, cursor, o.From, bottom, top, material);
                WallTrim(group, axis, t0, t1, cursor, o.From, top, trim);
            }
            if (o.Bottom > bottom + 0.001f)
            {
                WallBox(group, $"{name}_{piece++}_Below", axis, t0, t1, o.From, o.To, bottom, o.Bottom, material);
                WallTrim(group, axis, t0, t1, o.From, o.To, o.Bottom, trim);
            }
            if (o.Top < top - 0.001f)
                WallBox(group, $"{name}_{piece++}_Above", axis, t0, t1, o.From, o.To, o.Top, top, material);
            cursor = Mathf.Max(cursor, o.To);
        }
        if (to > cursor + 0.001f)
        {
            WallBox(group, $"{name}_{piece}", axis, t0, t1, cursor, to, bottom, top, material);
            WallTrim(group, axis, t0, t1, cursor, to, top, trim);
        }
    }

    // The old school wall: the lower part painted a darker shade up to a rail, with a skirting board at the floor.
    private static void WallTrim(Transform parent, Axis axis, float t0, float t1, float u0, float u1, float pieceTop, Trim trim)
    {
        if (trim == Trim.None || pieceTop <= Floor + 0.01f)
            return;

        void Face(float face, float outward)
        {
            float paintTop = Mathf.Min(DadoY, pieceTop);
            WallBox(parent, "Trim_Paint", axis, Mathf.Min(face, face + outward * 0.015f), Mathf.Max(face, face + outward * 0.015f),
                u0, u1, Floor, paintTop, _paint, collider: false);
            WallBox(parent, "Trim_Skirting", axis, Mathf.Min(face, face + outward * 0.05f), Mathf.Max(face, face + outward * 0.05f),
                u0, u1, Floor, Mathf.Min(Floor + 0.3f, pieceTop), _trim, collider: false);
            if (pieceTop >= DadoY + 0.12f)
                WallBox(parent, "Trim_Rail", axis, Mathf.Min(face, face + outward * 0.06f), Mathf.Max(face, face + outward * 0.06f),
                    u0, u1, DadoY, DadoY + 0.12f, _trim, collider: false);
        }

        if (trim == Trim.Low || trim == Trim.Both)
            Face(t0, -1f);
        if (trim == Trim.High || trim == Trim.Both)
            Face(t1, 1f);
    }

    // ── Relief ───────────────────────────────────────────────────────────────
    // The plain concrete ceiling beams of a typical Soviet school - no columns or pilasters, the beams
    // span from wall to wall: across the corridor every 8 m or so, in a grid over the entrance hall;
    // a simple cornice round the ceilings, two ribs along the corridor's.
    private static void BuildRelief(Transform parent)
    {
        float corrSouth = RowAWall + Int * 0.5f, corrNorth = RowCWall - Int * 0.5f;
        float beamBottom = Ceiling - 0.7f;

        var corridor = Group(parent, "Corridor");
        foreach (float x in new[] { -9f, 3f, 12f, 20f, 27f, 35f, 43f, 51f, 63f, 75f })
            Box(corridor, $"Beam_{x:0}", new Vector3(x - 0.4f, beamBottom, corrSouth), new Vector3(x + 0.4f, Ceiling, corrNorth), _interior, collider: false);
        foreach (float z in new[] { -22f, -19f })
            Box(corridor, "Rib", new Vector3(IX0, Ceiling - 0.35f, z - 0.2f), new Vector3(IX1, Ceiling, z + 0.2f), _interior, collider: false);
        Box(corridor, "Cornice_North", new Vector3(IX0, Ceiling - 0.35f, corrNorth - 0.25f), new Vector3(IX1, Ceiling, corrNorth), _interior, collider: false);
        Box(corridor, "Cornice_South", new Vector3(IX0, Ceiling - 0.35f, corrSouth), new Vector3(IX1, Ceiling, corrSouth + 0.25f), _interior, collider: false);

        var hall = Group(parent, "EntranceHall");
        float hx0 = 22.25f, hx1 = 41.75f, hz0 = IZ0, hz1 = RowAWall - Int * 0.5f;
        float hallBeam = Ceiling - 0.8f;
        foreach (float z in new[] { -39f, -31f })
            Box(hall, $"Beam_Z{z:0}", new Vector3(hx0, hallBeam, z - 0.45f), new Vector3(hx1, Ceiling, z + 0.45f), _interior, collider: false);
        foreach (float x in new[] { 26.5f, 37.5f })
            Box(hall, $"Beam_X{x:0}", new Vector3(x - 0.45f, hallBeam, hz0), new Vector3(x + 0.45f, Ceiling, hz1), _interior, collider: false);
        Box(hall, "Cornice_South", new Vector3(hx0, Ceiling - 0.35f, hz0), new Vector3(hx1, Ceiling, hz0 + 0.25f), _interior, collider: false);
        Box(hall, "Cornice_North", new Vector3(hx0, Ceiling - 0.35f, hz1 - 0.25f), new Vector3(hx1, Ceiling, hz1), _interior, collider: false);
        Box(hall, "Cornice_West", new Vector3(hx0, Ceiling - 0.35f, hz0), new Vector3(hx0 + 0.25f, Ceiling, hz1), _interior, collider: false);
        Box(hall, "Cornice_East", new Vector3(hx1 - 0.25f, Ceiling - 0.35f, hz0), new Vector3(hx1, Ceiling, hz1), _interior, collider: false);
    }

    private static GameObject WallBox(Transform parent, string name, Axis axis, float t0, float t1, float u0, float u1, float y0, float y1,
        Material material, bool collider = true)
    {
        return axis == Axis.X
            ? Box(parent, name, new Vector3(u0, y0, t0), new Vector3(u1, y1, t1), material, collider)
            : Box(parent, name, new Vector3(t0, y0, u0), new Vector3(t1, y1, u1), material, collider);
    }

    // The Soviet double window: two white-painted wooden frames, one near each face of the thick wall,
    // each with a centre post, a transom and a fortochka (the little vent pane) in its top corner; a deep
    // painted sill inside, and the cast-iron radiator under it. The glass stops the player.
    private static void BuildWindow(Transform parent, Axis axis, float plane, float center, float width, float sill, float top)
    {
        var group = Group(parent, $"Window_{(axis == Axis.X ? "X" : "Z")}{center:0.#}_{plane:0.#}");
        float inward = Inward(axis, plane);
        float u0 = center - width * 0.5f, u1 = center + width * 0.5f;

        Sash(group, "Outer", axis, plane - inward * 0.3f, center, width, sill, top);
        Sash(group, "Inner", axis, plane + inward * 0.3f, center, width, sill, top);

        float face = plane + inward * Ext * 0.5f;
        float s0 = plane + inward * 0.22f, s1 = face + inward * 0.25f;
        WallBox(group, "Sill", axis, Mathf.Min(s0, s1), Mathf.Max(s0, s1), u0 - 0.08f, u1 + 0.08f, sill, sill + 0.06f, _windowPaint, collider: false);

        // see-through glass casts no shadow in URP, so the sun would throw a hard white block of light on
        // the floor under every window; an invisible pane casts the shadow instead
        const float bar = 0.17f;
        var shade = WallBox(group, "Glass_Shadow", axis, plane - 0.01f, plane + 0.01f, u0 + bar, u1 - bar, sill + bar, top - bar, _frame, collider: false);
        shade.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        // the radiator on the floor under it, against the inside face of the wall
        var radiator = SchoolProps.Get(width >= 3.5f ? "Radiator_16" : "Radiator_10");
        float yaw = axis == Axis.X ? (inward > 0f ? 0f : 180f) : (inward > 0f ? 90f : -90f);
        PlaceProp(group, radiator, axis == Axis.X ? new Vector3(center, Floor, face) : new Vector3(face, Floor, center), yaw);
    }

    private static void Sash(Transform parent, string name, Axis axis, float t, float center, float width, float sill, float top)
    {
        const float bar = 0.17f, depth = 0.12f, post = 0.1f;
        float u0 = center - width * 0.5f, u1 = center + width * 0.5f;
        float t0 = t - depth * 0.5f, t1 = t + depth * 0.5f;
        var group = Group(parent, name);

        WallBox(group, "Frame_Left", axis, t0, t1, u0, u0 + bar, sill, top, _windowPaint);
        WallBox(group, "Frame_Right", axis, t0, t1, u1 - bar, u1, sill, top, _windowPaint);
        WallBox(group, "Frame_Bottom", axis, t0, t1, u0 + bar, u1 - bar, sill, sill + bar, _windowPaint);
        WallBox(group, "Frame_Top", axis, t0, t1, u0 + bar, u1 - bar, top - bar, top, _windowPaint);
        bool wide = width > 2.5f;
        if (wide)
            WallBox(group, "Frame_Middle", axis, t0, t1, center - bar * 0.5f, center + bar * 0.5f, sill + bar, top - bar, _windowPaint);
        float transom = Mathf.Lerp(sill, top, 0.7f);
        WallBox(group, "Frame_Transom", axis, t0, t1, u0 + bar, u1 - bar, transom - bar * 0.5f, transom + bar * 0.5f, _windowPaint);
        // the fortochka: a small opening pane at the top of the left leaf, under the transom
        float ventRight = wide ? (u0 + center) * 0.5f : u0 + width * 0.5f;
        float ventBottom = Mathf.Lerp(sill, transom, 0.55f);
        WallBox(group, "Fortochka_Side", axis, t0, t1, ventRight - post * 0.5f, ventRight + post * 0.5f, ventBottom, transom - bar * 0.5f, _windowPaint);
        WallBox(group, "Fortochka_Bottom", axis, t0, t1, u0 + bar, ventRight, ventBottom - post * 0.5f, ventBottom + post * 0.5f, _windowPaint);

        var glass = WallBox(group, "Glass", axis, t - 0.01f, t + 0.01f, u0 + bar, u1 - bar, sill + bar, top - bar, _glass);
        GameObjectUtility.SetStaticEditorFlags(glass, _static & ~StaticEditorFlags.ContributeGI);
    }

    // which way is into the building from an outside wall's plane (+1 / -1 along the wall's thickness axis)
    private static float Inward(Axis axis, float plane) =>
        axis == Axis.X ? (plane < (BZ0 + BZ1) * 0.5f ? 1f : -1f) : (plane < (BX0 + BX1) * 0.5f ? 1f : -1f);

    // The game's door (or a pair of them) in a doorway cut to its leaves, with a dark frame round it on
    // both sides of the wall, and the room's sign beside it.
    private static void PlaceDoor(Transform parent, DoorSpot spot)
    {
        var group = Group(parent, $"Door_{(spot.Axis == Axis.X ? "X" : "Z")}{spot.Center:0.#}_{spot.Plane:0.#}");

        if (spot.Double)
        {
            // two leaves meeting in the middle, the right one mirrored so it hinges on the right
            PlaceLeaf(group, "Door_Left", spot, spot.Center - DoorWidth * 0.5f - DoorLeafCenter, mirrored: false);
            PlaceLeaf(group, "Door_Right", spot, spot.Center + DoorWidth * 0.5f + DoorLeafCenter, mirrored: true);
        }
        else
        {
            PlaceLeaf(group, "Door", spot, spot.Center - DoorLeafCenter, mirrored: false);
        }

        // casing: jambs and a head on each face, just outside the opening
        const float trim = 0.14f, thick = 0.05f;
        float u0 = spot.Center - spot.Width * 0.5f, u1 = spot.Center + spot.Width * 0.5f, top = Floor + DoorHeight;
        foreach (var (f0, f1) in new[] { (spot.T0 - thick, spot.T0), (spot.T1, spot.T1 + thick) })
        {
            WallBox(group, "Casing_Left", spot.Axis, f0, f1, u0 - trim, u0, Floor, top + trim, _trim, collider: false);
            WallBox(group, "Casing_Right", spot.Axis, f0, f1, u1, u1 + trim, Floor, top + trim, _trim, collider: false);
            WallBox(group, "Casing_Head", spot.Axis, f0, f1, u0, u1, top, top + trim, _trim, collider: false);
        }

        if (!string.IsNullOrEmpty(spot.Sign))
        {
            // beside the door at eye height, to the reader's right (or left where the wall runs out on the right)
            float face = spot.SignSide == Side.Low ? spot.T0 - 0.03f : spot.T1 + 0.03f;
            var facing = SignFacing(spot.Axis, spot.SignSide);
            float sideways = spot.Width * 0.5f + trim + 0.3f + SignWidth * 0.5f;
            Vector3 right = facing * Vector3.right * (spot.SignOnLeft ? -1f : 1f);
            var center = spot.Axis == Axis.X ? new Vector3(spot.Center, SignY, face) : new Vector3(face, SignY, spot.Center);
            PlaceSign(group, spot.Sign, center + right * sideways, facing);
        }
    }

    // A sign is read by someone standing on its side of the wall, looking at the wall.
    private static Quaternion SignFacing(Axis axis, Side side)
    {
        if (axis == Axis.X)
            return Quaternion.Euler(0f, side == Side.Low ? 0f : 180f, 0f);
        return Quaternion.Euler(0f, side == Side.Low ? 90f : -90f, 0f);
    }

    private static void PlaceLeaf(Transform parent, string name, DoorSpot spot, float hinge, bool mirrored)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DoorPrefab);
        if (prefab == null)
            return;

        var door = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        door.name = name;
        float yaw = (spot.Axis == Axis.X ? 0f : -90f) + (mirrored ? 180f : 0f);
        var position = spot.Axis == Axis.X
            ? new Vector3(hinge, Floor + DoorPivotAboveFloor, spot.Plane)
            : new Vector3(spot.Plane, Floor + DoorPivotAboveFloor, hinge);
        door.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DoorSprite);
        if (sprite != null)
        {
            foreach (var sr in door.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.sprite = sprite;
                // the drawing has its handle on the left; mirror it on a face where that would be the hinge side
                Vector3 handle = sr.transform.position - sr.transform.right * 0.5f;
                Vector3 freeEdge = sr.transform.position + sr.transform.right * 0.5f;
                sr.flipX = Vector3.Distance(handle, door.transform.position) < Vector3.Distance(freeEdge, door.transform.position);
            }
        }

        var leaf = door.GetComponentInChildren<DoorInteractable>(true);
        if (spot.Locked && leaf != null)
            leaf.gameObject.AddComponent<LockedDoor>();

        if (!string.IsNullOrEmpty(spot.Pictogram) && leaf != null)
            AddPictogram(leaf.transform, spot);
    }

    // The boy / girl figure on a toilet door, on the side the sign is read from. A child of the leaf, so it swings with it.
    private static void AddPictogram(Transform leaf, DoorSpot spot)
    {
        var sprite = SignSprite(spot.Pictogram);
        if (sprite == null)
            return;

        // which of the leaf's faces points to the sign's side of the wall
        Vector3 towardsSign = (spot.Axis == Axis.X ? Vector3.forward : Vector3.right) * (spot.SignSide == Side.Low ? -1f : 1f);
        float face = Vector3.Dot(leaf.forward, towardsSign) > 0f ? 1f : -1f;

        // copy the facing of the door's own drawing on that face
        Quaternion facing = Quaternion.identity;
        foreach (var existing in leaf.GetComponentsInChildren<SpriteRenderer>(true))
            if (Mathf.Sign(existing.transform.localPosition.z) == face)
                facing = existing.transform.localRotation;

        var go = new GameObject("Pictogram_" + spot.Pictogram);
        go.transform.SetParent(leaf, false);
        const float size = 1.15f, raise = 0.72f;
        Vector3 leafScale = leaf.localScale;
        go.transform.localPosition = new Vector3(0f, raise / leafScale.y, 1.3f * face);
        go.transform.localRotation = facing;
        Vector2 native = sprite.bounds.size;
        go.transform.localScale = new Vector3(size / native.x / leafScale.x, size / native.y / leafScale.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;
    }

    // A doodled plate with the handwritten room name on it.
    private static void PlaceSign(Transform parent, string text, Vector3 position, Quaternion facing)
    {
        var sign = new GameObject("Sign").transform;
        sign.SetParent(parent, false);
        sign.SetPositionAndRotation(position, facing);

        var plate = SignSprite("School_Sign_Plate");
        if (plate != null)
        {
            var go = new GameObject("Plate");
            go.transform.SetParent(sign, false);
            Vector2 native = plate.bounds.size;
            go.transform.localScale = new Vector3(SignWidth / native.x, SignHeight / native.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = plate;
        }

        var label = new GameObject("Text");
        label.transform.SetParent(sign, false);
        label.transform.localPosition = new Vector3(0f, 0f, -0.01f);   // just in front of the plate, towards the reader
        var tmp = label.AddComponent<TMPro.TextMeshPro>();
        tmp.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(SignFont);
        tmp.text = text;
        tmp.color = Color.black;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.5f;
        tmp.fontSizeMax = 12f;
        tmp.lineSpacing = -25f;
        tmp.rectTransform.sizeDelta = new Vector2(SignWidth * 0.84f, SignHeight * 0.8f);
        tmp.GetComponent<MeshRenderer>().sortingOrder = 2;
    }

    private static Sprite SignSprite(string name)
    {
        string path = SignFolder + name + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return null;
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    /// <summary>A box from <paramref name="min"/> to <paramref name="max"/> (world), as a ProBuilder cube like the rest of the game's walls.</summary>
    private static GameObject Box(Transform parent, string name, Vector3 min, Vector3 max, Material material, bool collider = true)
    {
        Vector3 size = max - min;
        var pb = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
        var go = pb.gameObject;
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = (min + max) * 0.5f;
        pb.SetMaterial(pb.faces, material);
        pb.ToMesh();
        pb.Refresh();
        // ProBuilder leaves an empty second submesh behind, and the outline (on seats) would draw on that
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        if (mesh.subMeshCount > 1)
            mesh.subMeshCount = 1;

        if (collider)
        {
            var box = go.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = size;
        }

        GameObjectUtility.SetStaticEditorFlags(go, _static);
        return go;
    }

    private static Transform Group(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void AddSurface(GameObject target, Component surface)
    {
        if (surface == null)
            return;
        ComponentUtility.CopyComponent(surface);
        ComponentUtility.PasteComponentAsNew(target);
    }

    // ── Setup ────────────────────────────────────────────────────────────────

    private static GameObject FindOrCreateLevel()
    {
        var scene = EditorSceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == LevelName)
                return root;

        var level = new GameObject(LevelName);
        level.AddComponent<Level09>();
        Undo.RegisterCreatedObjectUndo(level, "Build School");
        return level;
    }

    private static void LoadMaterials()
    {
        Material Mat(string path) => AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/" + path + ".mat");
        _exterior = Mat("clean-pale-concrete-stucco-pattern 1");   // the yard walls
        _interior = SchoolTextures.Get("Plaster", 3f, 0.05f);      // whitewashed plaster walls
        _roof = _interior;                                           // and ceilings
        _counter = SchoolTextures.Get("Laminate", 1.5f, 0.3f);     // counters, table tops, benches
        _paint = SchoolTextures.Get("WallPaint", 2.4f, 0.3f);   // the lower walls' glossy oil paint
        _trim = SchoolTextures.Get("Trim", 1.2f, 0.35f);        // wall rail and skirting: worn dark gray paint, softer than the black window frames
        _frame = Mat("DarkGray 4");                                  // window frames and door casings, like the game's ink lines
        _metal = SchoolTextures.Get("Metal", 1f, 0.45f);             // table legs, counter tops, the tray rail
        _glass = GlassMaterial();

        _terrazzo = SchoolTextures.Get("Terrazzo", 2.4f, 0.4f);
        _windowPaint = SchoolTextures.Get("WindowPaint", 1.2f, 0.45f);   // window frames and sills, like the radiators
        _boards = SchoolTextures.Get("Boards", 2.4f, 0.2f);
        _gym = SchoolTextures.Get("Gym", 2.4f, 0.4f);
        _smallTiles = SchoolTextures.Get("SmallTiles", 2.4f, 0.5f);

        // footstep surfaces: the stairwell's floor inside
        _outdoorSurface = FindSurface("Level_08/Street/StaticEnvironment/Walls/WallOfExit (1)");
        _indoorSurface = FindSurface("Level_07/ToDestroy/StaticEnvironment/Walls/WallOfExit (2)") ?? _outdoorSurface;

        var reference = GameObject.Find("Level_08/Street/StaticEnvironment/Walls/WallOfExit (13)");
        _static = reference != null ? GameObjectUtility.GetStaticEditorFlags(reference) : (StaticEditorFlags)~0;
    }

    private static Component FindSurface(string path)
    {
        var go = GameObject.Find(path);
        return go != null ? go.GetComponent("FSR_SimpleSurface") : null;
    }

    // Pale, see-through glass that suits the black-and-white look.
    private static Material GlassMaterial()
    {
        const string path = "Assets/_Project/Materials/School/School_Glass.mat";
        var glass = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (glass != null)
            return glass;

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials/School"))
            AssetDatabase.CreateFolder("Assets/_Project/Materials", "School");

        glass = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "School_Glass" };
        glass.SetFloat("_Surface", 1f);   // transparent
        glass.SetFloat("_Blend", 0f);     // alpha
        glass.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glass.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glass.SetFloat("_ZWrite", 0f);
        glass.SetColor("_BaseColor", new Color(0.92f, 0.92f, 0.92f, 0.22f));
        glass.SetFloat("_Smoothness", 0.85f);
        glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        glass.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        AssetDatabase.CreateAsset(glass, path);
        return glass;
    }
}
#endif
