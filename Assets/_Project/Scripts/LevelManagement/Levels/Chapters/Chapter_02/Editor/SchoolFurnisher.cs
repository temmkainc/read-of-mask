// Tools → Read of Mask → Furnish School (Level 09)
// Fills the school (SchoolBuilder's rooms) with SchoolProps' furniture, the way a post-Soviet school
// looked in 1999 - and on the date on every blackboard, Thursday the 14th of October:
//   classrooms:  rows of two-seat desks facing a three-panel blackboard on the teacher's platform,
//                portraits above it, glass-fronted bookcases at the back, stands on the walls,
//                curtains and pot plants on the sills; each room with its subject's things
//   corridor:    benches between the doors, wall newspapers and the timetable over them, ficus tubs and
//                plants on wire stands, a drinking fountain, the fire shield, a clock over the way into the hall
//   hall:        the cloakroom (coats behind a barred counter) and a long mirror, the watchman's desk and
//                the cabinet of cups by the entrance; in the middle a carpet runner up to a faceless bust in
//                the children's mask, a planter, rows of hall seats and a table-tennis table; the honor
//                board, the rules, the state symbols, "learning is light" over the doors
//
// The drawings on the walls are SchoolDrawings' PNGs, one per placement, kept through rebuilds - paint
// your own over them. The words (headings, chalk, menu, slogans) are the strings below.
//   canteen:     places laid on every table, trays and the kitchen's pots, the menu on a blackboard,
//                a long trough of taps to wash your hands, "bread is the head of everything"
//   gym:         wall bars, backboards, benches, mats, the vaulting horse, ropes, the court's lines
//   and the changing rooms' benches and hooks; lamps everywhere.
// (Radiators, windows, toilets and sinks are SchoolBuilder's.)
//
// Everything lives under Level_09/Furniture and is rebuilt from scratch on every run (SchoolBuilder
// runs it at the end of a build), so move anything you add by hand out of there first.

