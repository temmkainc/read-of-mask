// Tools → Read of Mask → School Figures → Make Prefabs / Place In School
// The faceless kids of Level 09: the mannequin body (the same one under the TV-headed characters),
// all white, each wearing a drawn copy of the player's own mask (SchoolMaskArt, DrawnMask) with one
// of a few mouths. About the player's height, stocky, with a
// slightly big head. They stand about the entrance hall and the corridor and sit at the dining room
// tables, each doing one idle thing on the spot, and all of them watch the player.
//
// "Place In School" rebuilds Level_09/Figures from the list below, so move anything you add by
// hand out of there (or stop using the tool) before running it again.

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Activity = SchoolFigure.Activity;

public static class SchoolFigureTools
{
    private const string ModelPath = "Assets/Idle MoCap/MorroMan_Idle_MoCap.fbx";
    private const string ControllerPath = "Assets/_Project/Animations/SchoolFigures/SchoolFigure.controller";
    private const string MaterialFolder = "Assets/_Project/Materials/SchoolFigures";
    private const string BodyMaterialPath = "Assets/_Project/Materials/White.mat";
    private const string MaskCardPath = "Assets/_Project/Models/SchoolFigures/SchoolMaskCard.asset";
    public const string KidPrefabPath = "Assets/_Project/Prefabs/SchoolFigures/SchoolKid.prefab";

    private const float ModelHeight = 2.88f;       // the mannequin's height at scale 1
    private const float KidHeadScale = 1.18f, KidChestScale = 1.12f;
    private const float Floor = 0.432f;           // SchoolBuilder's floor
    private const float BenchTop = Floor + 0.8f;  // SchoolBuilder's dining benches
    // in the sitting clips, the seat the figure rests on is this high and this far behind the root (scale 1)
    private const float SitSeatHeight = 0.655f, SitSeatBack = 0.193f;

    // The mask is a drawing (SchoolMaskArt) on a card made in the head bone's own space, where the face
    // looks down -Y and up is +Z. The card runs from the chin to the forehead and a little past the
    // face on both sides, bent round the face and back over the brow, just clear of it all the way.
    private const float CardWidth = 0.31f, CardBottom = -0.035f, CardTop = 0.36f;
    private const float CardBend = 0.3f;     // the radius it's bent round the face with
    private const float CardGap = 0.016f;    // in front of the face
    // the front of the face down its middle, (height, depth): measured off the mannequin's head mesh
    private static readonly Vector2[] FaceProfile =
    {
        new(-0.02f, -0.125f), new(0.02f, -0.135f), new(0.06f, -0.143f), new(0.10f, -0.147f), new(0.14f, -0.150f),
        new(0.18f, -0.150f), new(0.22f, -0.146f), new(0.26f, -0.140f), new(0.30f, -0.098f), new(0.34f, -0.060f),
    };

    private const string Mocap = "Assets/Idle MoCap/Animations/";

    // the clip each activity plays (a state of the same name in the controller)
    private static (Activity activity, AnimationClip clip)[] Clips() => new[]
    {
        (Activity.Stand, FbxClip(Mocap + "Idle_Stance_01_MB_v01.fbx")),
        (Activity.Fidget, FbxClip(Mocap + "Idle_Stance_02_MB_v01.fbx")),
        (Activity.LookAround, FbxClip(Mocap + "Looking_Around_01_MB_v01.fbx")),
        (Activity.Search, FbxClip(Mocap + "Looking_Around_02_MB_v01.fbx")),
        (Activity.Think, FbxClip(Mocap + "Looking_Around_Holding_Object_MB_v01.fbx")),
        (Activity.Shiver, FbxClip(Mocap + "Cold_Idle_01_MB_v01.fbx")),
        (Activity.RubHands, FbxClip(Mocap + "Cold_Idle_02_MBv01.fbx")),
        (Activity.Huddle, FbxClip(Mocap + "Cold_Idle_03_MB_v01.fbx")),
        (Activity.Reach, FbxClip(Mocap + "Cold_Idle_04_MB_v02.fbx")),
        (Activity.Cough, FbxClip(Mocap + "Coughing_Idle_MB_v01.fbx")),
        (Activity.Greet, FbxClip(Mocap + "Greeting_Stance_MB_v02.fbx", "Greeting_Stance")),
        (Activity.Sit, AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Idle MoCap/Sitting_Idle.anim")),
        (Activity.SitPlay, AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Idle MoCap/PlayingGamepad.anim")),
    };

    // ── Prefab ───────────────────────────────────────────────────────────────

    [MenuItem("Tools/Read of Mask/School Figures/Make Prefabs")]
    public static void MakePrefabs()
    {
        MakeMaskCard();
        MakePrefab(KidPrefabPath, MakeController(), KidHeadScale, KidChestScale);
        AssetDatabase.SaveAssets();
        Debug.Log("[SchoolFigureTools] Made the kid prefab.");
    }

    // ── Materials ────────────────────────────────────────────────────────────
    // Made once and then left alone, so they can be tuned in the inspector.

    private static Material MaskMaterial(string face)
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(SchoolMaskArt.PathFor(face)) == null)
            SchoolMaskArt.DrawAll();
        return LoadOrMakeMaterial($"{MaterialFolder}/SchoolMask_{face}.mat", "ReadOfMask/DrawnMask", AssetDatabase.LoadAssetAtPath<Texture2D>(SchoolMaskArt.PathFor(face)));
    }

