// The Level 09 school's furniture and props, built from code in the same blocky, grayscale way as
// the building - the things that make it a school in a post-Soviet town in 1999: cast-iron radiators,
// fluorescent tube lamps, two-seat desks with tubular chairs, three-panel blackboards on a teacher's
// platform, glass-fronted bookcases, wall stands, a cloakroom with coat racks behind a barred counter,
// the watchman's desk with a rotary phone, gym wall bars, a vaulting horse and mats, squat toilets
// with high cast-iron cisterns, long trough sinks, canteen place settings, a fire shield...
//
// Each prop is one mesh (a submesh per material) saved with its prefab, so a room full of desks is a
// few dozen cheap draws, not thousands of boxes. Sizes are the school's kid-sized scale: the real
// thing's measurements times about 1.6. Every prop's back is at z = 0 and its front faces +z; wall
// props stand against the wall at z = 0, everything stands on y = 0 (lamps hang from it).
//
// SchoolBuilder and SchoolFurnisher ask for props by name (Get); each is rebuilt once per run.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class SchoolProps
{
    private const string PrefabFolder = "Assets/_Project/Prefabs/School/Props";
    private const string MeshFolder = "Assets/_Project/Models/School/Props";
    private const string MaterialFolder = "Assets/_Project/Materials/School";

    private static readonly HashSet<string> BuiltThisRun = new();

    /// <summary>Call at the start of a build, so each prop is rebuilt (once) from the current code.</summary>
    public static void BeginRun() => BuiltThisRun.Clear();

    public static GameObject Get(string name)
    {
        string path = $"{PrefabFolder}/{name}.prefab";
        if (BuiltThisRun.Contains(name))
        {
            var built = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (built != null)
                return built;
        }
        BuiltThisRun.Add(name);
        return Build(name);
    }

    [MenuItem("Tools/Read of Mask/School Props/Rebuild Props")]
    public static void RebuildMenu()
    {
        BeginRun();
        Debug.Log("[SchoolProps] Props are rebuilt the next time the school is built or furnished.");
    }

    // ── Materials ────────────────────────────────────────────────────────────

    private static Material _laminate, _metal, _trim, _wood, _paint, _chalk, _porcelain, _fabric, _fabricLight;
    private static Material _leather, _leaf, _foliage, _mirror, _smallTiles, _boards, _glass, _lamp, _lampGlass;

    private static void LoadMaterials()
    {
        _laminate = SchoolTextures.Get("Laminate", 1.5f, 0.3f);
        _metal = SchoolTextures.Get("Metal", 1f, 0.45f);
        _trim = SchoolTextures.Get("Trim", 1.2f, 0.35f);
        _wood = SchoolTextures.Get("WoodPaint", 1.2f, 0.4f);
        _paint = SchoolTextures.Get("WindowPaint", 1.2f, 0.45f);
        _chalk = SchoolTextures.Get("Chalkboard", 2f, 0.1f);
        _porcelain = SchoolTextures.Get("Porcelain", 1f, 0.75f);
        _fabric = SchoolTextures.Get("Fabric", 0.8f, 0.05f);
        _fabricLight = SchoolTextures.Get("FabricLight", 0.8f, 0.05f);
        _leather = SchoolTextures.Get("Leather", 1f, 0.35f);
        _leaf = SchoolTextures.Get("Leaf", 0.5f, 0.25f);
        _foliage = SchoolTextures.Get("Foliage", 0.5f, 0.3f);
        _mirror = SchoolTextures.Get("Mirror", 2f, 0.95f);
        _smallTiles = SchoolTextures.Get("SmallTiles", 2.4f, 0.5f);
        _boards = SchoolTextures.Get("Boards", 2.4f, 0.2f);
        _glass = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/School_Glass.mat") ?? _mirror;
        _lamp = Emissive("School_LampTube", 1.6f);
        _lampGlass = Emissive("School_LampGlass", 0.9f);
    }

    // the lit tube or frosted glass of a lamp: white that glows a little, no real light (the sun and
    // the level's lights do the lighting; dozens of real lights would cost too much)
    private static Material Emissive(string name, float intensity)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null)
            return mat;
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        mat.SetColor("_BaseColor", Color.white);
        mat.SetColor("_EmissionColor", Color.white * intensity);
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        mat.SetFloat("_Smoothness", 0.6f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    // ── The props ────────────────────────────────────────────────────────────

    private static GameObject Build(string name)
    {
        LoadMaterials();
        var kit = new Kit();
        // Indoors the sun only reaches the floor through the windows, so most props cast no shadow
        // (every caster is drawn again in each shadow cascade); the ones that stand in those sunlit
        // patches do - the desks, the curtains, the gym's window grilles.
        bool shadows = name.StartsWith("DeskPair") || name == "Curtains" || name == "Grille_4";
        bool collider = true;
        Action<GameObject> decorate = null;

        switch (name)
        {
            case "Radiator_10": Radiator(kit, 10); break;
            case "Radiator_16": Radiator(kit, 16); break;
            case "Lamp_Tube": LampTube(kit); collider = false; break;
            case "Lamp_Plate": LampPlate(kit); collider = false; break;
            case "DeskPair": Desk(kit); Chair(kit, new Vector3(-0.48f, 0f, -0.78f), Quaternion.identity); Chair(kit, new Vector3(0.48f, 0f, -0.78f), Quaternion.identity); break;
            case "DeskPair_Upturned":
                // after the last lesson: both chairs upside down on the desk, backs hanging over its edge
                Desk(kit);
                Chair(kit, new Vector3(-0.48f, 1.2f + 0.72f, -0.17f), Quaternion.Euler(0f, 0f, 180f));
                Chair(kit, new Vector3(0.48f, 1.2f + 0.72f, -0.17f), Quaternion.Euler(0f, 0f, 180f));
                break;
            case "TeacherDesk": TeacherDesk(kit); break;
            case "TeacherPlatform": TeacherPlatform(kit); break;
            case "Chalkboard": Chalkboard(kit); break;
            case "Bookcase": Bookcase(kit, 7); break;
            case "Bookcase_2": Bookcase(kit, 19); break;
            case "Board_4x2": Board(kit, 4f, 2f, _fabricLight); break;
            case "Board_5x2.6": Board(kit, 5f, 2.6f, _fabricLight); break;
            case "Board_5.2x3": Board(kit, 5.2f, 3f, _fabricLight); break;
            case "BoardDark_4x1.8": Board(kit, 4f, 1.8f, _chalk); break;
            case "BoardDark_7x1.1": Board(kit, 7f, 1.1f, _chalk); break;
            case "BoardDark_10x1.4": Board(kit, 10f, 1.4f, _chalk); break;
            case "PictureFrame": PictureFrame(kit, 0.9f, 1.15f); break;
            case "Bench": Bench(kit); break;
            case "GymBench": GymBench(kit); break;
            case "WallBars_8": WallBars(kit, 8); break;
            case "Backboard": Backboard(kit); collider = false; break;
            case "Mat": Mat(kit, Vector3.zero, 0f); break;
            case "MatStack": Mat(kit, Vector3.zero, 0f); Mat(kit, new Vector3(0.05f, 0.16f, -0.04f), 3f); Mat(kit, new Vector3(-0.04f, 0.32f, 0.06f), -4f); break;
            case "VaultingHorse": VaultingHorse(kit); break;
            case "Springboard": Springboard(kit); break;
            case "ClimbingRope": ClimbingRope(kit); collider = false; break;
            case "GymLines": GymLines(kit); collider = false; break;
            case "Grille_4": Grille(kit, 4f, 3.6f); collider = false; break;
            case "CoatRack": CoatRack(kit); break;
            case "CloakroomCounter": CloakroomCounter(kit); break;
            case "GuardDesk": GuardDesk(kit); break;
            case "Plant_Small": PlantSmall(kit, spiky: false); collider = false; break;
            case "Plant_Spiky": PlantSmall(kit, spiky: true); collider = false; break;
            case "Plant_Tub": PlantTub(kit, Gogo + "Decoration_Plant_04_01.prefab", 1.1f); break;
            case "Plant_Palm": PlantTub(kit, Gogo + "Decoration_Plant_01_01.prefab", 1.2f); break;
            case "Curtains": Curtains(kit, 3.6f, 4.1f); collider = false; break;
            case "TableSetting": TableSetting(kit); collider = false; break;
            case "TrayStack": TrayStack(kit); collider = false; break;
            case "KitchenPots": KitchenPots(kit); collider = false; break;
            case "KitchenStove": KitchenStove(kit); break;
            case "KitchenOven": WallModel(kit, Panda + "Prop_Oven_01.prefab", Kid); break;
            case "KitchenTable": KitchenTable(kit); break;
            case "KitchenSink": WallModel(kit, Panda + "Prop_Sink_01.prefab", Kid); break;
            case "KitchenShelf": KitchenShelf(kit); break;
            case "KitchenFridge": WallModel(kit, Panda + "Prop_Fridge_01.prefab", Kid); break;
            case "TrayRack": WallModel(kit, Panda + "Prop_TrayHolder.prefab", Kid); break;
            case "TroughSink": TroughSink(kit); break;
            case "Mirror_5": Mirror(kit, 5f, 1f); collider = false; break;
            case "SquatToilet": SquatToilet(kit); break;
            case "ToiletScreen": ToiletScreen(kit); break;
            case "Bin": Bin(kit); collider = false; break;
            case "FloorDrain": FloorDrain(kit); collider = false; break;
            case "PipePair": PipePair(kit); collider = false; break;
            case "PipeRiser": PipeRiser(kit); collider = false; break;
            case "TowelHooks": TowelHooks(kit); collider = false; break;
            case "BucketMop": BucketMop(kit); collider = false; break;
            case "WallClock":
                Disc(kit, Vector3.zero, Vector3.forward, 0.45f, 0.08f, _trim);
                decorate = go => AddDrawing(go, "Clock", new Vector3(0f, 0f, 0.085f), new Vector2(0.82f, 0.82f));
                collider = false;
                break;
            case "FireShield": FireShield(kit); collider = false; break;
            case "HookRail": HookRail(kit); collider = false; break;
            case "Globe": Globe(kit); collider = false; break;
            case "Bust": Bust(kit); break;
            case "TrophyCabinet": TrophyCabinet(kit); break;
            case "Runner": Runner(kit, 3.6f, 12.2f); collider = false; break;
            case "SeatRows": SeatRows(kit); break;
            case "Planter": Planter(kit); break;
            case "Mirror_Tall": Mirror(kit, 1.6f, 3.2f); collider = false; break;
            case "DrinkingFountain": DrinkingFountain(kit); break;
            case "PlantStand": PlantStand(kit); collider = false; break;
            default:
                Debug.LogWarning($"[SchoolProps] No prop called {name}.");
                return null;
        }

        return Save(name, kit, shadows, collider, decorate);
    }

    // A cast-iron sectional radiator, painted over many times, on two brackets, its pipes into the floor.
    private static void Radiator(Kit kit, int sections)
    {
        const float pitch = 0.13f, z0 = 0.07f, z1 = 0.27f, zc = 0.17f;
        float half = sections * pitch * 0.5f;
        for (int i = 0; i < sections; i++)
        {
            float x = -half + pitch * (i + 0.5f);
            kit.Box(new Vector3(x - 0.05f, 0.26f, z0), new Vector3(x + 0.05f, 1.2f, z1), _paint);
            kit.Box(new Vector3(x - 0.035f, 0.3f, z0 - 0.015f), new Vector3(x + 0.035f, 1.16f, z0), _paint);
        }
        kit.Cylinder(new Vector3(-half, 0.34f, zc), new Vector3(half, 0.34f, zc), 0.05f, _paint);
        kit.Cylinder(new Vector3(-half, 1.12f, zc), new Vector3(half, 1.12f, zc), 0.05f, _paint);
        foreach (float side in new[] { -1f, 1f })
        {
            float px = side * (half + 0.07f);
            kit.Cylinder(new Vector3(side * half, 0.34f, zc), new Vector3(px, 0.34f, zc), 0.035f, _paint);
            kit.Cylinder(new Vector3(px, 0f, zc), new Vector3(px, 0.36f, zc), 0.035f, _paint);
            kit.Box(new Vector3(side * half * 0.6f - 0.03f, 0.95f, 0f), new Vector3(side * half * 0.6f + 0.03f, 1.0f, z0), _metal);
        }
    }

    // a two-tube fluorescent fitting with louvres, the light of every Soviet classroom and corridor
    private static void LampTube(Kit kit)
    {
        kit.Box(new Vector3(-1f, -0.1f, -0.16f), new Vector3(1f, 0f, 0.16f), _paint);
        foreach (float z in new[] { -0.08f, 0.08f })
            kit.Cylinder(new Vector3(-0.95f, -0.14f, z), new Vector3(0.95f, -0.14f, z), 0.035f, _lamp, 8);
        kit.Box(new Vector3(-1f, -0.2f, -0.16f), new Vector3(-0.95f, -0.1f, 0.16f), _paint);
        kit.Box(new Vector3(0.95f, -0.2f, -0.16f), new Vector3(1f, -0.1f, 0.16f), _paint);
        for (int k = 0; k < 8; k++)
        {
            float x = -0.84f + k * 0.24f;
            kit.Box(new Vector3(x - 0.01f, -0.2f, -0.16f), new Vector3(x + 0.01f, -0.1f, 0.16f), _paint);
        }
    }

    // a round frosted ceiling lamp, for the toilets and changing rooms
    private static void LampPlate(Kit kit)
    {
        kit.Cylinder(new Vector3(0f, -0.05f, 0f), new Vector3(0f, 0f, 0f), 0.5f, 0.5f, _paint, 24);
        kit.Cylinder(new Vector3(0f, -0.2f, 0f), new Vector3(0f, -0.05f, 0f), 0.4f, 0.46f, _lampGlass, 24);
    }

    // The two-seat school desk: a laminate top on tubular legs, a shelf for the bags, a front panel.
    // The pupils sit at its -z side.
    private static void Desk(Kit kit)
    {
        kit.Box(new Vector3(-0.96f, 1.14f, -0.42f), new Vector3(0.96f, 1.2f, 0.42f), _laminate);
        kit.Box(new Vector3(-0.97f, 1.12f, -0.43f), new Vector3(0.97f, 1.14f, 0.43f), _wood);
        kit.Box(new Vector3(-0.9f, 0.88f, -0.32f), new Vector3(0.9f, 0.92f, 0.38f), _wood);
        kit.Box(new Vector3(-0.9f, 0.5f, 0.36f), new Vector3(0.9f, 1.12f, 0.4f), _wood);
        foreach (float x in new[] { -0.86f, 0.86f })
        {
            kit.Cylinder(new Vector3(x, 0f, -0.34f), new Vector3(x, 1.12f, -0.34f), 0.028f, _metal, 8);
            kit.Cylinder(new Vector3(x, 0f, 0.34f), new Vector3(x, 1.12f, 0.34f), 0.028f, _metal, 8);
            kit.Cylinder(new Vector3(x, 0.04f, -0.4f), new Vector3(x, 0.04f, 0.4f), 0.03f, _metal, 8);
            kit.Cylinder(new Vector3(x, 1.1f, -0.4f), new Vector3(x, 1.1f, 0.4f), 0.025f, _metal, 8);
        }
    }

    // a pupil's chair: a plywood seat and back on a bent tube frame, facing +z
    private static void Chair(Kit kit, Vector3 position, Quaternion rotation)
    {
        kit.Push(position, rotation);
        kit.Box(new Vector3(-0.33f, 0.67f, -0.3f), new Vector3(0.33f, 0.72f, 0.3f), _wood);
        kit.Box(new Vector3(-0.31f, 0.98f, -0.34f), new Vector3(0.31f, 1.32f, -0.3f), _wood);
        foreach (float x in new[] { -0.28f, 0.28f })
        {
            kit.Cylinder(new Vector3(x, 0f, -0.26f), new Vector3(x, 0.67f, -0.26f), 0.022f, _metal, 6);
            kit.Cylinder(new Vector3(x, 0f, 0.26f), new Vector3(x, 0.67f, 0.26f), 0.022f, _metal, 6);
            kit.Cylinder(new Vector3(x, 0.67f, -0.3f), new Vector3(x, 1.3f, -0.32f), 0.02f, _metal, 6);
        }
        kit.Cylinder(new Vector3(-0.28f, 0.64f, 0.26f), new Vector3(0.28f, 0.64f, 0.26f), 0.018f, _metal, 6);
        kit.Pop();
    }

    // the teacher's desk: a pedestal of drawers on one side, the class register and a pile of exercise
    // books on top, a heavy wooden chair behind it (the teacher sits at -z, facing the class)
    private static void TeacherDesk(Kit kit)
    {
        kit.Box(new Vector3(-1.2f, 1.19f, -0.55f), new Vector3(1.2f, 1.25f, 0.55f), _wood);
        kit.Box(new Vector3(0.4f, 0f, -0.5f), new Vector3(1.15f, 1.19f, 0.5f), _wood);
        foreach (float y in new[] { 0.42f, 0.8f })
            kit.Box(new Vector3(0.42f, y, -0.52f), new Vector3(1.13f, y + 0.02f, -0.5f), _trim);
        foreach (float y in new[] { 0.2f, 0.6f, 0.98f })
            kit.Box(new Vector3(0.7f, y, -0.55f), new Vector3(0.85f, y + 0.04f, -0.5f), _metal);
        kit.Box(new Vector3(-1.15f, 0f, -0.5f), new Vector3(-1.05f, 1.19f, -0.4f), _wood);
        kit.Box(new Vector3(-1.15f, 0f, 0.4f), new Vector3(-1.05f, 1.19f, 0.5f), _wood);
        kit.Box(new Vector3(-1.15f, 0.45f, 0.44f), new Vector3(0.4f, 1.19f, 0.5f), _wood);
        kit.Box(new Vector3(-0.75f, 1.25f, -0.25f), new Vector3(-0.15f, 1.3f, 0.4f), _fabric);          // the register
        kit.Box(new Vector3(0.3f, 1.25f, -0.15f), new Vector3(0.78f, 1.38f, 0.25f), _fabricLight);     // exercise books
        kit.Box(new Vector3(0.34f, 1.38f, -0.12f), new Vector3(0.74f, 1.41f, 0.22f), _fabric);
        WoodenChair(kit, new Vector3(-0.3f, 0f, -0.95f), 0.78f);
    }

    private static void WoodenChair(Kit kit, Vector3 position, float seat)
    {
        kit.Push(position, Quaternion.identity);
        kit.Box(new Vector3(-0.36f, seat - 0.06f, -0.32f), new Vector3(0.36f, seat, 0.32f), _wood);
        foreach (float x in new[] { -0.32f, 0.28f })
        {
            kit.Box(new Vector3(x, 0f, 0.24f), new Vector3(x + 0.05f, seat - 0.06f, 0.29f), _wood);
            kit.Box(new Vector3(x, 0f, -0.29f), new Vector3(x + 0.05f, seat + 0.75f, -0.24f), _wood);
        }
        kit.Box(new Vector3(-0.32f, seat + 0.35f, -0.3f), new Vector3(0.33f, seat + 0.7f, -0.26f), _wood);
        kit.Pop();
    }

    // a low wooden platform along the blackboard, for the teacher's desk
    private static void TeacherPlatform(Kit kit)
    {
        kit.Box(new Vector3(-6.5f, 0f, 0f), new Vector3(6.5f, 0.22f, 2.6f), _boards);
        kit.Box(new Vector3(-6.52f, 0f, 2.58f), new Vector3(6.52f, 0.22f, 2.62f), _trim);
    }

    // The three-panel blackboard: a fixed middle panel and two wings hinged on it, folded a little
    // forward; a wooden frame, a chalk ledge with chalk and the cloth.
    private static void Chalkboard(Kit kit)
    {
        BoardPanel(kit, -2.2f, 2.2f);
        kit.Push(new Vector3(2.26f, 0f, 0.02f), Quaternion.Euler(0f, -25f, 0f));
        BoardPanel(kit, 0.06f, 1.86f);
        kit.Pop();
        kit.Push(new Vector3(-2.26f, 0f, 0.02f), Quaternion.Euler(0f, 25f, 0f));
        BoardPanel(kit, -1.86f, -0.06f);
        kit.Pop();
        kit.Box(new Vector3(-2.3f, -0.12f, 0f), new Vector3(2.3f, -0.06f, 0.22f), _wood);
        kit.Box(new Vector3(-2.3f, -0.06f, 0.18f), new Vector3(2.3f, -0.02f, 0.22f), _wood);
        foreach (float x in new[] { -1.4f, -1.25f, 0.6f })
            kit.Box(new Vector3(x, -0.06f, 0.08f), new Vector3(x + 0.12f, -0.03f, 0.11f), _paint);
        kit.Box(new Vector3(1.2f, -0.06f, 0.04f), new Vector3(1.55f, -0.01f, 0.17f), _fabricLight);
    }

    private static void BoardPanel(Kit kit, float x0, float x1)
    {
        const float h = 1.9f, f = 0.07f;
        kit.Box(new Vector3(x0, 0f, 0.02f), new Vector3(x1, h, 0.08f), _chalk);
        kit.Box(new Vector3(x0 - f, h, 0f), new Vector3(x1 + f, h + f, 0.1f), _wood);
        kit.Box(new Vector3(x0 - f, -f, 0f), new Vector3(x1 + f, 0f, 0.1f), _wood);
        kit.Box(new Vector3(x0 - f, 0f, 0f), new Vector3(x0, h, 0.1f), _wood);
        kit.Box(new Vector3(x1, 0f, 0f), new Vector3(x1 + f, h, 0.1f), _wood);
    }

    // a glass-fronted bookcase: closed doors below, books behind the glass above
    private static void Bookcase(Kit kit, int seed)
    {
        const float w = 0.8f, h = 3.4f, d = 0.6f;
        kit.Box(new Vector3(-w, 0f, 0f), new Vector3(-w + 0.04f, h, d), _wood);
        kit.Box(new Vector3(w - 0.04f, 0f, 0f), new Vector3(w, h, d), _wood);
        kit.Box(new Vector3(-w, h - 0.04f, 0f), new Vector3(w, h, d), _wood);
        kit.Box(new Vector3(-w, 0f, 0.02f), new Vector3(w, 0.12f, d), _wood);
        kit.Box(new Vector3(-w + 0.04f, 0f, 0f), new Vector3(w - 0.04f, h - 0.04f, 0.03f), _wood);
        kit.Box(new Vector3(-w + 0.04f, 1.36f, 0f), new Vector3(w - 0.04f, 1.4f, d - 0.02f), _wood);
        kit.Box(new Vector3(-w + 0.04f, 0.12f, d - 0.04f), new Vector3(-0.01f, 1.36f, d), _wood);
        kit.Box(new Vector3(0.01f, 0.12f, d - 0.04f), new Vector3(w - 0.04f, 1.36f, d), _wood);
        kit.Box(new Vector3(-0.12f, 0.8f, d), new Vector3(-0.06f, 0.95f, d + 0.03f), _metal);
        kit.Box(new Vector3(0.06f, 0.8f, d), new Vector3(0.12f, 0.95f, d + 0.03f), _metal);

        var random = new System.Random(seed);
        var spines = new[] { _fabric, _fabricLight, _trim, _wood, _leather };
        foreach (float shelf in new[] { 1.4f, 2.08f, 2.74f })
        {
            if (shelf > 1.5f)
                kit.Box(new Vector3(-w + 0.04f, shelf - 0.03f, 0.03f), new Vector3(w - 0.04f, shelf, d - 0.05f), _wood);
            float x = -w + 0.06f;
            while (x < w - 0.2f)
            {
                float bw = 0.05f + (float)random.NextDouble() * 0.07f, bh = 0.34f + (float)random.NextDouble() * 0.22f;
                if (random.NextDouble() < 0.1)
                {
                    x += bw * 2f;
                    continue;
                }
                kit.Box(new Vector3(x, shelf, 0.08f), new Vector3(x + bw, shelf + bh, 0.46f), spines[random.Next(spines.Length)]);
                x += bw + 0.005f;
            }
        }
        kit.Box(new Vector3(-w + 0.04f, 1.4f, d - 0.03f), new Vector3(w - 0.04f, h - 0.04f, d - 0.015f), _glass);
        kit.Box(new Vector3(-0.02f, 1.4f, d - 0.04f), new Vector3(0.02f, h - 0.04f, d), _wood);
        kit.Box(new Vector3(-w + 0.04f, h - 0.12f, d - 0.04f), new Vector3(w - 0.04f, h - 0.04f, d), _wood);
    }

    // a wall stand: a wooden frame round a felt (or blackboard) panel; drawings and headings are added on it
    private static void Board(Kit kit, float w, float h, Material panel)
    {
        kit.Box(new Vector3(-w * 0.5f, 0f, 0f), new Vector3(w * 0.5f, h, 0.05f), _wood);
        kit.Box(new Vector3(-w * 0.5f + 0.07f, 0.07f, 0.05f), new Vector3(w * 0.5f - 0.07f, h - 0.07f, 0.065f), panel);
    }

    private static void PictureFrame(Kit kit, float w, float h)
    {
        kit.Box(new Vector3(-w * 0.5f, 0f, 0f), new Vector3(w * 0.5f, h, 0.05f), _trim);
    }

    // the corridor bench: three wooden slats on tubular frames, no back - the wall is the back
    private static void Bench(Kit kit)
    {
        foreach (var (z0, z1) in new[] { (0.06f, 0.22f), (0.25f, 0.41f), (0.44f, 0.6f) })
            kit.Box(new Vector3(-2f, 0.74f, z0), new Vector3(2f, 0.8f, z1), _wood);
        foreach (float x in new[] { -1.6f, 0f, 1.6f })
        {
            kit.Cylinder(new Vector3(x, 0f, 0.12f), new Vector3(x, 0.74f, 0.12f), 0.03f, _metal, 8);
            kit.Cylinder(new Vector3(x, 0f, 0.54f), new Vector3(x, 0.74f, 0.54f), 0.03f, _metal, 8);
            kit.Cylinder(new Vector3(x, 0.72f, 0.08f), new Vector3(x, 0.72f, 0.58f), 0.025f, _metal, 8);
            kit.Cylinder(new Vector3(x, 0.05f, 0.1f), new Vector3(x, 0.05f, 0.56f), 0.025f, _metal, 8);
        }
        kit.Cylinder(new Vector3(-1.6f, 0.3f, 0.33f), new Vector3(1.6f, 0.3f, 0.33f), 0.025f, _metal, 8);
    }

    // the gym bench: one long board on wooden legs, a narrow beam under it (centred, not against a wall)
    private static void GymBench(Kit kit)
    {
        kit.Box(new Vector3(-2.4f, 0.42f, -0.19f), new Vector3(2.4f, 0.48f, 0.19f), _boards);
        kit.Box(new Vector3(-2.3f, 0.08f, -0.05f), new Vector3(2.3f, 0.16f, 0.05f), _wood);
        foreach (float x in new[] { -2.1f, 0f, 2.1f })
        {
            kit.Box(new Vector3(x - 0.06f, 0.03f, -0.17f), new Vector3(x + 0.06f, 0.42f, 0.17f), _wood);
            kit.Box(new Vector3(x - 0.07f, 0f, -0.18f), new Vector3(x + 0.07f, 0.03f, 0.18f), _metal);
        }
    }

    // gym wall bars: wooden uprights and round rungs, standing off the wall on brackets
    private static void WallBars(Kit kit, int sections)
    {
        const float w = 1.28f, h = 4.2f;
        float half = sections * w * 0.5f;
        for (int k = 0; k <= sections; k++)
        {
            float x = -half + k * w;
            kit.Box(new Vector3(x - 0.05f, 0f, 0.18f), new Vector3(x + 0.05f, h, 0.3f), _wood);
            foreach (float y in new[] { 0.4f, h - 0.5f })
                kit.Box(new Vector3(x - 0.03f, y, 0f), new Vector3(x + 0.03f, y + 0.1f, 0.18f), _metal);
            if (k == sections)
                break;
            for (int r = 0; r < 14; r++)
            {
                float y = 0.25f + r * 0.3f;
                kit.Cylinder(new Vector3(x + 0.05f, y, 0.24f), new Vector3(x + w - 0.05f, y, 0.24f), 0.025f, _wood, 6);
            }
        }
    }

    // a basketball backboard on wall arms: the board with its painted lines, the hoop and a net
    private static void Backboard(Kit kit)
    {
        foreach (float x in new[] { -0.9f, 0.9f })
        {
            kit.Box(new Vector3(x - 0.04f, -0.4f, 0f), new Vector3(x + 0.04f, -0.32f, 1.0f), _metal);
            kit.Box(new Vector3(x - 0.04f, 1.3f, 0f), new Vector3(x + 0.04f, 1.38f, 1.0f), _metal);
        }
        kit.Box(new Vector3(-1.45f, -0.4f, 0.96f), new Vector3(1.45f, 1.7f, 1.02f), _paint);
        const float line = 0.06f, f = 1.025f;
        kit.Box(new Vector3(-1.45f, 1.64f, 1.02f), new Vector3(1.45f, 1.7f, f), _trim);
        kit.Box(new Vector3(-1.45f, -0.4f, 1.02f), new Vector3(1.45f, -0.34f, f), _trim);
        kit.Box(new Vector3(-1.45f, -0.4f, 1.02f), new Vector3(-1.39f, 1.7f, f), _trim);
        kit.Box(new Vector3(1.39f, -0.4f, 1.02f), new Vector3(1.45f, 1.7f, f), _trim);
        kit.Box(new Vector3(-0.47f, 0.15f, 1.02f), new Vector3(0.47f, 0.15f + line, f), _trim);
        kit.Box(new Vector3(-0.47f, 0.87f, 1.02f), new Vector3(0.47f, 0.87f + line, f), _trim);
        kit.Box(new Vector3(-0.47f, 0.15f, 1.02f), new Vector3(-0.47f + line, 0.93f, f), _trim);
        kit.Box(new Vector3(0.47f - line, 0.15f, 1.02f), new Vector3(0.47f, 0.93f, f), _trim);

        var rim = new Vector3(0f, 0.15f, 1.02f + 0.42f);
        Ring(kit, rim, 0.37f, 0.018f, _metal);
        Ring(kit, rim + Vector3.down * 0.65f, 0.22f, 0.008f, _fabricLight);
        kit.Box(new Vector3(-0.12f, 0.1f, 1.02f), new Vector3(0.12f, 0.17f, 1.08f), _metal);
        for (int i = 0; i < 10; i++)
        {
            float a = i / 10f * Mathf.PI * 2f;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            kit.Cylinder(rim + dir * 0.37f, rim + Vector3.down * 0.65f + dir * 0.22f, 0.007f, _fabricLight, 4);
        }
    }

    private static void Ring(Kit kit, Vector3 centre, float radius, float thickness, Material material)
    {
        const int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float a0 = i / (float)segments * Mathf.PI * 2f, a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
            kit.Cylinder(centre + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius,
                         centre + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius, thickness, material, 4, caps: false);
        }
    }

    // a leather gym mat, its edges a little rounded by years of use
    private static void Mat(Kit kit, Vector3 offset, float yaw)
    {
        kit.Push(offset, Quaternion.Euler(0f, yaw, 0f));
        kit.Box(new Vector3(-1.6f, 0.01f, -0.8f), new Vector3(1.6f, 0.15f, 0.8f), _leather);
        kit.Box(new Vector3(-1.58f, 0f, -0.78f), new Vector3(1.58f, 0.16f, 0.78f), _leather);
        kit.Pop();
    }

    // the vaulting "goat": a leather body on four splayed legs
    private static void VaultingHorse(Kit kit)
    {
        kit.Cylinder(new Vector3(-0.5f, 1.72f, 0f), new Vector3(0.5f, 1.72f, 0f), 0.3f, _leather, 16);
        kit.Box(new Vector3(-0.5f, 1.42f, -0.3f), new Vector3(0.5f, 1.72f, 0.3f), _leather);
        foreach (float x in new[] { -0.35f, 0.35f })
        foreach (float z in new[] { -0.18f, 0.18f })
        {
            var top = new Vector3(x, 1.42f, z);
            var foot = new Vector3(x * 1.6f, 0f, z * 2.3f);
            kit.Cylinder(top, Vector3.Lerp(top, foot, 0.5f), 0.05f, _wood, 8);
            kit.Cylinder(Vector3.Lerp(top, foot, 0.5f), foot, 0.035f, _metal, 8);
            kit.Box(foot + new Vector3(-0.06f, 0f, -0.06f), foot + new Vector3(0.06f, 0.05f, 0.06f), _leather);
        }
    }

    // the springboard in front of it: a sloping board on a low frame
    private static void Springboard(Kit kit)
    {
        kit.Box(new Vector3(-0.55f, 0f, -0.8f), new Vector3(0.55f, 0.06f, 0.8f), _wood);
        kit.Push(new Vector3(0f, 0.06f, -0.8f), Quaternion.Euler(-9f, 0f, 0f));
        kit.Box(new Vector3(-0.55f, 0f, 0f), new Vector3(0.55f, 0.06f, 1.62f), _wood);
        kit.Box(new Vector3(-0.5f, 0.06f, 0.6f), new Vector3(0.5f, 0.07f, 1.55f), _leather);
        kit.Pop();
    }

    // a climbing rope from the gym ceiling, knotted, its foot just off the floor
    private static void ClimbingRope(Kit kit)
    {
        const float length = 11.3f;
        kit.Box(new Vector3(-0.15f, -0.05f, -0.15f), new Vector3(0.15f, 0f, 0.15f), _metal);
        kit.Cylinder(new Vector3(0f, -length, 0f), new Vector3(0f, -0.05f, 0f), 0.045f, _fabricLight, 8);
        for (float y = -length + 0.3f; y < -6f; y += 1.2f)
            kit.Cylinder(new Vector3(0f, y, 0f), new Vector3(0f, y + 0.12f, 0f), 0.075f, _fabricLight, 8);
    }

    // the gym floor's painted lines: the court's boundary, the centre line and circle (round the gym's middle)
    private static void GymLines(Kit kit)
    {
        const float hx = 13.75f, hz = 8.89f, w = 0.08f, y0 = 0.002f, y1 = 0.006f;
        kit.Box(new Vector3(-hx, y0, -hz), new Vector3(hx, y1, -hz + w), _trim);
        kit.Box(new Vector3(-hx, y0, hz - w), new Vector3(hx, y1, hz), _trim);
        kit.Box(new Vector3(-hx, y0, -hz), new Vector3(-hx + w, y1, hz), _trim);
        kit.Box(new Vector3(hx - w, y0, -hz), new Vector3(hx, y1, hz), _trim);
        kit.Box(new Vector3(-w * 0.5f, y0, -hz), new Vector3(w * 0.5f, y1, hz), _trim);
        const int segments = 40;
        for (int i = 0; i < segments; i++)
        {
            float a = (i + 0.5f) / segments * 360f;
            kit.Push(Vector3.zero, Quaternion.Euler(0f, a, 0f));
            kit.Box(new Vector3(1.8f - w * 0.5f, y0, -0.15f), new Vector3(1.8f + w * 0.5f, y1, 0.15f), _trim);
            kit.Pop();
        }
    }

    // a protective grille over a gym window, against the balls
    private static void Grille(Kit kit, float w, float h)
    {
        const float z = 0.15f;
        for (float x = -w * 0.5f; x <= w * 0.5f + 0.01f; x += 0.3f)
            kit.Cylinder(new Vector3(x, 0f, z), new Vector3(x, h, z), 0.012f, _metal, 4);
        for (float y = 0f; y <= h + 0.01f; y += 0.3f)
            kit.Cylinder(new Vector3(-w * 0.5f, y, z), new Vector3(w * 0.5f, y, z), 0.012f, _metal, 4);
        foreach (float x in new[] { -w * 0.5f, w * 0.5f })
            kit.Box(new Vector3(x - 0.03f, 0f, 0f), new Vector3(x + 0.03f, h, z + 0.02f), _metal);
    }

    // a double-sided cloakroom rack, full of coats (long along x)
    private static void CoatRack(Kit kit)
    {
        const float half = 3.2f, bar = 2.6f;
        kit.Box(new Vector3(-half, 0f, -0.3f), new Vector3(half, 0.06f, -0.24f), _metal);
        kit.Box(new Vector3(-half, 0f, 0.24f), new Vector3(half, 0.06f, 0.3f), _metal);
        for (float x = -half; x <= half + 0.01f; x += 1.6f)
        {
            kit.Cylinder(new Vector3(x, 0f, 0f), new Vector3(x, bar + 0.1f, 0f), 0.03f, _metal, 8);
            kit.Box(new Vector3(x - 0.05f, 0f, -0.3f), new Vector3(x + 0.05f, 0.06f, 0.3f), _metal);
        }
        kit.Cylinder(new Vector3(-half, bar, 0f), new Vector3(half, bar, 0f), 0.025f, _metal, 8);

        var random = new System.Random(5);
        foreach (float side in new[] { -1f, 1f })
        for (float x = -half + 0.35f; x < half - 0.3f; x += 0.42f)
        {
            kit.Box(new Vector3(x - 0.015f, bar - 0.12f, side * 0.02f), new Vector3(x + 0.015f, bar, side * 0.1f), _metal);
            if (random.NextDouble() < 0.25)
                continue;
            var mat = random.NextDouble() < 0.6 ? _fabric : _fabricLight;
            float length = 1.2f + (float)random.NextDouble() * 0.4f;
            kit.Push(new Vector3(x, bar - 0.06f, side * 0.15f), Quaternion.Euler(0f, (float)(random.NextDouble() - 0.5) * 16f, 0f));
            kit.Box(new Vector3(-0.3f, -length, -0.09f), new Vector3(0.3f, -0.12f, 0.09f), mat);
            kit.Box(new Vector3(-0.34f, -0.34f, -0.1f), new Vector3(0.34f, -0.12f, 0.1f), mat);
            kit.Box(new Vector3(-0.13f, -0.14f, -0.08f), new Vector3(0.13f, 0.02f, 0.08f), mat);
            kit.Pop();
        }
    }

    // the cloakroom counter (long along x): a wooden counter, bars up to head height, a window to hand coats through
    private static void CloakroomCounter(Kit kit)
    {
        const float half = 3.6f, top = 1.58f, barsTop = 3.7f, window = 0.9f;
        kit.Box(new Vector3(-half, 0f, -0.3f), new Vector3(half, 1.5f, 0.3f), _wood);
        kit.Box(new Vector3(-half - 0.05f, 1.5f, -0.35f), new Vector3(half + 0.05f, top, 0.35f), _laminate);
        for (float x = -half + 0.1f; x < half; x += 0.2f)
        {
            float bottom = Mathf.Abs(x) < window ? 2.98f : top;
            kit.Cylinder(new Vector3(x, bottom, 0f), new Vector3(x, barsTop, 0f), 0.018f, _metal, 4);
        }
        kit.Box(new Vector3(-half, barsTop, -0.04f), new Vector3(half, barsTop + 0.08f, 0.04f), _metal);
        kit.Box(new Vector3(-window - 0.05f, 2.9f, -0.04f), new Vector3(window + 0.05f, 2.98f, 0.04f), _metal);
    }

    // The watchman's desk by the entrance: a rotary phone, a desk lamp, the visitors' logbook, a glass
    // of tea; his chair behind it (seat at the same height as the benches, so a kid can sit there).
    private static void GuardDesk(Kit kit)
    {
        kit.Box(new Vector3(-0.9f, 1.19f, -0.5f), new Vector3(0.9f, 1.25f, 0.5f), _laminate);
        kit.Box(new Vector3(-0.88f, 0f, -0.45f), new Vector3(-0.3f, 1.19f, 0.45f), _wood);
        kit.Box(new Vector3(0.78f, 0f, -0.45f), new Vector3(0.88f, 1.19f, -0.35f), _wood);
        kit.Box(new Vector3(0.78f, 0f, 0.35f), new Vector3(0.88f, 1.19f, 0.45f), _wood);
        kit.Box(new Vector3(-0.88f, 0.4f, 0.42f), new Vector3(0.88f, 1.19f, 0.46f), _wood);

        kit.Push(new Vector3(0.45f, 1.25f, -0.05f), Quaternion.Euler(0f, 15f, 0f));
        kit.Box(new Vector3(-0.13f, 0f, -0.11f), new Vector3(0.13f, 0.09f, 0.11f), _trim);
        kit.Box(new Vector3(-0.17f, 0.09f, -0.04f), new Vector3(0.17f, 0.12f, 0.04f), _trim);
        kit.Box(new Vector3(-0.2f, 0.08f, -0.06f), new Vector3(-0.11f, 0.15f, 0.06f), _trim);
        kit.Box(new Vector3(0.11f, 0.08f, -0.06f), new Vector3(0.2f, 0.15f, 0.06f), _trim);
        Disc(kit, new Vector3(0f, 0.05f, 0.11f), Vector3.forward, 0.065f, 0.01f, _paint);
        kit.Pop();

        kit.Push(new Vector3(-0.6f, 1.25f, 0.2f), Quaternion.identity);
        kit.Cylinder(Vector3.zero, new Vector3(0f, 0.03f, 0f), 0.12f, _metal, 12);
        kit.Cylinder(new Vector3(0f, 0.03f, 0f), new Vector3(0.06f, 0.48f, -0.06f), 0.015f, _metal, 6);
        kit.Cylinder(new Vector3(0.06f, 0.48f, -0.06f), new Vector3(0.14f, 0.34f, -0.14f), 0.04f, 0.14f, _metal, 12);
        kit.Pop();

        kit.Box(new Vector3(-0.3f, 1.25f, -0.35f), new Vector3(0.22f, 1.28f, 0.05f), _fabric);
        kit.Box(new Vector3(-0.29f, 1.28f, -0.34f), new Vector3(0.21f, 1.285f, 0.04f), _fabricLight);
        kit.Cylinder(new Vector3(0.1f, 1.25f, 0.28f), new Vector3(0.1f, 1.38f, 0.28f), 0.045f, _glass, 10);
        WoodenChair(kit, new Vector3(0f, 0f, -0.85f), 0.8f);
    }

    // A pot plant for a windowsill: one of the Gogo plants in a white Gogo vase (or a spiky aloe).
    private static void PlantSmall(Kit kit, bool spiky)
    {
        string vase = Gogo + (spiky ? "Decoration_Vase_05_01.prefab" : "Decoration_Vase_07_01.prefab");
        string plant = Gogo + (spiky ? "Decoration_Plant_10_01.prefab" : "Decoration_Plant_08_01.prefab");
        const float scale = 0.85f;
        Model(kit, vase, Vector3.zero, 0f, scale, Keep);
        Model(kit, plant, new Vector3(0f, ModelHeight(vase) * scale * 0.8f, 0f), spiky ? 30f : 0f, scale * (spiky ? 1.3f : 1f), Foliage);
    }

    // a big pot plant (a ficus, or a palm) in a wooden tub with iron hoops, for the corridor and hall corners
    private static void PlantTub(Kit kit, string plant, float scale)
    {
        kit.Cylinder(Vector3.zero, new Vector3(0f, 0.75f, 0f), 0.38f, 0.47f, _wood, 24);
        kit.Cylinder(new Vector3(0f, 0.12f, 0f), new Vector3(0f, 0.17f, 0f), 0.4f, 0.41f, _metal, 24, caps: false);
        kit.Cylinder(new Vector3(0f, 0.6f, 0f), new Vector3(0f, 0.65f, 0f), 0.455f, 0.46f, _metal, 24, caps: false);
        kit.Cylinder(new Vector3(0f, 0.72f, 0f), new Vector3(0f, 0.74f, 0f), 0.44f, _leather, 24);
        Model(kit, plant, new Vector3(0f, 0.72f, 0f), 0f, scale, Foliage);
    }

    // Classroom curtains: a rod over the window and a pleated drape gathered either side of it.
    // Centred on the window (w wide), from y = 0 (just under the sill) up h.
    private static void Curtains(Kit kit, float w, float h)
    {
        float half = w * 0.5f;
        kit.Cylinder(new Vector3(-half - 0.9f, h + 0.05f, 0.24f), new Vector3(half + 0.9f, h + 0.05f, 0.24f), 0.025f, _metal, 6);
        foreach (float side in new[] { -1f, 1f })
        {
            kit.Box(new Vector3(side * (half + 0.85f) - 0.02f, h, 0f), new Vector3(side * (half + 0.85f) + 0.02f, h + 0.1f, 0.24f), _metal);
            for (int p = 0; p < 6; p++)
            {
                float x = side * (half + 0.1f + p * 0.12f);
                float z = p % 2 == 0 ? 0.12f : 0.18f;
                kit.Box(new Vector3(x - 0.07f, 0f, z), new Vector3(x + 0.07f, h, z + 0.03f), _fabricLight);
            }
        }
    }

    // the school's kid-sized scale against the Pandazole models' real one
    private const float Kid = 1.6f;

    // One canteen table's places (origin on the table top's centre): a plate with a bowl of soup and a
    // spoon, a mug of kompot, for each place; a loaf on a plate in the middle of each half; a napkin holder.
    private static void TableSetting(Kit kit)
    {
        const float s = 1.15f;
        foreach (float x in new[] { -1.7f, 0f, 1.7f })
        foreach (float side in new[] { -1f, 1f })
        {
            float z = side * 0.42f;
            Model(kit, Panda + "Prop_Plate_01.prefab", new Vector3(x, 0f, z), 0f, s);
            Model(kit, Panda + "Prop_Bowel_02.prefab", new Vector3(x, 0.03f, z), 0f, s);
            kit.Cylinder(new Vector3(x, 0.075f, z), new Vector3(x, 0.08f, z), 0.1f, _leather, 16);
            kit.Box(new Vector3(x + 0.24f, 0f, z - 0.12f), new Vector3(x + 0.26f, 0.012f, z + 0.12f), _metal);
            Model(kit, Panda + "Prop_Cup_01.prefab", new Vector3(x - 0.34f, 0f, z * 0.7f), side > 0f ? 200f : 20f, s);
        }
        foreach (float x in new[] { -0.85f, 0.85f })
        {
            Model(kit, Panda + "Prop_Plate_01.prefab", new Vector3(x, 0f, 0f), 0f, s);
            Model(kit, Panda + "Food_Bread.prefab", new Vector3(x, 0.03f, 0f), 90f, s);
        }
        kit.Box(new Vector3(-0.08f, 0f, -0.03f), new Vector3(0.08f, 0.13f, 0.03f), _metal);
        kit.Box(new Vector3(-0.07f, 0.02f, -0.02f), new Vector3(0.07f, 0.16f, 0.02f), _paint);
    }

    // a stack of canteen trays on the counter
    private static void TrayStack(Kit kit)
    {
        for (int i = 0; i < 12; i++)
            Model(kit, Panda + "Prop_Tray.prefab", new Vector3(0f, i * 0.022f, 0f), 90f + (i % 3 - 1) * 2f, Kid);
    }

    // the kitchen's big stock pots, in a row (along x), on the serving counter
    private static void KitchenPots(Kit kit)
    {
        foreach (float x in new[] { -0.9f, 0f, 0.9f })
            Model(kit, Panda + "Prop_Pot_05.prefab", new Vector3(x, 0f, 0f), x * 40f, 2.4f);
    }

    // A model against a wall: its back moved to z = 0.
    private static void WallModel(Kit kit, string path, float scale)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        float back = 0f;
        if (prefab != null)
            foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null)
                    back = Mathf.Min(back, filter.transform.localToWorldMatrix.MultiplyPoint3x4(filter.sharedMesh.bounds.min).z);
        Model(kit, path, new Vector3(0f, 0f, -back * scale), 0f, scale);
    }

    // The canteen kitchen's six-burner range against the wall, with two stock pots and a pan on it.
    private static void KitchenStove(Kit kit)
    {
        WallModel(kit, Panda + "Prop_Stove.prefab", Kid);
        const float top = 0.985f * Kid;   // the cooktop (measured on the model)
        Model(kit, Panda + "Prop_Pot_05.prefab", new Vector3(-0.45f, top, 0.6f), 20f, 2.2f);
        Model(kit, Panda + "Prop_Pot_03.prefab", new Vector3(0.5f, top, 0.65f), -30f, Kid);
        Model(kit, Panda + "Prop_Pan_01.prefab", new Vector3(0.55f, top, 0.95f), 160f, Kid);
    }

    // a stainless work table (long along x) with the morning's work on it
    private static void KitchenTable(Kit kit)
    {
        // the model's top is 1.134 high; scaled to the serving counter's height
        const float scale = 1.32f, top = 1.134f * scale;
        Model(kit, Panda + "Prop_KitchenTable_04.prefab", Vector3.zero, 0f, scale);
        Model(kit, Panda + "Prop_CuttingPlate.prefab", new Vector3(-1.2f, top, 0.05f), 10f, 2f);
        Model(kit, Panda + "Prop_Knife_01.prefab", new Vector3(-1.1f, top + 0.02f, 0.05f), 80f, Kid);
        Model(kit, Panda + "Prop_Bowel_04.prefab", new Vector3(-0.3f, top, -0.1f), 0f, 2f);
        Model(kit, Panda + "Prop_Pot_01.prefab", new Vector3(0.6f, top, 0.05f), 15f, Kid);
        Model(kit, Panda + "Prop_Bowel_02.prefab", new Vector3(1.4f, top, -0.15f), 0f, 1.8f);
        Model(kit, Panda + "Prop_Bowel_02.prefab", new Vector3(1.4f, top + 0.09f, -0.15f), 40f, 1.8f);
        Model(kit, Panda + "Prop_Tray.prefab", new Vector3(1.6f, top, 0.25f), 0f, Kid);
    }

    // a wire shelving rack against the wall, stacked with pots, pans and bowls
    private static void KitchenShelf(Kit kit)
    {
        WallModel(kit, Panda + "Prop_StorageShelve.prefab", Kid);
        float z = 0.47f;
        foreach (var (x, shelf, prop, yaw) in new[]
        {
            (-1.2f, 0.68f, "Prop_Pot_02", 0f), (-0.4f, 0.68f, "Prop_Pot_04", 30f), (0.5f, 0.68f, "Prop_Pot_06", -20f), (1.3f, 0.68f, "Prop_Pot_01", 10f),
            (-1.2f, 1.22f, "Prop_Bowel_03", 0f), (-0.6f, 1.22f, "Prop_Bowel_03", 0f), (0.3f, 1.22f, "Prop_Pan_02", 90f), (1.2f, 1.22f, "Prop_Pan_03", 80f),
            (-0.9f, 1.76f, "Prop_Tray", 0f), (-0.9f, 1.79f, "Prop_Tray", 3f), (0.4f, 1.76f, "Prop_Plate_02", 0f), (0.4f, 1.79f, "Prop_Plate_02", 0f), (1.2f, 1.76f, "Prop_Cup_02", 0f),
        })
            Model(kit, Panda + prop + ".prefab", new Vector3(x, shelf * Kid, z), yaw, Kid);
    }

    // a long trough sink with a row of taps on one pipe along the wall (long along x)
    private static void TroughSink(Kit kit)
    {
        const float half = 2.5f;
        kit.Box(new Vector3(-half, 1.0f, 0.62f), new Vector3(half, 1.35f, 0.7f), _porcelain);
        kit.Box(new Vector3(-half, 1.0f, 0f), new Vector3(half, 1.4f, 0.08f), _porcelain);
        kit.Box(new Vector3(-half, 1.0f, 0f), new Vector3(-half + 0.08f, 1.35f, 0.7f), _porcelain);
        kit.Box(new Vector3(half - 0.08f, 1.0f, 0f), new Vector3(half, 1.35f, 0.7f), _porcelain);
        kit.Box(new Vector3(-half + 0.08f, 1.0f, 0.08f), new Vector3(half - 0.08f, 1.06f, 0.62f), _porcelain);
        kit.Cylinder(new Vector3(0f, 1.06f, 0.35f), new Vector3(0f, 1.065f, 0.35f), 0.05f, _metal, 8);
        kit.Cylinder(new Vector3(0f, 0f, 0.35f), new Vector3(0f, 1.0f, 0.35f), 0.05f, _metal, 8);
        foreach (float x in new[] { -1.8f, 0f, 1.8f })
            kit.Box(new Vector3(x - 0.04f, 0.8f, 0f), new Vector3(x + 0.04f, 1.0f, 0.6f), _metal);
        kit.Cylinder(new Vector3(-half + 0.1f, 1.75f, 0.05f), new Vector3(half - 0.1f, 1.75f, 0.05f), 0.03f, _metal, 8);
        // soap dishes on the back edge, a cake of soap in some
        foreach (float x in new[] { -1.2f, 0f, 1.2f })
        {
            kit.Box(new Vector3(x - 0.09f, 1.4f, 0.01f), new Vector3(x + 0.09f, 1.43f, 0.08f), _porcelain);
            if (x <= 0f)
                kit.Box(new Vector3(x - 0.05f, 1.43f, 0.025f), new Vector3(x + 0.05f, 1.46f, 0.07f), _fabricLight);
        }
        foreach (float x in new[] { -1.8f, -0.6f, 0.6f, 1.8f })
        {
            kit.Cylinder(new Vector3(x, 1.75f, 0.05f), new Vector3(x, 1.75f, 0.24f), 0.025f, _metal, 8);
            kit.Cylinder(new Vector3(x, 1.75f, 0.24f), new Vector3(x, 1.6f, 0.27f), 0.02f, _metal, 8);
            kit.Box(new Vector3(x - 0.07f, 1.8f, 0.1f), new Vector3(x + 0.07f, 1.84f, 0.15f), _metal);
        }
    }

    private static void Mirror(Kit kit, float w, float h)
    {
        kit.Box(new Vector3(-w * 0.5f, 0f, 0f), new Vector3(w * 0.5f, h, 0.02f), _trim);
        kit.Box(new Vector3(-w * 0.5f + 0.05f, 0.05f, 0.02f), new Vector3(w * 0.5f - 0.05f, h - 0.05f, 0.03f), _mirror);
    }

    // The squat toilet of every Soviet school: a porcelain pan with foot treads set in a tiled step, a
    // cast-iron cistern high on the wall, its pipe down to the pan and a chain to pull.
    private static void SquatToilet(Kit kit)
    {
        kit.Box(new Vector3(-0.85f, 0f, 0.05f), new Vector3(0.85f, 0.24f, 1.6f), _smallTiles);
        kit.Box(new Vector3(-0.42f, 0.24f, 0.25f), new Vector3(0.42f, 0.29f, 1.45f), _porcelain);
        kit.Box(new Vector3(-0.17f, 0.24f, 0.5f), new Vector3(0.17f, 0.292f, 1.15f), _leather);
        foreach (float x in new[] { -0.3f, 0.3f })
            for (int r = 0; r < 4; r++)
                kit.Box(new Vector3(x - 0.09f, 0.29f, 0.75f + r * 0.09f), new Vector3(x + 0.09f, 0.31f, 0.8f + r * 0.09f), _porcelain);
        kit.Box(new Vector3(-0.35f, 3.4f, 0.05f), new Vector3(0.35f, 3.8f, 0.4f), _metal);
        kit.Box(new Vector3(-0.08f, 3.2f, 0f), new Vector3(0.08f, 3.4f, 0.1f), _metal);
        kit.Cylinder(new Vector3(0f, 3.4f, 0.18f), new Vector3(0f, 0.27f, 0.3f), 0.035f, _metal, 8);
        kit.Cylinder(new Vector3(0.3f, 2.4f, 0.32f), new Vector3(0.3f, 3.42f, 0.32f), 0.008f, _metal, 4);
        kit.Cylinder(new Vector3(0.3f, 2.28f, 0.32f), new Vector3(0.3f, 2.4f, 0.32f), 0.025f, _porcelain, 8);
    }

    // a tiled screen wall inside the toilet's door, so the room can't be seen from the corridor (long along x)
    private static void ToiletScreen(Kit kit)
    {
        kit.Box(new Vector3(-2.2f, 0f, -0.075f), new Vector3(2.2f, 3f, 0.075f), _smallTiles);
        kit.Box(new Vector3(-2.24f, 3f, -0.1f), new Vector3(2.24f, 3.06f, 0.1f), _paint);
    }

    // a dented metal pedal bin
    private static void Bin(Kit kit)
    {
        kit.Lathe(Vector3.zero, new[] { new Vector2(0f, 0.01f), new Vector2(0.17f, 0.01f), new Vector2(0.19f, 0.05f), new Vector2(0.2f, 0.55f), new Vector2(0.21f, 0.57f), new Vector2(0.18f, 0.62f), new Vector2(0.06f, 0.66f), new Vector2(0f, 0.67f) }, _metal, 20);
        kit.Box(new Vector3(-0.06f, 0f, 0.15f), new Vector3(0.06f, 0.03f, 0.3f), _trim);
    }

    // a floor drain's grate
    private static void FloorDrain(Kit kit)
    {
        kit.Box(new Vector3(-0.22f, 0f, -0.22f), new Vector3(0.22f, 0.006f, 0.22f), _metal);
        for (int i = 0; i < 5; i++)
        {
            float z = -0.14f + i * 0.07f;
            kit.Box(new Vector3(-0.15f, 0.006f, z - 0.015f), new Vector3(0.15f, 0.008f, z + 0.015f), _leather);
        }
    }

    // two painted water pipes running along the bottom of a wall, one metre long (scale it along x)
    private static void PipePair(Kit kit)
    {
        kit.Cylinder(new Vector3(0f, 0.38f, 0.13f), new Vector3(1f, 0.38f, 0.13f), 0.045f, _paint, 10, caps: false);
        kit.Cylinder(new Vector3(0f, 0.54f, 0.12f), new Vector3(1f, 0.54f, 0.12f), 0.032f, _paint, 10, caps: false);
    }

    // a pipe riser in a corner, floor to ceiling, with its smaller twin
    private static void PipeRiser(Kit kit)
    {
        kit.Cylinder(new Vector3(0f, 0f, 0.14f), new Vector3(0f, 7f, 0.14f), 0.05f, _paint, 10);
        kit.Cylinder(new Vector3(0.16f, 0f, 0.13f), new Vector3(0.16f, 7f, 0.13f), 0.032f, _paint, 10);
        foreach (float y in new[] { 0.38f, 0.54f })
            kit.Cylinder(new Vector3(0f, y, 0.14f), new Vector3(0.16f, y, 0.13f), 0.04f, _paint, 8);
    }

    // a towel rail by the sinks: a board with hooks, two worn towels
    private static void TowelHooks(Kit kit)
    {
        kit.Box(new Vector3(-0.7f, 0f, 0f), new Vector3(0.7f, 0.15f, 0.04f), _wood);
        foreach (float x in new[] { -0.45f, 0f, 0.45f })
            kit.Box(new Vector3(x - 0.02f, 0.05f, 0.04f), new Vector3(x + 0.02f, 0.09f, 0.16f), _metal);
        kit.Box(new Vector3(-0.67f, -0.85f, 0.06f), new Vector3(-0.23f, 0.06f, 0.09f), _fabricLight);
        kit.Box(new Vector3(-0.2f, -0.7f, 0.06f), new Vector3(0.2f, 0.06f, 0.09f), _fabric);
    }

    // an enamel bucket with a mop leaning against the wall beside it
    private static void BucketMop(Kit kit)
    {
        kit.Cylinder(Vector3.zero, new Vector3(0f, 0.45f, 0f), 0.2f, 0.26f, _metal, 14);
        kit.Cylinder(new Vector3(0f, 0.38f, 0f), new Vector3(0f, 0.385f, 0f), 0.24f, _leather, 14);
        kit.Cylinder(new Vector3(-0.25f, 0.45f, 0f), new Vector3(0f, 0.7f, 0f), 0.008f, _metal, 4);
        kit.Cylinder(new Vector3(0f, 0.7f, 0f), new Vector3(0.25f, 0.45f, 0f), 0.008f, _metal, 4);
        kit.Push(new Vector3(0.5f, 0f, 0.2f), Quaternion.Euler(-10f, 0f, 0f));
        kit.Cylinder(new Vector3(0f, 0.06f, 0f), new Vector3(0f, 2.4f, 0f), 0.025f, _wood, 6);
        kit.Box(new Vector3(-0.25f, 0f, -0.08f), new Vector3(0.25f, 0.08f, 0.08f), _wood);
        kit.Box(new Vector3(-0.28f, -0.01f, -0.13f), new Vector3(0.28f, 0.06f, 0.13f), _fabricLight);
        kit.Pop();
    }

    // the fire shield: an extinguisher, a cone-shaped fire bucket, an axe and a shovel on a board
    private static void FireShield(Kit kit)
    {
        kit.Box(new Vector3(-0.85f, 0f, 0f), new Vector3(0.85f, 2.3f, 0.04f), _trim);
        kit.Box(new Vector3(-0.8f, 0.05f, 0.04f), new Vector3(0.8f, 2.25f, 0.045f), _wood);
        kit.Cylinder(new Vector3(-0.45f, 0.3f, 0.17f), new Vector3(-0.45f, 1.1f, 0.17f), 0.12f, _wood, 12);
        kit.Cylinder(new Vector3(-0.45f, 1.1f, 0.17f), new Vector3(-0.45f, 1.22f, 0.17f), 0.05f, _metal, 8);
        kit.Cylinder(new Vector3(-0.45f, 1.18f, 0.17f), new Vector3(-0.3f, 0.6f, 0.28f), 0.015f, _trim, 4);
        kit.Cylinder(new Vector3(0.4f, 1.35f, 0.18f), new Vector3(0.4f, 1.85f, 0.18f), 0.0f, 0.17f, _wood, 12);
        kit.Cylinder(new Vector3(0.15f, 0.5f, 0.1f), new Vector3(0.15f, 1.2f, 0.1f), 0.025f, _wood, 6);
        kit.Box(new Vector3(0.08f, 1.08f, 0.07f), new Vector3(0.35f, 1.22f, 0.13f), _metal);
        kit.Cylinder(new Vector3(0.62f, 0.3f, 0.1f), new Vector3(0.62f, 1.25f, 0.1f), 0.025f, _wood, 6);
        kit.Box(new Vector3(0.5f, 0.1f, 0.08f), new Vector3(0.74f, 0.4f, 0.12f), _metal);
    }

    // a changing-room hook rail with a few bags and towels left on it
    private static void HookRail(Kit kit)
    {
        kit.Box(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0.25f, 0.05f), _wood);
        var random = new System.Random(3);
        for (int i = 0; i < 8; i++)
        {
            float x = -1.75f + i * 0.5f;
            kit.Box(new Vector3(x - 0.02f, 0.08f, 0.05f), new Vector3(x + 0.02f, 0.12f, 0.2f), _metal);
            double r = random.NextDouble();
            if (r < 0.3)
                kit.Box(new Vector3(x - 0.2f, -0.65f, 0.08f), new Vector3(x + 0.2f, 0.06f, 0.24f), _fabric);
            else if (r < 0.5)
                kit.Box(new Vector3(x - 0.18f, -0.9f, 0.1f), new Vector3(x + 0.18f, 0.08f, 0.13f), _fabricLight);
        }
    }

    private static void Globe(Kit kit)
    {
        kit.Cylinder(Vector3.zero, new Vector3(0f, 0.04f, 0f), 0.16f, _trim, 12);
        kit.Cylinder(new Vector3(0f, 0.04f, 0f), new Vector3(0f, 0.22f, 0f), 0.02f, _metal, 6);
        kit.Sphere(new Vector3(0f, 0.5f, 0f), 0.26f, _paint);
        for (int i = 0; i < 8; i++)
        {
            float a0 = Mathf.Lerp(-100f, 100f, i / 8f) * Mathf.Deg2Rad, a1 = Mathf.Lerp(-100f, 100f, (i + 1) / 8f) * Mathf.Deg2Rad;
            kit.Cylinder(new Vector3(Mathf.Sin(a0) * 0.3f, 0.5f - Mathf.Cos(a0) * 0.3f, 0f), new Vector3(Mathf.Sin(a1) * 0.3f, 0.5f - Mathf.Cos(a1) * 0.3f, 0f), 0.012f, _metal, 4);
        }
    }

    // The plaster bust on a tall plinth that stood in every school's hall - a poet, a scientist, someone
    // the children could no longer name. Faceless, like everyone here.
    private static void Bust(Kit kit)
    {
        kit.Box(new Vector3(-0.6f, 0f, -0.6f), new Vector3(0.6f, 0.18f, 0.6f), _paint);
        kit.Box(new Vector3(-0.45f, 0.18f, -0.45f), new Vector3(0.45f, 2.1f, 0.45f), _paint);
        kit.Box(new Vector3(-0.55f, 2.1f, -0.55f), new Vector3(0.55f, 2.2f, 0.55f), _paint);
        kit.Box(new Vector3(-0.4f, 2.2f, -0.24f), new Vector3(0.4f, 2.42f, 0.24f), _porcelain);
        kit.Cylinder(new Vector3(-0.42f, 2.6f, 0f), new Vector3(0.42f, 2.6f, 0f), 0.2f, _porcelain, 16);
        kit.Box(new Vector3(-0.42f, 2.42f, -0.2f), new Vector3(0.42f, 2.6f, 0.2f), _porcelain);
        kit.Cylinder(new Vector3(0f, 2.7f, 0f), new Vector3(0f, 2.92f, 0.02f), 0.11f, _porcelain, 12);
        kit.Sphere(new Vector3(0f, 3.05f, 0.03f), 0.2f, _porcelain);
        kit.Sphere(new Vector3(0f, 3.16f, 0.01f), 0.2f, _porcelain);
        // and it wears the mask the children wear
        var mask = AssetDatabase.LoadAssetAtPath<Material>(BustMaskPath);
        if (mask != null)
            kit.Card(new Vector3(0f, 3.1f, 0.245f), 0.34f, 0.44f, 0.25f, mask);
        kit.Box(new Vector3(-0.3f, 0.9f, 0.45f), new Vector3(0.3f, 1.1f, 0.47f), _trim);   // the name plate
    }

    private const string BustMaskPath = "Assets/_Project/Materials/SchoolFigures/SchoolMask_Smile.mat";

    // a glass cabinet of the school's sports cups, pennants and plaques
    private static void TrophyCabinet(Kit kit)
    {
        const float w = 1f, h = 3.2f, d = 0.6f;
        kit.Box(new Vector3(-w, 0f, 0f), new Vector3(-w + 0.05f, h, d), _wood);
        kit.Box(new Vector3(w - 0.05f, 0f, 0f), new Vector3(w, h, d), _wood);
        kit.Box(new Vector3(-w, h - 0.05f, 0f), new Vector3(w, h, d), _wood);
        kit.Box(new Vector3(-w, 0f, 0f), new Vector3(w, 0.7f, d), _wood);
        kit.Box(new Vector3(-w + 0.05f, 0.7f, 0f), new Vector3(w - 0.05f, h - 0.05f, 0.03f), _fabric);
        var random = new System.Random(23);
        foreach (float shelf in new[] { 0.7f, 1.55f, 2.35f })
        {
            if (shelf > 1f)
                kit.Box(new Vector3(-w + 0.05f, shelf - 0.03f, 0.03f), new Vector3(w - 0.05f, shelf, d - 0.05f), _glass);
            for (float x = -0.7f; x <= 0.71f; x += 0.35f)
            {
                double r = random.NextDouble();
                var at = new Vector3(x, shelf, 0.3f);
                if (r < 0.55)
                    Trophy(kit, at, 0.8f + (float)random.NextDouble() * 0.5f, lid: random.NextDouble() < 0.4);
                else if (r < 0.8)
                {
                    // a pennant on a little stand
                    kit.Cylinder(at, at + Vector3.up * 0.5f, 0.01f, _metal, 4);
                    kit.Box(at + new Vector3(0.01f, 0.2f, -0.005f), at + new Vector3(0.2f, 0.48f, 0.005f), _fabricLight);
                }
                else
                {
                    // a plaque leaning on the back
                    kit.Push(at + new Vector3(0f, 0f, -0.15f), Quaternion.Euler(-12f, 0f, 0f));
                    kit.Box(new Vector3(-0.13f, 0f, 0f), new Vector3(0.13f, 0.34f, 0.03f), _wood);
                    kit.Box(new Vector3(-0.09f, 0.05f, 0.03f), new Vector3(0.09f, 0.28f, 0.035f), _metal);
                    kit.Pop();
                }
            }
        }
        kit.Box(new Vector3(-w + 0.05f, 0.7f, d - 0.03f), new Vector3(w - 0.05f, h - 0.05f, d - 0.015f), _glass);
        kit.Box(new Vector3(-0.02f, 0.7f, d - 0.04f), new Vector3(0.02f, h - 0.05f, d), _wood);
    }

    // A sports cup, turned on a lathe: a stepped foot, a knotted stem, a deep bowl with two curled
    // handles - and sometimes a lid with a knob.
    private static void Trophy(Kit kit, Vector3 at, float size, bool lid)
    {
        Vector2 P(float r, float y) => new Vector2(r * size, y * size);
        kit.Lathe(at, new[]
        {
            P(0f, 0f), P(0.09f, 0f), P(0.09f, 0.02f), P(0.075f, 0.03f), P(0.05f, 0.045f), P(0.025f, 0.06f), P(0.02f, 0.16f),
            P(0.04f, 0.18f), P(0.02f, 0.2f), P(0.025f, 0.22f), P(0.06f, 0.25f), P(0.095f, 0.3f), P(0.115f, 0.37f), P(0.12f, 0.44f),
            P(0.11f, 0.44f), P(0.1f, 0.38f), P(0.07f, 0.31f), P(0f, 0.28f),
        }, _metal, 24);
        foreach (float side in new[] { -1f, 1f })
        {
            Vector3 prev = Vector3.zero;
            for (int i = 0; i <= 8; i++)
            {
                float t = i / 8f * Mathf.PI;
                var p = at + new Vector3(side * (0.115f + Mathf.Sin(t) * 0.07f), 0.4f - t / Mathf.PI * 0.12f, 0f) * size;
                if (i > 0)
                    kit.Cylinder(prev, p, 0.008f * size, _metal, 6);
                prev = p;
            }
        }
        if (lid)
            kit.Lathe(at + Vector3.up * 0.44f * size, new[] { P(0f, 0f), P(0.12f, 0f), P(0.11f, 0.02f), P(0.04f, 0.05f), P(0.03f, 0.07f), P(0.04f, 0.09f), P(0f, 0.11f) }, _metal, 20);
    }

    // A worn carpet runner from the door (long along z, from z = 0): a dark field with light borders
    // and a row of diamonds down the middle.
    private static void Runner(Kit kit, float width, float length)
    {
        float half = width * 0.5f;
        kit.Box(new Vector3(-half, 0f, 0f), new Vector3(half, 0.012f, length), _fabric);
        foreach (float x in new[] { -half + 0.12f, half - 0.32f })
            kit.Box(new Vector3(x, 0.012f, 0.12f), new Vector3(x + 0.2f, 0.016f, length - 0.12f), _fabricLight);
        kit.Box(new Vector3(-half + 0.12f, 0.012f, 0.12f), new Vector3(half - 0.12f, 0.016f, 0.32f), _fabricLight);
        kit.Box(new Vector3(-half + 0.12f, 0.012f, length - 0.32f), new Vector3(half - 0.12f, 0.016f, length - 0.12f), _fabricLight);
        for (float z = 1.2f; z < length - 0.8f; z += 1.6f)
        {
            kit.Push(new Vector3(0f, 0.012f, z), Quaternion.Euler(0f, 45f, 0f));
            kit.Box(new Vector3(-0.38f, 0f, -0.38f), new Vector3(0.38f, 0.004f, 0.38f), _fabricLight);
            kit.Box(new Vector3(-0.26f, 0.004f, -0.26f), new Vector3(0.26f, 0.006f, 0.26f), _fabric);
            kit.Pop();
        }
    }

    // Two rows of joined hall seats, back to back on one frame (long along x): four plywood seats facing
    // +z, four facing -z, armrests between. The seats are at the benches' height, so kids can sit there.
    private static void SeatRows(Kit kit)
    {
        const float pitch = 0.8f, seat = 0.8f;
        foreach (float x in new[] { -1.4f, 0f, 1.4f })
        {
            kit.Box(new Vector3(x - 0.05f, 0f, -0.55f), new Vector3(x + 0.05f, 0.06f, 0.55f), _metal);
            kit.Cylinder(new Vector3(x, 0.06f, 0f), new Vector3(x, seat - 0.12f, 0f), 0.04f, _metal, 8);
        }
        kit.Box(new Vector3(-1.65f, seat - 0.14f, -0.06f), new Vector3(1.65f, seat - 0.08f, 0.06f), _metal);
        foreach (float side in new[] { -1f, 1f })
        {
            for (int i = 0; i < 4; i++)
            {
                float x = -1.2f + i * pitch;
                float z0 = side * 0.16f, z1 = side * 0.7f;
                kit.Box(new Vector3(x - 0.36f, seat - 0.06f, Mathf.Min(z0, z1)), new Vector3(x + 0.36f, seat, Mathf.Max(z0, z1)), _wood);
                kit.Push(new Vector3(x, seat, side * 0.12f), Quaternion.Euler(side * -8f, 0f, 0f));
                kit.Box(new Vector3(-0.35f, 0.12f, -0.025f), new Vector3(0.35f, 0.66f, 0.025f), _wood);
                kit.Pop();
            }
            for (int i = 0; i <= 4; i++)
            {
                float x = -1.6f + i * pitch;
                kit.Box(new Vector3(x - 0.03f, seat - 0.08f, side * 0.1f - 0.03f), new Vector3(x + 0.03f, seat + 0.3f, side * 0.1f + 0.03f), _metal);
                kit.Box(new Vector3(x - 0.04f, seat + 0.3f, Mathf.Min(side * 0.1f, side * 0.6f)), new Vector3(x + 0.04f, seat + 0.34f, Mathf.Max(side * 0.1f, side * 0.6f)), _wood);
            }
        }
    }

    // a long tiled planter (long along x), waist-high, full of Gogo greenery
    private static void Planter(Kit kit)
    {
        const float hx = 1.1f, hz = 0.42f, h = 0.9f;
        kit.Box(new Vector3(-hx, 0f, -hz), new Vector3(hx, h, hz), _smallTiles);
        kit.Box(new Vector3(-hx - 0.04f, h, -hz - 0.04f), new Vector3(hx + 0.04f, h + 0.06f, hz + 0.04f), _wood);
        kit.Box(new Vector3(-hx + 0.05f, h - 0.02f, -hz + 0.05f), new Vector3(hx - 0.05f, h + 0.04f, hz - 0.05f), _leather);
        Model(kit, Gogo + "Decoration_Plant_09_01.prefab", new Vector3(-0.55f, h, 0f), 0f, 0.8f, Foliage);
        Model(kit, Gogo + "Decoration_Plant_05_01.prefab", new Vector3(0.15f, h, 0.05f), 70f, 0.9f, Foliage);
        Model(kit, Gogo + "Decoration_Plant_07_01.prefab", new Vector3(0.7f, h, -0.05f), 140f, 1f, Foliage);
    }

    // a drinking fountain against the wall: a porcelain bowl on a pedestal with its bubbler
    private static void DrinkingFountain(Kit kit)
    {
        var centre = new Vector3(0f, 0f, 0.35f);
        kit.Lathe(centre, new[]
        {
            new Vector2(0f, 0f), new Vector2(0.17f, 0f), new Vector2(0.15f, 0.06f), new Vector2(0.11f, 0.15f), new Vector2(0.1f, 0.95f),
            new Vector2(0.13f, 1.05f), new Vector2(0.24f, 1.15f), new Vector2(0.3f, 1.25f), new Vector2(0.28f, 1.26f), new Vector2(0.18f, 1.18f), new Vector2(0f, 1.15f),
        }, _porcelain, 24);
        kit.Cylinder(centre + Vector3.up * 1.15f, centre + Vector3.up * 1.32f, 0.025f, _metal, 10);
        kit.Cylinder(new Vector3(0f, 0.9f, 0f), new Vector3(0f, 0.9f, 0.27f), 0.03f, _metal, 10);
        kit.Cylinder(new Vector3(0.17f, 1.27f, 0.6f), new Vector3(0.17f, 1.31f, 0.6f), 0.04f, _metal, 10);
    }

    // a tall wire plant stand with a lily in a vase on top
    private static void PlantStand(Kit kit)
    {
        const float top = 1.3f;
        for (int i = 0; i < 3; i++)
        {
            float a = i * 120f * Mathf.Deg2Rad;
            var foot = new Vector3(Mathf.Cos(a) * 0.3f, 0f, Mathf.Sin(a) * 0.3f);
            var head = new Vector3(Mathf.Cos(a) * 0.18f, top, Mathf.Sin(a) * 0.18f);
            kit.Cylinder(foot, head, 0.015f, _metal, 6);
        }
        kit.Cylinder(new Vector3(0f, top - 0.02f, 0f), new Vector3(0f, top, 0f), 0.22f, _metal, 20);
        string vase = Gogo + "Decoration_Vase_07_01.prefab";
        Model(kit, vase, new Vector3(0f, top, 0f), 0f, 0.9f, Keep);
        Model(kit, Gogo + "Decoration_Plant_06_01.prefab", new Vector3(0f, top + ModelHeight(vase) * 0.9f * 0.8f, 0f), 0f, 0.9f, Foliage);
    }

    // a short round slab (axis along `axis`), like a clock body or a hob ring
    private static void Disc(Kit kit, Vector3 centre, Vector3 axis, float radius, float thickness, Material material) =>
        kit.Cylinder(centre, centre + axis.normalized * thickness, radius, material, 20);

    // ── Models from the project's asset packs ────────────────────────────────
    // The Gogo plants and vases (already white, like everything here) and the Pandazole kitchen and
    // tableware (in gray: SchoolProps makes a desaturated copy of their colour atlas), merged into the
    // props' own meshes so they cost no more than the rest.

    private const string Gogo = "Assets/Gogo Casual Pack/Gogo Casual Free Plants Pack/Prefabs/";
    private const string Panda = "Assets/Pandazole_Lowpoly_Asset_Bundle/Pandazole Kitchen Food/Prefabs/";

    /// <summary>A model (a prefab's meshes) at <paramref name="position"/>, turned by <paramref name="yaw"/>,
    /// scaled by <paramref name="scale"/>; its materials go through <paramref name="materialFor"/> (gray by default).</summary>
    private static void Model(Kit kit, string path, Vector3 position, float yaw, float scale, Func<Material, Material> materialFor = null)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"[SchoolProps] Missing model {path}.");
            return;
        }
        var root = Matrix4x4.TRS(prefab.transform.position, prefab.transform.rotation, Vector3.one).inverse;
        var place = Matrix4x4.TRS(position, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale);
        foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
        {
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null || filter.sharedMesh == null)
                continue;
            kit.Mesh(filter.sharedMesh, place * root * filter.transform.localToWorldMatrix, renderer.sharedMaterials, materialFor ?? Gray);
        }
    }

    /// <summary>The model's height (at scale 1), for stacking things on it.</summary>
    private static float ModelHeight(string path)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            return 0f;
        float top = 0f;
        foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            if (filter.sharedMesh != null)
                top = Mathf.Max(top, filter.transform.localToWorldMatrix.MultiplyPoint3x4(filter.sharedMesh.bounds.max).y);
        return top;
    }

    // plants' leaves in a soft gray, their white vases left as they are
    private static Material Foliage(Material original) => _foliage;
    private static Material Keep(Material original) => original;

    private static readonly Dictionary<Material, Material> GrayCopies = new();

    // a desaturated copy of a material (and of its texture), made once into Materials/School
    private static Material Gray(Material original)
    {
        if (original == null)
            return _metal;
        if (GrayCopies.TryGetValue(original, out var cached) && cached != null)
            return cached;
        string name = "Gray_" + original.name.Replace(" ", "");
        string path = $"{MaterialFolder}/{name}.mat";
        var gray = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (gray == null)
        {
            gray = new Material(original) { name = name };
            if (gray.HasProperty("_BaseColor"))
            {
                var c = gray.GetColor("_BaseColor");
                gray.SetColor("_BaseColor", new Color(c.grayscale, c.grayscale, c.grayscale, c.a));
            }
            if (gray.HasProperty("_BaseMap") && gray.GetTexture("_BaseMap") is Texture2D texture)
                gray.SetTexture("_BaseMap", GrayTexture(texture));
            AssetDatabase.CreateAsset(gray, path);
        }
        GrayCopies[original] = gray;
        return gray;
    }

    private static Texture2D GrayTexture(Texture2D source)
    {
        string path = $"{MaterialFolder}/Textures/Gray_{source.name}.png";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null)
            return existing;
        var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(source, rt);
        var previous = RenderTexture.active;
        RenderTexture.active = rt;
        var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        var pixels = copy.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            byte l = (byte)Mathf.Clamp(pixels[i].r * 0.3f + pixels[i].g * 0.59f + pixels[i].b * 0.11f, 0f, 255f);
            pixels[i] = new Color32(l, l, l, pixels[i].a);
        }
        copy.SetPixels32(pixels);
        System.IO.Directory.CreateDirectory(MaterialFolder + "/Textures");
        System.IO.File.WriteAllBytes(path, copy.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(copy);
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // ── Saving ───────────────────────────────────────────────────────────────

    private static void AddDrawing(GameObject go, string drawing, Vector3 position, Vector2 size)
    {
        var sprite = SchoolDrawings.Get(drawing);
        if (sprite == null)
            return;
        var child = new GameObject("Drawing");
        child.transform.SetParent(go.transform, false);
        child.transform.localPosition = position;
        // a sprite faces -z; turn it to face out of the wall (+z); fit it in `size`, keeping its proportions
        child.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        Vector2 native = sprite.bounds.size;
        float scale = Mathf.Min(size.x / native.x, size.y / native.y);
        child.transform.localScale = new Vector3(scale, scale, 1f);
        child.AddComponent<SpriteRenderer>().sprite = sprite;
    }

    private static GameObject Save(string name, Kit kit, bool shadows, bool collider, Action<GameObject> decorate)
    {
        System.IO.Directory.CreateDirectory(PrefabFolder);
        System.IO.Directory.CreateDirectory(MeshFolder);
        string meshPath = $"{MeshFolder}/{name}.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null)
        {
            mesh = new Mesh { name = name };
            kit.WriteTo(mesh);
            AssetDatabase.CreateAsset(mesh, meshPath);
        }
        else
        {
            kit.WriteTo(mesh);
            EditorUtility.SetDirty(mesh);
        }

        var go = new GameObject(name);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = kit.Materials;
        renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        if (collider)
        {
            var box = go.AddComponent<BoxCollider>();
            box.center = mesh.bounds.center;
            box.size = mesh.bounds.size;
        }
        decorate?.Invoke(go);

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabFolder}/{name}.prefab");
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    // ── Kit: a mesh built out of boxes, tapered cylinders and spheres, a submesh per material ──

    private sealed class Kit
    {
        private readonly List<Vector3> _vertices = new(), _normals = new();
        private readonly List<Vector2> _uvs = new();
        private readonly List<Material> _materials = new();
        private readonly List<List<int>> _triangles = new();
        private readonly Stack<Matrix4x4> _stack = new();
        private Matrix4x4 _matrix = Matrix4x4.identity;

        public Material[] Materials => _materials.ToArray();

        public void Push(Vector3 position, Quaternion rotation)
        {
            _stack.Push(_matrix);
            _matrix *= Matrix4x4.TRS(position, rotation, Vector3.one);
        }

        public void Pop() => _matrix = _stack.Pop();

        private List<int> Triangles(Material material)
        {
            int i = _materials.IndexOf(material);
            if (i < 0)
            {
                _materials.Add(material);
                _triangles.Add(new List<int>());
                i = _materials.Count - 1;
            }
            return _triangles[i];
        }

        private int Vertex(Vector3 local, Vector3 normal, Vector2 uv)
        {
            _vertices.Add(_matrix.MultiplyPoint3x4(local));
            _normals.Add(_matrix.MultiplyVector(normal).normalized);
            _uvs.Add(uv);
            return _vertices.Count - 1;
        }

        // a triangle facing `normal` (whatever order the corners come in)
        private void Triangle(List<int> tris, int a, int b, int c, Vector3 normal)
        {
            Vector3 cross = Vector3.Cross(_vertices[b] - _vertices[a], _vertices[c] - _vertices[a]);
            if (Vector3.Dot(cross, _matrix.MultiplyVector(normal)) < 0f)
                (b, c) = (c, b);
            tris.Add(a); tris.Add(b); tris.Add(c);
        }

        // UVs in metres on the face's own plane, so textures keep their scale on every prop
        private void Quad(Material material, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector3 uAxis, Vector3 vAxis)
        {
            var tris = Triangles(material);
            Vector2 Uv(Vector3 p) => new Vector2(Vector3.Dot(p, uAxis), Vector3.Dot(p, vAxis));
            int ia = Vertex(a, normal, Uv(a)), ib = Vertex(b, normal, Uv(b)), ic = Vertex(c, normal, Uv(c)), id = Vertex(d, normal, Uv(d));
            Triangle(tris, ia, ib, ic, normal);
            Triangle(tris, ia, ic, id, normal);
        }

        public void Box(Vector3 min, Vector3 max, Material material)
        {
            Vector3 lo = Vector3.Min(min, max), hi = Vector3.Max(min, max);
            float x0 = lo.x, y0 = lo.y, z0 = lo.z, x1 = hi.x, y1 = hi.y, z1 = hi.z;
            Quad(material, new(x0, y0, z1), new(x1, y0, z1), new(x1, y1, z1), new(x0, y1, z1), Vector3.forward, Vector3.right, Vector3.up);
            Quad(material, new(x0, y0, z0), new(x1, y0, z0), new(x1, y1, z0), new(x0, y1, z0), Vector3.back, Vector3.right, Vector3.up);
            Quad(material, new(x1, y0, z0), new(x1, y0, z1), new(x1, y1, z1), new(x1, y1, z0), Vector3.right, Vector3.forward, Vector3.up);
            Quad(material, new(x0, y0, z0), new(x0, y0, z1), new(x0, y1, z1), new(x0, y1, z0), Vector3.left, Vector3.forward, Vector3.up);
            Quad(material, new(x0, y1, z0), new(x1, y1, z0), new(x1, y1, z1), new(x0, y1, z1), Vector3.up, Vector3.right, Vector3.forward);
            Quad(material, new(x0, y0, z0), new(x1, y0, z0), new(x1, y0, z1), new(x0, y0, z1), Vector3.down, Vector3.right, Vector3.forward);
        }

        public void Cylinder(Vector3 a, Vector3 b, float radius, Material material, int segments = 12, bool caps = true) =>
            Cylinder(a, b, radius, radius, material, segments, caps);

        /// <summary>A round (tapered) tube from a (radius ra) to b (radius rb), smooth-shaded, optionally capped.</summary>
        public void Cylinder(Vector3 a, Vector3 b, float ra, float rb, Material material, int segments = 12, bool caps = true)
        {
            Vector3 axis = b - a;
            float length = axis.magnitude;
            if (length < 1e-5f)
                return;
            axis /= length;
            Vector3 u = Vector3.Cross(axis, Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
            Vector3 w = Vector3.Cross(axis, u);
            float slope = (ra - rb) / length;
            var tris = Triangles(material);

            int start = _vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments * Mathf.PI * 2f;
                Vector3 radial = u * Mathf.Cos(t) + w * Mathf.Sin(t);
                Vector3 normal = (radial + axis * slope).normalized;
                float around = t * Mathf.Max(ra, rb);
                Vertex(a + radial * ra, normal, new Vector2(around, 0f));
                Vertex(b + radial * rb, normal, new Vector2(around, length));
            }
            for (int i = 0; i < segments; i++)
            {
                int i0 = start + i * 2, i1 = i0 + 1, i2 = i0 + 2, i3 = i0 + 3;
                Vector3 mid = (u * Mathf.Cos((i + 0.5f) / segments * Mathf.PI * 2f) + w * Mathf.Sin((i + 0.5f) / segments * Mathf.PI * 2f));
                Triangle(tris, i0, i2, i1, mid);
                Triangle(tris, i1, i2, i3, mid);
            }

            if (!caps)
                return;
            foreach (var (centre, r, normal) in new[] { (a, ra, -axis), (b, rb, axis) })
            {
                if (r <= 1e-4f)
                    continue;
                int c = Vertex(centre, normal, Vector2.zero);
                int first = _vertices.Count;
                for (int i = 0; i <= segments; i++)
                {
                    float t = i / (float)segments * Mathf.PI * 2f;
                    Vector3 p = centre + (u * Mathf.Cos(t) + w * Mathf.Sin(t)) * r;
                    Vertex(p, normal, new Vector2(Vector3.Dot(p, u), Vector3.Dot(p, w)));
                }
                for (int i = 0; i < segments; i++)
                    Triangle(tris, c, first + i, first + i + 1, normal);
            }
        }

        /// <summary>Another mesh's geometry (with its own UVs), placed by <paramref name="matrix"/>; each of its
        /// submeshes gets the material <paramref name="materialFor"/> picks for the original one.</summary>
        public void Mesh(Mesh mesh, Matrix4x4 matrix, Material[] materials, Func<Material, Material> materialFor)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;
            int start = _vertices.Count;
            for (int i = 0; i < vertices.Length; i++)
                Vertex(matrix.MultiplyPoint3x4(vertices[i]), normals.Length > i ? matrix.MultiplyVector(normals[i]) : Vector3.up,
                    uvs.Length > i ? uvs[i] : Vector2.zero);
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                var original = materials.Length > 0 ? materials[Mathf.Min(sub, materials.Length - 1)] : null;
                var tris = Triangles(materialFor(original));
                foreach (int index in mesh.GetTriangles(sub))
                    tris.Add(start + index);
            }
        }

        /// <summary>A smooth surface of revolution round the vertical axis through <paramref name="centre"/>:
        /// <paramref name="profile"/> is (radius, height) from the bottom up - go back down inside for a cup.</summary>
        public void Lathe(Vector3 centre, Vector2[] profile, Material material, int segments = 24)
        {
            var tris = Triangles(material);
            int rings = profile.Length, start = _vertices.Count;
            var ringNormals = new Vector2[rings];
            for (int j = 0; j < rings; j++)
            {
                Vector2 tangent = (profile[Mathf.Min(rings - 1, j + 1)] - profile[Mathf.Max(0, j - 1)]).normalized;
                ringNormals[j] = new Vector2(tangent.y, -tangent.x);
                for (int s = 0; s <= segments; s++)
                {
                    float a = s / (float)segments * Mathf.PI * 2f;
                    var radial = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    Vertex(centre + radial * profile[j].x + Vector3.up * profile[j].y, radial * ringNormals[j].x + Vector3.up * ringNormals[j].y,
                        new Vector2(a * Mathf.Max(0.05f, profile[j].x), profile[j].y));
                }
            }
            for (int j = 0; j < rings - 1; j++)
            {
                Vector2 n2 = (ringNormals[j] + ringNormals[j + 1]).normalized;
                for (int s = 0; s < segments; s++)
                {
                    float a = (s + 0.5f) / segments * Mathf.PI * 2f;
                    var normal = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * n2.x + Vector3.up * n2.y;
                    int i0 = start + j * (segments + 1) + s, i1 = i0 + 1, i2 = i0 + segments + 1, i3 = i2 + 1;
                    Triangle(tris, i0, i2, i1, normal);
                    Triangle(tris, i1, i2, i3, normal);
                }
            }
        }

        /// <summary>A card facing +z, w x h round <paramref name="centre"/>, bent back at its sides round a
        /// circle of radius <paramref name="bend"/>, with the whole texture on it (0..1, read from the front).</summary>
        public void Card(Vector3 centre, float w, float h, float bend, Material material, int columns = 8)
        {
            var tris = Triangles(material);
            int start = _vertices.Count;
            for (int c = 0; c <= columns; c++)
            {
                float u = c / (float)columns, x = (u - 0.5f) * w;
                float z = -x * x / (2f * bend);
                var normal = new Vector3(x / bend, 0f, 1f).normalized;
                // looking at its front (from +z), the viewer's right is -x
                Vertex(centre + new Vector3(-x, -h * 0.5f, z), new Vector3(-normal.x, 0f, normal.z), new Vector2(u, 0f));
                Vertex(centre + new Vector3(-x, h * 0.5f, z), new Vector3(-normal.x, 0f, normal.z), new Vector2(u, 1f));
            }
            for (int c = 0; c < columns; c++)
            {
                int i0 = start + c * 2, i1 = i0 + 1, i2 = i0 + 2, i3 = i0 + 3;
                Triangle(tris, i0, i2, i1, Vector3.forward);
                Triangle(tris, i1, i2, i3, Vector3.forward);
            }
        }

        public void Sphere(Vector3 centre, float radius, Material material, int segments = 16, int rings = 10)
        {
            var tris = Triangles(material);
            int start = _vertices.Count;
            for (int r = 0; r <= rings; r++)
            {
                float v = r / (float)rings, phi = v * Mathf.PI;
                for (int s = 0; s <= segments; s++)
                {
                    float theta = s / (float)segments * Mathf.PI * 2f;
                    var n = new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta));
                    Vertex(centre + n * radius, n, new Vector2(theta * radius, phi * radius));
                }
            }
            for (int r = 0; r < rings; r++)
            for (int s = 0; s < segments; s++)
            {
                int i0 = start + r * (segments + 1) + s, i1 = i0 + 1, i2 = i0 + segments + 1, i3 = i2 + 1;
                Vector3 n = (_vertices[i0] - _matrix.MultiplyPoint3x4(centre)).normalized;
                Vector3 local = _matrix.inverse.MultiplyVector(n);
                Triangle(tris, i0, i2, i1, local);
                Triangle(tris, i1, i2, i3, local);
            }
        }

        public void WriteTo(Mesh mesh)
        {
            mesh.Clear();
            mesh.indexFormat = _vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.subMeshCount = _materials.Count;
            for (int i = 0; i < _triangles.Count; i++)
                mesh.SetTriangles(_triangles[i], i);
            mesh.RecalculateBounds();
        }
    }
}
#endif