#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SchoolFurnisher
{
    private const string LevelName = "Level_09";
    private const string GroupName = "Furniture";
    private const string SignFont = "Assets/TextMesh Pro/Resources/Fonts & Materials/Inkfree SDF.asset";
    private const string SchedulePath = "Assets/_Project/Sprites/Props/School_Schedule.png";
    private const string PlatePath = "Assets/_Project/Sprites/School/School_Sign_Plate.png";

    // SchoolBuilder's measurements
    private const float Floor = 0.432f, Ceiling = Floor + 7f, GymCeiling = Floor + 12f;
    private const float IX0 = -14f, IX1 = 80f, IZ0 = -44.78f, IZ1 = 3.22f;
    private const float SillY = Floor + 2.2f, SillTop = SillY + 0.06f;   // the window openings' bottom, the sill board's top
    private const float CorridorNorth = -16.5f, CorridorSouth = -24.5f, RowA = -25f, RowC = -16f;

    private static readonly Color Chalk = new(0.93f, 0.93f, 0.93f);
    private static readonly Color Ink = new(0.08f, 0.08f, 0.08f);
    private const string BoardDate = "Thursday, the fourteenth of October";

    private static Transform _root;
    private static StaticEditorFlags _static;
    private static TMP_FontAsset _font;

    [MenuItem("Tools/Read of Mask/Furnish School (Level 09)")]
    public static void FurnishMenu()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[SchoolFurnisher] Leave Play mode first.");
            return;
        }
        var level = GameObject.Find(LevelName);
        if (level == null)
        {
            Debug.LogWarning("[SchoolFurnisher] No Level_09 in the open scene - build the school first.");
            return;
        }
        SchoolProps.BeginRun();
        Furnish(level);
    }

    public static void Furnish(GameObject level)
    {
        var old = level.transform.Find(GroupName);
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);
        _root = new GameObject(GroupName).transform;
        _root.SetParent(level.transform, false);
        Undo.RegisterCreatedObjectUndo(_root.gameObject, "Furnish School");

        var reference = GameObject.Find("Level_08/Street/StaticEnvironment/Walls/WallOfExit (13)");
        _static = reference != null ? GameObjectUtility.GetStaticEditorFlags(reference) : (StaticEditorFlags)~0;
        _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SignFont);

        Classrooms(Group("Classrooms"));
        Corridor(Group("Corridor"));
        Hall(Group("EntranceHall"));
        Canteen(Group("DiningRoom"));
        Kitchen(Group("Kitchen"));
        Gym(Group("Gym"));
        ChangingRooms(Group("ChangingRooms"));
        Toilets(Group("Toilets"));

        // nobody sits where a kid already does (the canteen benches are the builder's, so it covers those too)
        SchoolInteractables.MarkTakenSeats(level.transform);

        EditorSceneManager.MarkSceneDirty(level.scene);
        Debug.Log($"[SchoolFurnisher] Furnished the school under {LevelName}/{GroupName}.");
    }

    // ── Classrooms ───────────────────────────────────────────────────────────

    private struct Classroom
    {
        public string Number, Subject, Chalk;
        public float X0, X1;
        public float[] Windows;
    }

    private static void Classrooms(Transform parent)
    {
        var rooms = new[]
        {
            new Classroom { Number = "101", Subject = "Mathematics", X0 = IX0, X1 = 1.75f, Windows = new[] { -10.5f, -6f, -1.5f },
                Chalk = "Homework: No. 214, 217\n\n2x + 3 = 11\nx = 4" },
            new Classroom { Number = "102", Subject = "Chemistry", X0 = 2.25f, X1 = 17.75f, Windows = new[] { 5.5f, 10f, 14.5f },
                Chalk = "Laboratory work No. 3\n\n2H2 + O2 = 2H2O" },
            new Classroom { Number = "103", Subject = "History", X0 = 18.25f, X1 = 33.75f, Windows = new[] { 21.5f, 26f, 30.5f },
                Chalk = "The Middle Ages\n\nHomework: p. 112, questions 1-4" },
            new Classroom { Number = "104", Subject = "Literature", X0 = 34.25f, X1 = 49.75f, Windows = new[] { 37.5f, 42f, 46.5f },
                Chalk = "Essay\n\"My Face\"" },
        };

        foreach (var room in rooms)
        {
            var group = Group(parent, $"Classroom_{room.Number}_{room.Subject}");
            float x0 = room.X0, x1 = room.X1, width = x1 - x0, cz = (RowC + IZ1) * 0.5f;

            // the front: the blackboard on the east wall, the platform along it, the teacher's desk by the windows
            var board = Put(group, "Chalkboard", new Vector3(x1, Floor + 1.55f, cz), -90f);
            if (board != null)
            {
                Label(board.transform, room.Chalk, new Vector3(0f, 0.85f, 0.09f), 4.1f, 1.45f, Chalk, 2.2f);
                Label(board.transform, BoardDate, new Vector3(0f, 1.72f, 0.09f), 4.1f, 0.25f, Chalk, 1.2f);
            }
            Put(group, "TeacherPlatform", new Vector3(x1, Floor, cz), -90f);
            Put(group, "TeacherDesk", new Vector3(x1 - 1.4f, Floor + 0.22f, -1.3f), -90f);

            // four rows of three desks, facing the board; in History the chairs are already up on the desks
            string desk = room.Subject == "History" ? "DeskPair_Upturned" : "DeskPair";
            for (int row = 0; row < 4; row++)
            foreach (float z in new[] { -1.6f, -6.4f, -11.2f })
                Put(group, desk, new Vector3(x1 - 5.3f - row * 2.2f, Floor, z), 90f);

            // above the board: portraits - or, in Chemistry, the periodic table
            if (room.Subject == "Chemistry")
            {
                var table = Put(group, "Board_5.2x3", new Vector3(x1, Floor + 3.85f, cz), -90f);
                if (table != null)
                    Drawing(table.transform, SchoolDrawings.Get("PeriodicTable", $"{Place(group)}_PeriodicTable"), new Vector3(0f, 1.5f, 0.07f), new Vector2(4.95f, 2.85f));
            }
            else
            {
                float[] along = { -3.2f, -1.6f, 0f, 1.6f, 3.2f };
                for (int i = 0; i < along.Length; i++)
                {
                    var frame = Put(group, "PictureFrame", new Vector3(x1, Floor + 4.55f, cz + along[i]), -90f);
                    if (frame != null)
                        Drawing(frame.transform, SchoolDrawings.Get($"Portrait_{1 + (i + room.Number[2]) % 4}", $"{Place(group)}_Portrait_{i + 1}"),
                            new Vector3(0f, 0.575f, 0.055f), new Vector2(0.78f, 1f));
                }
            }

            // the back wall: bookcases
            foreach (float z in new[] { -14.2f, -12.6f, -11f })
                Put(group, z < -12f ? "Bookcase" : "Bookcase_2", new Vector3(x0, Floor, z), 90f);
            foreach (float z in new[] { 0f, 1.6f })
                Put(group, "Bookcase_2", new Vector3(x0, Floor, z), 90f);

            // the side wall by the corridor: two stands
            Stand(group, new Vector3(x0 + 3f, Floor + 2.6f, RowC), 0f,
                room.Subject == "Mathematics" ? "Geometry" : room.Subject == "History" ? "Our Land" : "Class Corner",
                room.Subject == "Mathematics" ? new[] { "Geometry", "Sheet_2" } : new[] { "Sheet_1", "Sheet_3" });
            if (room.Subject == "History")
            {
                var map = Put(group, "Board_5.2x3", new Vector3(x0 + 8.6f, Floor + 2.3f, RowC), 0f);
                if (map != null)
                    Drawing(map.transform, SchoolDrawings.Get("Map", $"{Place(group)}_Map"), new Vector3(0f, 1.5f, 0.07f), new Vector2(4.95f, 2.85f));
                Put(group, "Globe", new Vector3(x1 - 1.4f, Floor + 0.22f + 1.25f, -0.6f), 0f);
            }
            else
            {
                Stand(group, new Vector3(x0 + 8f, Floor + 2.6f, RowC), 0f,
                    room.Subject == "Literature" ? "Reading List" : "Duty Roster", new[] { "Sheet_2", "Sheet_1" });
            }

            // the windows: curtains, a pot plant on every sill
            for (int i = 0; i < room.Windows.Length; i++)
            {
                float wx = room.Windows[i];
                Put(group, "Curtains", new Vector3(wx, SillY - 0.3f, IZ1), 180f);
                Put(group, i % 2 == 0 ? "Plant_Small" : "Plant_Spiky", new Vector3(wx + (i % 2 == 0 ? -0.9f : 0.8f), SillTop, IZ1 - 0.05f), 0f);
            }
            if (room.Number == "101")
            {
                // the corner room also has a window in the west wall
                Put(group, "Curtains", new Vector3(IX0, SillY - 0.3f, -6f), 90f);
                Put(group, "Plant_Small", new Vector3(IX0 + 0.05f, SillTop, -6.9f), 0f);
            }

            foreach (float fx in new[] { 0.2f, 0.5f, 0.8f })
            foreach (float z in new[] { -12f, -6.4f, -0.8f })
                Put(group, "Lamp_Tube", new Vector3(x0 + width * fx, Ceiling, z), 0f);
        }
    }

    // ── Corridor ─────────────────────────────────────────────────────────────

    private static void Corridor(Transform parent)
    {
        // between the ceiling beams
        foreach (float x in new[] { -11.5f, -3f, 7.5f, 16f, 23.5f, 31f, 39f, 47f, 57f, 69f, 77.5f })
            Put(parent, "Lamp_Tube", new Vector3(x, Ceiling, -20.5f), 0f);

        // benches between the doors and pilasters, and the stands above them
        var north = new (float x, string header, string[] sheets)[]
        {
            (7f, "School News", new[] { "Sheet_1", "Sheet_2" }),
            (22.8f, "Our Pride", new[] { "Photo", "Photo", "Photo" }),
            (59f, "Rules for Pupils", new[] { "Sheet_3", "Sheet_2" }),
        };
        foreach (var (x, header, sheets) in north)
        {
            Put(parent, "Bench", new Vector3(x, Floor, CorridorNorth), 180f);
            Stand(parent, new Vector3(x, Floor + 2.5f, CorridorNorth), 180f, header, sheets);
        }
        // the timetable over the bench opposite the hall
        Put(parent, "Bench", new Vector3(39f, Floor, CorridorNorth), 180f);
        var timetable = Put(parent, "Board_5.2x3", new Vector3(39f, Floor + 2.2f, CorridorNorth), 180f);
        if (timetable != null)
        {
            Drawing(timetable.transform, AssetDatabase.LoadAssetAtPath<Sprite>(SchedulePath), new Vector3(0f, 1.38f, 0.07f), new Vector2(5f, 2.54f));
            Label(timetable.transform, "Timetable", new Vector3(0f, 2.82f, 0.07f), 3f, 0.3f, Ink, 2f);
        }

        var south = new (float x, string header, string[] sheets)[]
        {
            (-4f, "Wall Newspaper", new[] { "Sheet_2", "Sheet_3" }),
            (15.5f, "Sport", new[] { "Sheet_1", "Photo", "Photo" }),
            (46.6f, "Notices", new[] { "Sheet_3", "Sheet_1" }),
            (66.8f, "Fire Safety", new[] { "Sheet_2", "Sheet_3" }),
        };
        foreach (var (x, header, sheets) in south)
        {
            Put(parent, "Bench", new Vector3(x, Floor, CorridorSouth), 0f);
            Stand(parent, new Vector3(x, Floor + 2.5f, CorridorSouth), 0f, header, sheets);
        }

        // (x, z, yaw); the east palm was moved by hand, out of the corner
        var tubs = new[] { (-13.3f, -24f, 307.7f), (-13.3f, -17f, 307.7f), (79.3f, -24f, 298.3f), (78.63f, -18.24f, 298.3f), (23.3f, -23.9f, 2.3f), (40.7f, -23.9f, 181.7f) };
        for (int i = 0; i < tubs.Length; i++)
            Put(parent, i % 2 == 0 ? "Plant_Tub" : "Plant_Palm", new Vector3(tubs[i].Item1, Floor, tubs[i].Item2), tubs[i].Item3);

        // plants on wire stands along the walls, a drinking fountain by the canteen
        foreach (float x in new[] { -9f, 3.2f, 27f, 51f, 75f })
            Put(parent, "PlantStand", new Vector3(x, Floor, CorridorNorth - 0.4f), x * 17f);
        foreach (float x in new[] { -9f, 63f })
            Put(parent, "PlantStand", new Vector3(x, Floor, CorridorSouth + 0.4f), x * 17f);
        Put(parent, "DrinkingFountain", new Vector3(12f, Floor, CorridorSouth), 0f);

        Put(parent, "FireShield", new Vector3(77.5f, Floor + 0.9f, CorridorSouth), 0f);
        Put(parent, "WallClock", new Vector3(32f, Floor + 6.3f, CorridorSouth), 0f);
    }

    // ── Entrance hall ────────────────────────────────────────────────────────

    private static void Hall(Transform parent)
    {
        foreach (float x in new[] { 24.4f, 32f, 39.6f })
        foreach (float z in new[] { -41.9f, -35f, -28f })
            Put(parent, "Lamp_Tube", new Vector3(x, Ceiling, z), 0f);

        // the cloakroom in the corner by the entrance: a barred counter, coats on a rack and on hooks
        Put(parent, "CloakroomCounter", new Vector3(39f, Floor, -41.2f), 90f);
        Put(parent, "CoatRack", new Vector3(40.6f, Floor, -41.2f), 90f);
        Put(parent, "HookRail", new Vector3(41.75f, Floor + 2.7f, -41.2f), -90f);
        Sign(parent, "Cloakroom", new Vector3(38.96f, Floor + 3.35f, -41.2f), -90f);

        // the watchman's desk, facing the way in; a bench to wait on
        Put(parent, "GuardDesk", new Vector3(26.9f, Floor, -42.6f), 90f);
        Put(parent, "Bench", new Vector3(22.25f, Floor, -41.5f), 90f);

        // the honor board and the rules on either side of the way to the corridor
        var honor = Put(parent, "Board_5x2.6", new Vector3(22.25f, Floor + 2.3f, -27.9f), 90f);
        if (honor != null)
        {
            Label(honor.transform, "Honor Board", new Vector3(0f, 2.3f, 0.07f), 3.5f, 0.42f, Ink, 3f);
            for (int row = 0; row < 2; row++)
            for (int i = 0; i < 5; i++)
                Drawing(honor.transform, SchoolDrawings.Get("Photo", $"{Place(parent)}_HonorBoard_Photo_{row * 5 + i + 1:00}"), new Vector3(-1.9f + i * 0.95f, 1.45f - row * 0.95f, 0.07f), new Vector2(0.62f, 0.78f));
        }
        Stand(parent, new Vector3(41.75f, Floor + 2.5f, -27.9f), -90f, "Rules for Pupils", new[] { "Sheet_3", "Sheet_1" });

        // over the entrance, inside
        var banner = Put(parent, "BoardDark_10x1.4", new Vector3(32f, Floor + 5f, IZ0), 0f);
        if (banner != null)
            Label(banner.transform, "LEARNING IS LIGHT,\nIGNORANCE IS DARKNESS", new Vector3(0f, 0.7f, 0.07f), 9.4f, 1.2f, Chalk, 6f);

        // the middle of the hall, away from the walls: a runner from the door up to the bust (in its mask),
        // a planter beside it, and on either side of the runner hall seats back to back
        Put(parent, "Runner", new Vector3(32f, Floor, -44.6f), 0f);
        Put(parent, "Bust", new Vector3(32f, Floor, -29.8f), 180f);
        Put(parent, "Planter", new Vector3(29.4f, Floor, -29.8f), 0f);
        Put(parent, "SeatRows", new Vector3(27.5f, Floor, -35.8f), 0f);
        Put(parent, "SeatRows", new Vector3(36.5f, Floor, -35.8f), 0f);

        // by the way in: the school's cups behind glass, the state symbols over the bench (the long
        // mirror by the cloakroom was taken out by hand)
        Put(parent, "TrophyCabinet", new Vector3(26.1f, Floor, IZ0), 0f);
        Stand(parent, new Vector3(22.25f, Floor + 2.5f, -41.5f), 90f, "State Symbols", new[] { "Flag", "Emblem", "Sheet_3" });

        Put(parent, "WallClock", new Vector3(32f, Floor + 6.3f, RowA), 180f);
        Put(parent, "Plant_Tub", new Vector3(23.4f, Floor, -44.2f), 40f);
        Put(parent, "Plant_Tub", new Vector3(41.2f, Floor, -25.45f), 190f);
    }

    // ── Canteen ──────────────────────────────────────────────────────────────

    private static void Canteen(Transform parent)
    {
        foreach (float x in new[] { 0.5f, 6.5f, 12.5f, 18.5f })
        foreach (float z in new[] { -41f, -35f, -29f })
            Put(parent, "Lamp_Tube", new Vector3(x, Ceiling, z), 0f);

        // SchoolBuilder's tables: their tops at Floor + 1.3
        foreach (float x in new[] { 6f, 15f })
        foreach (float z in new[] { -41.4f, -36.6f, -31.8f })
            Put(parent, "TableSetting", new Vector3(x, Floor + 1.3f, z), 0f);

        Put(parent, "TrayStack", new Vector3(-3f, Floor + 1.62f, -41.3f), 0f);
        Put(parent, "KitchenPots", new Vector3(-4.95f, Floor + 1.62f, -37f), 90f);

        var menu = Put(parent, "BoardDark_4x1.8", new Vector3(-3.75f, Floor + 4.2f, -36f), 90f);
        if (menu != null)
            Label(menu.transform, "MENU\nBorscht\nBuckwheat, cutlet\nKompot, bread", new Vector3(0f, 0.9f, 0.07f), 3.6f, 1.6f, Chalk, 3f);
        var bread = Put(parent, "BoardDark_7x1.1", new Vector3(21.75f, Floor + 4.6f, -35f), -90f);
        if (bread != null)
            Label(bread.transform, "BREAD IS THE HEAD OF EVERYTHING", new Vector3(0f, 0.55f, 0.07f), 6.6f, 0.9f, Chalk, 4f);

        // wash your hands before you eat
        Put(parent, "TroughSink", new Vector3(2.5f, Floor, RowA), 180f);
        Put(parent, "Mirror_5", new Vector3(2.5f, Floor + 2.2f, RowA), 180f);

        float[] windows = { 1f, 7f, 13f, 19f };
        for (int i = 0; i < windows.Length; i++)
            Put(parent, i % 2 == 0 ? "Plant_Spiky" : "Plant_Small", new Vector3(windows[i] + 0.8f, SillTop, IZ0 + 0.05f), 0f);
        Put(parent, "Plant_Tub", new Vector3(21.2f, Floor, -25.5f), 120f);
    }

    // The canteen's kitchen (x -14..-4.25, z -44.78..-25), seen through the serving hatch: ranges and an
    // oven on the west wall, fridges by the corridor, a double sink and a rack of pots on the south wall,
    // tray racks on the north wall, two work tables down the middle.
    private static void Kitchen(Transform parent)
    {
        const float west = IX0, south = IZ0, north = RowA;
        Put(parent, "KitchenStove", new Vector3(west, Floor, -42.6f), 90f);
        Put(parent, "KitchenStove", new Vector3(west, Floor, -40.5f), 90f);
        Put(parent, "KitchenOven", new Vector3(west, Floor, -38.3f), 90f);
        Put(parent, "KitchenFridge", new Vector3(west, Floor, -30.6f), 90f);
        Put(parent, "KitchenFridge", new Vector3(west, Floor, -27.9f), 90f);
        Put(parent, "KitchenSink", new Vector3(-12.5f, Floor, south), 0f);
        Put(parent, "KitchenShelf", new Vector3(-6.1f, Floor, south), 0f);
        Put(parent, "TrayRack", new Vector3(-12.6f, Floor, north), 180f);
        Put(parent, "TrayRack", new Vector3(-11.4f, Floor, north), 180f);
        Put(parent, "KitchenTable", new Vector3(-9.4f, Floor, -39.5f), 90f);
        Put(parent, "KitchenTable", new Vector3(-9.4f, Floor, -33.5f), 90f);
        Put(parent, "Lamp_Tube", new Vector3(-9f, Ceiling, -40f), 90f);
        Put(parent, "Lamp_Tube", new Vector3(-9f, Ceiling, -33f), 90f);
        Put(parent, "Lamp_Tube", new Vector3(-9f, Ceiling, -27.5f), 90f);
    }

    // ── Gym ──────────────────────────────────────────────────────────────────

    private static void Gym(Transform parent)
    {
        foreach (float x in new[] { 47.5f, 57f, 66.5f })
        foreach (float z in new[] { -40f, -30f })
            Put(parent, "Lamp_Tube", new Vector3(x, GymCeiling, z), 0f);

        // either side of the door from the corridor
        Put(parent, "WallBars_8", new Vector3(49.2f, Floor, RowA), 180f);
        Put(parent, "WallBars_8", new Vector3(65.4f, Floor, RowA), 180f);
        Put(parent, "Mat", new Vector3(49.2f, Floor, -27f), 0f);

        // backboards over the doors on the short walls
        Put(parent, "Backboard", new Vector3(42.25f, Floor + 4.6f, -35f), 90f);
        Put(parent, "Backboard", new Vector3(71.75f, Floor + 4.6f, -35f), -90f);

        foreach (float x in new[] { 48f, 56f, 64f })
            Put(parent, "GymBench", new Vector3(x, Floor, -44.2f), 0f);
        Put(parent, "MatStack", new Vector3(44.6f, Floor, -42.9f), 0f);
        Put(parent, "VaultingHorse", new Vector3(53.5f, Floor, -36f), 0f);
        Put(parent, "Springboard", new Vector3(51.3f, Floor, -36f), 90f);
        Put(parent, "ClimbingRope", new Vector3(66.5f, GymCeiling, -37.5f), 0f);
        Put(parent, "ClimbingRope", new Vector3(66.5f, GymCeiling, -35.5f), 0f);
        Put(parent, "GymLines", new Vector3(57f, Floor, -34.89f), 0f);
        foreach (float wx in new[] { 47f, 53f, 59f, 65f })
            Put(parent, "Grille_4", new Vector3(wx, Floor + 7f, IZ0), 0f);
    }

    private static void ChangingRooms(Transform parent)
    {
        // a bench and a hook rail on each side of the wall between them
        Put(parent, "Bench", new Vector3(76.1f, Floor, -35f), 180f);
        Put(parent, "HookRail", new Vector3(76.1f, Floor + 2.6f, -35f), 180f);
        Put(parent, "Bench", new Vector3(76.1f, Floor, -34.5f), 0f);
        Put(parent, "HookRail", new Vector3(76.1f, Floor + 2.6f, -34.5f), 0f);
        Put(parent, "Lamp_Plate", new Vector3(76.1f, Ceiling, -40f), 0f);
        Put(parent, "Lamp_Plate", new Vector3(76.1f, Ceiling, -29.75f), 0f);
    }

    private static void Toilets(Transform parent)
    {
        ToiletRoom(Group(parent, "Girls"), west: 50.25f, east: 64.75f, door: 55f, boys: false);
        ToiletRoom(Group(parent, "Boys"), west: 65.25f, east: IX1, door: 70f, boys: true);
    }

    // One of the toilets (SchoolBuilder's: cabins along the east wall, a trough of taps on the west wall,
    // tiled to above the rail): a second trough of taps with a mirror, towels, pipes along the walls,
    // bins in the cabins, a drain, lamps, a mop. (The screen inside the door, the "Keep it clean!"
    // sign and the graffiti were taken out by hand.)
    private static void ToiletRoom(Transform parent, float west, float east, float door, bool boys)
    {
        const float tiles = 0.08f, south = RowC, cabinWidth = 3.2f;
        float front = east - 4.2f;

        Put(parent, "Lamp_Plate", new Vector3(door, Ceiling, -9f), 0f);
        Put(parent, "Lamp_Plate", new Vector3(door, Ceiling, -1f), 0f);

        Put(parent, "TroughSink", new Vector3(west + tiles, Floor, -12.3f), 90f);
        Put(parent, "Mirror_5", new Vector3(west + tiles, Floor + 2.2f, -12.3f), 90f);
        if (boys)
            Put(parent, "BucketMop", new Vector3(west + 1f, Floor, IZ1 - 0.4f), 180f);
        else
            Put(parent, "BucketMop", new Vector3(west + 0.95f, Floor, south + 0.5f), 0f);
        Put(parent, "TowelHooks", new Vector3(west + tiles, Floor + 2.3f, -7.55f), 90f);
        Put(parent, "FloorDrain", new Vector3((west + front) * 0.5f, Floor, -6f), 0f);

        // pipes along the south wall either side of the door, a riser in the corner
        foreach (var (x0, x1) in new[] { (west, door - 1.45f), (door + 1.45f, east) })
        {
            var pipes = Put(parent, "PipePair", new Vector3(x0, Floor, south + tiles), 0f);
            if (pipes != null)
                pipes.transform.localScale = new Vector3(x1 - x0, 1f, 1f);
        }
        Put(parent, "PipeRiser", new Vector3(west + 0.3f, Floor, south + tiles), 0f);

        // a bin in every other cabin, beside the step
        for (int i = 0; i < 5; i++)
        {
            float cz = IZ1 - i * cabinWidth - cabinWidth * 0.5f;
            if ((i + (boys ? 1 : 0)) % 2 == 0)
                Put(parent, "Bin", new Vector3(east - 0.55f, Floor, cz + 1.22f), 0f);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Transform Group(string name) => Group(_root, name);

    private static Transform Group(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    /// <summary>A prop at <paramref name="position"/>, its front (+z) turned by <paramref name="yaw"/> -
    /// 0 faces +z, 180 faces -z, 90 faces +x, -90 faces -x (on a wall: the way into the room).</summary>
    private static GameObject Put(Transform parent, string prop, Vector3 position, float yaw)
    {
        var prefab = SchoolProps.Get(prop);
        if (prefab == null)
            return null;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        // seats and cabinet doors stay non-static, so doors can swing and outlines show
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, SchoolInteractables.IsInteractable(t.gameObject) ? 0 : _static);
        return go;
    }

    // A felt stand (4 x 2) with a heading and two or three sheets pinned on it. Each sheet is a drawing
    // of its own, named after the room and the heading (e.g. Corridor_Stand_SchoolNews_1), to paint.
    private static void Stand(Transform parent, Vector3 position, float yaw, string header, string[] sheets)
    {
        var stand = Put(parent, "Board_4x2", position, yaw);
        if (stand == null)
            return;
        Label(stand.transform, header, new Vector3(0f, 1.72f, 0.07f), 3.4f, 0.36f, Ink, 3f);
        string where = Place(parent) + "_Stand_" + Slug(header);
        float spacing = 3.6f / sheets.Length;
        for (int i = 0; i < sheets.Length; i++)
        {
            float x = -1.8f + spacing * (i + 0.5f);
            bool photo = sheets[i] == "Photo";
            Drawing(stand.transform, SchoolDrawings.Get(sheets[i], $"{where}_{i + 1}"), new Vector3(x, 0.8f, 0.07f), photo ? new Vector2(0.8f, 1f) : new Vector2(1.05f, 1.4f));
        }
    }

    // where a drawing hangs, to start its file name with: a room's group name, with the classrooms
    // shortened (Classroom_101_Mathematics -> Class101_Mathematics)
    private static string Place(Transform group) => group.name.Replace("Classroom_", "Class");

    // letters and digits only, for drawing file names
    private static string Slug(string text)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char ch in text)
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
        return sb.ToString();
    }

    // a doodled plate with a handwritten word, like the door signs
    private static void Sign(Transform parent, string text, Vector3 position, float yaw)
    {
        var sign = Group(parent, "Sign_" + text);
        sign.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        Drawing(sign, AssetDatabase.LoadAssetAtPath<Sprite>(PlatePath), new Vector3(0f, 0f, 0.01f), new Vector2(1.5f, 0.58f), stretch: true);
        Label(sign, text, new Vector3(0f, 0f, 0.02f), 1.26f, 0.46f, Ink, 12f);
    }

    // A drawing on something's front (+z), centred at localPosition and fitted inside size keeping its
    // proportions (so a drawing of any shape can replace it) - or stretched to fill it exactly.
    private static void Drawing(Transform parent, Sprite sprite, Vector3 localPosition, Vector2 size, bool stretch = false)
    {
        if (sprite == null)
            return;
        var go = new GameObject("Drawing_" + sprite.name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // a sprite faces -z; turn it to face out
        Vector2 native = sprite.bounds.size;
        float fit = Mathf.Min(size.x / native.x, size.y / native.y);
        go.transform.localScale = stretch ? new Vector3(size.x / native.x, size.y / native.y, 1f) : new Vector3(fit, fit, 1f);
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        GameObjectUtility.SetStaticEditorFlags(go, _static);
    }

    // handwritten text on something's front (+z), centred at localPosition, fitted into width x height
    private static void Label(Transform parent, string text, Vector3 localPosition, float width, float height, Color color, float maxSize)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font = _font;
        tmp.text = text;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.3f;
        tmp.fontSizeMax = maxSize;
        tmp.lineSpacing = -20f;
        tmp.rectTransform.sizeDelta = new Vector2(width, height);
        var renderer = tmp.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 2;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        GameObjectUtility.SetStaticEditorFlags(go, _static);
    }
}
#endif