    private static Material LoadOrMakeMaterial(string path, string shader, Texture texture)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;
        System.IO.Directory.CreateDirectory(MaterialFolder);
        material = new Material(Shader.Find(shader));
        if (texture != null)
            material.SetTexture("_MainTex", texture);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    // ── Mask card ────────────────────────────────────────────────────────────

    private static void MakeMaskCard()
    {
        const int columns = 12, rows = 16;
        var vertices = new Vector3[(columns + 1) * (rows + 1)];
        var uvs = new Vector2[vertices.Length];
        for (int r = 0; r <= rows; r++)
        for (int c = 0; c <= columns; c++)
        {
            float u = c / (float)columns, v = r / (float)rows;
            // looking at the face, its right is the head bone's -X
            float x = (0.5f - u) * CardWidth;
            float z = Mathf.Lerp(CardBottom, CardTop, v);
            float y = FaceFront(z) - CardGap + x * x / (2f * CardBend);
            vertices[r * (columns + 1) + c] = new Vector3(x, y, z);
            uvs[r * (columns + 1) + c] = new Vector2(u, v);
        }
        var triangles = new int[columns * rows * 6];
        for (int r = 0, t = 0; r < rows; r++)
        for (int c = 0; c < columns; c++)
        {
            int i = r * (columns + 1) + c;
            triangles[t++] = i; triangles[t++] = i + columns + 1; triangles[t++] = i + 1;
            triangles[t++] = i + 1; triangles[t++] = i + columns + 1; triangles[t++] = i + columns + 2;
        }

        var mesh = AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(MaskCardPath);
        if (mesh == null)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(MaskCardPath));
            mesh = new UnityEngine.Mesh { name = "SchoolMaskCard" };
            AssetDatabase.CreateAsset(mesh, MaskCardPath);
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
    }

    private static float FaceFront(float z)
    {
        if (z <= FaceProfile[0].x)
            return FaceProfile[0].y;
        for (int i = 1; i < FaceProfile.Length; i++)
            if (z <= FaceProfile[i].x)
                return Mathf.Lerp(FaceProfile[i - 1].y, FaceProfile[i].y, Mathf.InverseLerp(FaceProfile[i - 1].x, FaceProfile[i].x, z));
        return FaceProfile[FaceProfile.Length - 1].y;
    }

    private static AnimatorController MakeController()
    {
        AssetDatabase.DeleteAsset(ControllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        // IK pass on, so SchoolFigure can turn their heads to watch the player
        var layers = controller.layers;
        layers[0].iKPass = true;
        controller.layers = layers;
        var machine = controller.layers[0].stateMachine;
        foreach (var (activity, clip) in Clips())
        {
            var state = machine.AddState(activity.ToString());
            state.motion = clip;
            if (activity == Activity.Stand)
                machine.defaultState = state;
        }
        return controller;
    }

    private static AnimationClip FbxClip(string path, string name = null) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c => !c.name.StartsWith("__") && (name == null || c.name == name));

    private static void MakePrefab(string path, RuntimeAnimatorController controller, float headScale, float chestScale)
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/SchoolFigures"))
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "SchoolFigures");

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        var figure = (GameObject)PrefabUtility.InstantiatePrefab(model);
        figure.name = System.IO.Path.GetFileNameWithoutExtension(path);

        // stand it in its idle pose to fit the mask and collider
        var stand = FbxClip(Mocap + "Idle_Stance_01_MB_v01.fbx");
        stand.SampleAnimation(figure, 1f);
        figure.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        stand.SampleAnimation(figure, 1f);
        figure.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        var animator = figure.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        // only animate while on screen (most of the 47 are behind the player or in another room);
        // SchoolFigure animates each one fully on its first frame so it's never caught out of its pose
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        var body = AssetDatabase.LoadAssetAtPath<Material>(BodyMaterialPath);
        foreach (var r in figure.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var materials = r.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
                materials[i] = body;
            r.sharedMaterials = materials;
            r.updateWhenOffscreen = false;
        }

        // the mask card, made in the head bone's own space, so it follows the face whatever the head does
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        var mask = new GameObject("Mask", typeof(MeshFilter), typeof(MeshRenderer));
        mask.transform.SetParent(head, false);
        mask.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(MaskCardPath);
        var maskRenderer = mask.GetComponent<MeshRenderer>();
        maskRenderer.sharedMaterial = MaskMaterial(SchoolMaskArt.Faces[0]);
        maskRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // broader shoulders and chest, scaled evenly so nothing skews; the neck and upper arms are
        // scaled back so the head and arms keep their size
        var neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        var upperBody = neck.parent;
        upperBody.localScale = Vector3.one * chestScale;
        neck.localScale = Vector3.one / chestScale;
        animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).localScale = Vector3.one / chestScale;
        animator.GetBoneTransform(HumanBodyBones.RightUpperArm).localScale = Vector3.one / chestScale;
        head.localScale = Vector3.one * headScale;

        var capsule = figure.AddComponent<CapsuleCollider>();
        capsule.center = new Vector3(0f, ModelHeight * 0.5f, 0f);
        capsule.height = ModelHeight;
        capsule.radius = 0.35f;

        figure.AddComponent<SchoolFigure>();

        PrefabUtility.SaveAsPrefabAsset(figure, path);
        Object.DestroyImmediate(figure);
    }

    // ── Placing them ─────────────────────────────────────────────────────────

    private struct Spot
    {
        public string Name;
        public float Height;
        public Activity Activity;
        public Vector3 Position;
        public float Yaw;
    }

    private static Spot Kid(string name, Activity activity, float x, float z, float yaw, float height = 2.75f) =>
        new Spot { Name = name, Activity = activity, Position = new Vector3(x, Floor, z), Yaw = yaw, Height = height };

    // Kid on a dining bench, facing the table. benchZ is the bench's centre, facing +1 = towards +z.
    private static Spot Sitter(string name, Activity activity, float x, float benchZ, float facing, float height = 2.75f)
    {
        float scale = height / ModelHeight;
        return new Spot
        {
            Name = name, Activity = activity, Height = height,
            Position = new Vector3(x, BenchTop - SitSeatHeight * scale, benchZ + facing * SitSeatBack * scale),
            Yaw = facing > 0f ? 0f : 180f,
        };
    }

    // Kids around a point, facing in towards it.
    private static Spot[] Circle(string name, float x, float z, float radius, float startAngle, params Activity[] activities)
    {
        var spots = new Spot[activities.Length];
        for (int i = 0; i < activities.Length; i++)
        {
            float angle = startAngle + i * 360f / activities.Length;
            var offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
            spots[i] = Kid($"{name}_{i + 1}", activities[i], x + offset.x, z + offset.z, angle + 180f, 2.65f + 0.05f * (i % 3));
        }
        return spots;
    }

    private static Spot[] Spots()
    {
        // Entrance hall: x 22.25..41.75, z -44.78..-25, the way in at x 29.5..34.5 on the south wall,
        // columns at x 26.5 / 37.5, z -39 / -31, the dining room door at (22, -35), the gym's at (42, -35).
        // The way from the entrance up to the corridor and the ways to both doors stay clear.
        var hall = new[]
        {
            Kid("Kid_Hall_Waiting", Activity.Stand, 32f, -36.5f, 180f, 2.8f),
            Kid("Kid_Hall_Reach", Activity.Reach, 35.6f, -41.2f, 210f),
            Kid("Kid_Hall_Entrance_Shiver", Activity.Shiver, 27.9f, -43.4f, 30f, 2.65f),
            Kid("Kid_Hall_Huddle", Activity.Huddle, 23.6f, -43.3f, 50f),
            Kid("Kid_Hall_Think", Activity.Think, 24.2f, -40.2f, 80f, 2.7f),
            Kid("Kid_Hall_Search", Activity.Search, 40.3f, -43f, -35f),
            Kid("Kid_Hall_RubHands", Activity.RubHands, 39.4f, -39.4f, -110f, 2.7f),
            Kid("Kid_Hall_LookAround", Activity.LookAround, 36.4f, -33.6f, 220f, 2.8f),
            Kid("Kid_Hall_Column", Activity.Fidget, 27.6f, -32.8f, 150f, 2.65f),
            Kid("Kid_Hall_Cough", Activity.Cough, 34.9f, -26.8f, 160f),
            // a pair by the gym side, a ring of three by the dining room side
            Kid("Kid_Hall_Pair_1", Activity.Greet, 39.9f, -28.4f, -90f),
            Kid("Kid_Hall_Pair_2", Activity.Fidget, 38.7f, -28.4f, 90f, 2.65f),
        }.Concat(Circle("Kid_Hall_Ring", 24.9f, -28.2f, 1.05f, 0f, Activity.Think, Activity.Greet, Activity.Stand));

        // Corridor: x -14..80, z -24.5..-16.5. Classroom and toilet doors in the north wall at
        // x -1 / 15 / 31 / 47 / 55 / 70; in the south wall the dining room door at x 8.5, the hall
        // open over x 24..40, the gym door at x 57.5; pilasters at x -9 / 3 / 12 / 20 / 27 / 35 / 43 / 51 / 63 / 75.
        // Kept off the doorways, mostly along the walls, with a few out in the middle.
        var corridor = new[]
        {
            Kid("Kid_Corridor_WestEnd", Activity.Stand, -12.5f, -20.5f, 90f, 2.8f),
            Kid("Kid_Corridor_101_West", Activity.Fidget, -6.2f, -17.4f, 180f),
            Kid("Kid_Corridor_101_East", Activity.Think, 5.6f, -17.4f, 200f, 2.7f),
            Kid("Kid_Corridor_Pair_1", Activity.Greet, 10.4f, -17.6f, -90f),
            Kid("Kid_Corridor_Pair_2", Activity.Fidget, 9.2f, -17.6f, 90f, 2.65f),
            Kid("Kid_Corridor_Dining_West", Activity.Cough, 1.4f, -23.6f, 20f),
            Kid("Kid_Corridor_Dining_East", Activity.Shiver, 14.8f, -23.6f, -20f, 2.7f),
            Kid("Kid_Corridor_Reach", Activity.Reach, 18f, -20.6f, 250f, 2.8f),
            Kid("Kid_Corridor_103_West", Activity.RubHands, 25.4f, -17.4f, 180f, 2.65f),
            Kid("Kid_Corridor_103_East", Activity.Search, 38.6f, -17.4f, 190f),
            Kid("Kid_Corridor_104", Activity.Huddle, 49.6f, -17.4f, 170f),
            Kid("Kid_Corridor_Gym_West", Activity.Stand, 53.4f, -23.6f, 10f, 2.7f),
            Kid("Kid_Corridor_Toilets", Activity.Think, 61.5f, -17.5f, 190f, 2.8f),
            Kid("Kid_Corridor_Middle_East", Activity.Fidget, 66.5f, -20.8f, 270f),
            Kid("Kid_Corridor_South_East", Activity.Shiver, 73f, -23.5f, -20f, 2.65f),
            Kid("Kid_Corridor_EastEnd", Activity.Stand, 78.5f, -20.5f, -90f, 2.8f),
        }.Concat(Circle("Kid_Corridor_Ring", 44.5f, -20.6f, 1f, 30f, Activity.LookAround, Activity.Cough, Activity.Stand));

        // Dining room: x -3.75..21.75, the serving counter along x -3.75..-2.25; tables at x 6 / 15,
        // z -41.4 / -36.6 / -31.8, a bench 1.325 m either side of each. Only kids at the tables here.
        var dining = new[]
        {
            Sitter("Kid_Dining_A1_1", Activity.Sit, 4.4f, -42.725f, 1f),
            Sitter("Kid_Dining_A1_2", Activity.SitPlay, 5.7f, -42.725f, 1f, 2.65f),
            Sitter("Kid_Dining_A1_3", Activity.Sit, 7.4f, -40.075f, -1f, 2.8f),
            Sitter("Kid_Dining_A2_1", Activity.SitPlay, 4.6f, -37.925f, 1f),
            Sitter("Kid_Dining_A2_2", Activity.Sit, 5.2f, -35.275f, -1f, 2.7f),
            Sitter("Kid_Dining_A2_3", Activity.Sit, 7.6f, -35.275f, -1f),
            Sitter("Kid_Dining_A3_1", Activity.SitPlay, 6f, -30.475f, -1f, 2.8f),
            Sitter("Kid_Dining_B1_1", Activity.Sit, 14.2f, -40.075f, -1f, 2.65f),
            Sitter("Kid_Dining_B2_1", Activity.Sit, 13.4f, -37.925f, 1f),
            Sitter("Kid_Dining_B2_2", Activity.SitPlay, 14.7f, -37.925f, 1f, 2.7f),
            Sitter("Kid_Dining_B2_3", Activity.Sit, 16.6f, -37.925f, 1f, 2.8f),
            Sitter("Kid_Dining_B2_4", Activity.SitPlay, 15.8f, -35.275f, -1f),
            Sitter("Kid_Dining_B3_1", Activity.Sit, 16.4f, -33.125f, 1f, 2.65f),
        };

        return hall.Concat(corridor).Concat(dining).ToArray();
    }

    [MenuItem("Tools/Read of Mask/School Figures/Place In School")]
    public static void PlaceInSchool()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[SchoolFigureTools] Leave Play mode first.");
            return;
        }

        var kidPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KidPrefabPath);
        if (kidPrefab == null)
        {
            MakePrefabs();
            kidPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KidPrefabPath);
        }

        var level = Object.FindAnyObjectByType<Level09>(FindObjectsInactive.Include);
        if (level == null)
        {
            Debug.LogWarning("[SchoolFigureTools] No Level09 in the open scene.");
            return;
        }

        var old = level.transform.Find("Figures");
        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);
        var group = new GameObject("Figures").transform;
        group.SetParent(level.transform, false);
        Undo.RegisterCreatedObjectUndo(group.gameObject, "Place school figures");

        // each one a little slower or faster than the next, the same every time the tool runs
        var random = new System.Random(9);
        foreach (var spot in Spots())
        {
            var figure = (GameObject)PrefabUtility.InstantiatePrefab(kidPrefab, group);
            figure.name = spot.Name;
            figure.transform.localScale = Vector3.one * (spot.Height / ModelHeight);
            figure.transform.SetPositionAndRotation(spot.Position, Quaternion.Euler(0f, spot.Yaw, 0f));

            var so = new SerializedObject(figure.GetComponent<SchoolFigure>());
            so.FindProperty("_activity").enumValueIndex = (int)spot.Activity;
            so.FindProperty("_animationSpeed").floatValue = 0.6f + 0.3f * (float)random.NextDouble();
            so.ApplyModifiedPropertiesWithoutUndo();
            if (spot.Activity == Activity.Sit || spot.Activity == Activity.SitPlay)
                figure.GetComponent<CapsuleCollider>().enabled = false;

            // half of them wear the player's own smile, the rest one of the other faces
            int face = random.NextDouble() < 0.5 ? 0 : random.Next(1, SchoolMaskArt.Faces.Length);
            var mask = figure.GetComponentsInChildren<MeshRenderer>(true).First(r => r.name == "Mask");
            mask.sharedMaterial = MaskMaterial(SchoolMaskArt.Faces[face]);
        }

        EditorSceneManager.MarkSceneDirty(level.gameObject.scene);
        Debug.Log($"[SchoolFigureTools] Placed {group.childCount} figures in the school.");
    }
}
#endif
