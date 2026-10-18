using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Bakes the faceless guitarist's looping "sitting and playing" clip. It keeps the body, legs and
/// head of Sitting_Idle and, frame by frame, solves the arm muscles so the hands stay on the guitar
/// (which is parented to the hips): the left hand holds the neck, moving between chord positions
/// every bar, and the right hand strums over the sound hole on every beat.
/// Re-run it from the menu after changing the guitar placement or hand targets below; the prefab's
/// guitar must use the same placement (see GetGuitarHipsPose).
/// </summary>
public static class GuitaristPoseBaker
{
    public const string BasePrefabPath = "Assets/_Project/Prefabs/[Interactable]Faceless_Character.prefab";
    public const string BaseClipPath = "Assets/Idle MoCap/Sitting_Idle.anim";
    public const string GuitarPrefabPath = "Assets/_Project/Prefabs/Guitar.prefab";
    public const string OutClipPath = "Assets/_Project/Animations/Faceless/Faceless_GuitarPlaying.anim";

    // Guitar placement at the first frame of Sitting_Idle, in the character's root space.
    public static Vector3 GuitarPosition = new Vector3(0.16f, 1.10f, 0.13f);
    public static Vector3 NeckDirection = new Vector3(-0.92f, 0.36f, 0.16f);
    public static Vector3 SoundboardDirection = new Vector3(0f, 0.18f, 1f);
    public static float GuitarScale = 1.2f;

    // Hand (wrist) targets and finger directions in the guitar's own space
    // (+X towards the headstock, -Y towards the bass strings / player's head, +Z out of the soundboard).
    public static float[] ChordFretX = { 0.60f, 0.62f, 0.57f, 0.55f };
    public static Vector3 FretHandOffset = new Vector3(0f, 0.085f, -0.05f);
    public static Vector3 FretFingerDirection = new Vector3(0f, -0.75f, 0.66f);
    public static Vector3 StrumUp = new Vector3(-0.03f, -0.15f, 0.10f);
    public static Vector3 StrumDown = new Vector3(-0.03f, -0.05f, 0.10f);
    public static Vector3 StrumFingerDirection = new Vector3(0f, 0.85f, -0.5f);

    // Timing: one down-up strum per beat, one chord per bar of four beats.
    public static float BeatLength = 0.875f;
    public static float ClipLength = 14f;
    public static float ArmSampleRate = 15f;
    public static float BodySampleRate = 30f;

    // Finger muscles: thumb 1, thumb spread, thumb 2, thumb 3, then index/middle/ring/little (1, spread, 2, 3).
    public static float[] LeftFingers = { 0.3f, 0.4f, 0.1f, 0.1f, -0.2f, 0f, -0.6f, -0.5f, -0.3f, 0f, -0.6f, -0.5f, -0.35f, 0f, -0.6f, -0.5f, -0.4f, 0f, -0.6f, -0.5f };
    public static float[] RightFingers = { 0.1f, 0.2f, 0.1f, 0f, -0.55f, 0f, -0.7f, -0.5f, -0.5f, 0f, -0.6f, -0.5f, -0.5f, 0f, -0.6f, -0.5f, -0.5f, 0f, -0.6f, -0.5f };

    private const int LeftArmFirst = 37, RightArmFirst = 46, ArmMuscleCount = 9;
    private const int LeftFingerFirst = 55, RightFingerFirst = 75, FingerMuscleCount = 20;

    [MenuItem("Tools/Read of Mask/Bake Guitarist Animation")]
    private static void BakeFromMenu() => Debug.Log(Bake());

    /// <summary>Solves the pose at the given times and renders front and three-quarter views into outDir.</summary>
    public static string Preview(float[] times, string outDir)
    {
        using var rig = new Rig();
        string log = "";
        float[] previous = null;
        foreach (float t in times)
        {
            previous = rig.SolveAt(t, previous, out string info);
            log += info + "\n";
            Vector3 look = rig.Animator.GetBoneTransform(HumanBodyBones.Hips).position + Vector3.up * 0.3f;
            rig.Render(look, 0f, System.IO.Path.Combine(outDir, $"guitarist_front_{Mathf.RoundToInt(t * 1000f)}ms.png"));
            rig.Render(look, 55f, System.IO.Path.Combine(outDir, $"guitarist_34_{Mathf.RoundToInt(t * 1000f)}ms.png"));
        }
        return log;
    }

    /// <summary>Bakes the full loop into OutClipPath.</summary>
    public static string Bake()
    {
        using var rig = new Rig();
        var baseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(BaseClipPath);
        float baseLength = baseClip.length;

        // Arms: solved per sample, warm-started from the previous one.
        int armSamples = Mathf.RoundToInt(ClipLength * ArmSampleRate);
        var armCurves = new AnimationCurve[ArmMuscleCount * 2];
        for (int i = 0; i < armCurves.Length; i++) armCurves[i] = new AnimationCurve();
        float[] previous = null, first = null;
        float worstLeft = 0f, worstRight = 0f;
        for (int s = 0; s < armSamples; s++)
        {
            float t = s / ArmSampleRate;
            previous = rig.SolveAt(t, previous, out _);
            if (first == null) first = previous;
            worstLeft = Mathf.Max(worstLeft, rig.LastLeftError);
            worstRight = Mathf.Max(worstRight, rig.LastRightError);
            for (int i = 0; i < ArmMuscleCount; i++)
            {
                armCurves[i].AddKey(t, previous[LeftArmFirst + i]);
                armCurves[ArmMuscleCount + i].AddKey(t, previous[RightArmFirst + i]);
            }
        }
        // Close the loop on the first frame's pose.
        for (int i = 0; i < ArmMuscleCount; i++)
        {
            armCurves[i].AddKey(ClipLength, first[LeftArmFirst + i]);
            armCurves[ArmMuscleCount + i].AddKey(ClipLength, first[RightArmFirst + i]);
        }

        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(OutClipPath);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip { name = System.IO.Path.GetFileNameWithoutExtension(OutClipPath) };
        else clip.ClearCurves();

        var skipped = new HashSet<string>();
        for (int i = 0; i < ArmMuscleCount; i++)
        {
            skipped.Add(HumanTrait.MuscleName[LeftArmFirst + i]);
            skipped.Add(HumanTrait.MuscleName[RightArmFirst + i]);
        }
        for (int i = 0; i < FingerMuscleCount; i++)
        {
            skipped.Add(FingerCurveName(LeftFingerFirst + i));
            skipped.Add(FingerCurveName(RightFingerFirst + i));
        }

        // Body, legs, head, root: Sitting_Idle repeated to fill the loop.
        int bodySamples = Mathf.RoundToInt(ClipLength * BodySampleRate);
        foreach (var binding in AnimationUtility.GetCurveBindings(baseClip))
        {
            string name = binding.propertyName;
            // Hand IK goals belong to the old arm motion; fingers and arms are written below.
            if (skipped.Contains(name) || name.StartsWith("LeftHandT") || name.StartsWith("LeftHandQ") || name.StartsWith("RightHandT") || name.StartsWith("RightHandQ"))
                continue;
            var source = AnimationUtility.GetEditorCurve(baseClip, binding);
            var curve = new AnimationCurve();
            for (int s = 0; s <= bodySamples; s++)
            {
                float t = s / BodySampleRate;
                curve.AddKey(t, source.Evaluate(t % baseLength));
            }
            SetCurve(clip, name, curve);
        }

        for (int i = 0; i < ArmMuscleCount; i++)
        {
            SetCurve(clip, HumanTrait.MuscleName[LeftArmFirst + i], armCurves[i]);
            SetCurve(clip, HumanTrait.MuscleName[RightArmFirst + i], armCurves[ArmMuscleCount + i]);
        }
        for (int i = 0; i < FingerMuscleCount; i++)
        {
            SetCurve(clip, FingerCurveName(LeftFingerFirst + i), AnimationCurve.Constant(0f, ClipLength, LeftFingers[i]));
            SetCurve(clip, FingerCurveName(RightFingerFirst + i), AnimationCurve.Constant(0f, ClipLength, RightFingers[i]));
        }

        var settings = AnimationUtility.GetAnimationClipSettings(baseClip);
        settings.loopTime = true;
        settings.stopTime = ClipLength;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (isNew) AssetDatabase.CreateAsset(clip, OutClipPath);
        else EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        return $"Baked {OutClipPath}: {clip.length:F2}s, worst wrist error left {worstLeft * 100f:F1} cm, right {worstRight * 100f:F1} cm";
    }

    /// <summary>The guitar's pose relative to the Hips bone, for placing it in the prefab.</summary>
    public static void GetGuitarHipsPose(out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
    {
        using var rig = new Rig();
        localPosition = rig.Guitar.localPosition;
        localRotation = rig.Guitar.localRotation;
        localScale = rig.Guitar.localScale;
    }

    private static void SetCurve(AnimationClip clip, string property, AnimationCurve curve)
    {
        for (int k = 0; k < curve.length; k++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, k, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, k, AnimationUtility.TangentMode.ClampedAuto);
        }
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), property), curve);
    }

    // "Left Index 1 Stretched" -> "LeftHand.Index.1 Stretched", "Left Index Spread" -> "LeftHand.Index.Spread"
    // (the names humanoid clips use for finger curves).
    private static string FingerCurveName(int muscle)
    {
        string[] parts = HumanTrait.MuscleName[muscle].Split(' ');
        string prefix = parts[0] + "Hand." + parts[1] + ".";
        return parts[2] == "Spread" ? prefix + "Spread" : prefix + parts[2] + " " + parts[3];
    }

    private static float StrumAmount(float t)
    {
        // 0 = hand up, 1 = hand down; the down stroke is quicker than the way back up.
        float phase = (t % BeatLength) / BeatLength;
        return phase < 0.4f ? Mathf.SmoothStep(0f, 1f, phase / 0.4f) : 1f - Mathf.SmoothStep(0f, 1f, (phase - 0.4f) / 0.6f);
    }

    private static float FretX(float t)
    {
        float bar = BeatLength * 4f;
        int index = Mathf.FloorToInt(t / bar) % ChordFretX.Length;
        float into = t % bar;
        float previous = ChordFretX[(index + ChordFretX.Length - 1) % ChordFretX.Length];
        return Mathf.Lerp(previous, ChordFretX[index], Mathf.SmoothStep(0f, 1f, into / 0.2f));
    }

    /// <summary>A throwaway copy of the character and guitar in a preview scene.</summary>
    private sealed class Rig : IDisposable
    {
        public Animator Animator;
        public Transform Guitar;
        public float LastLeftError, LastRightError;

        private readonly Scene _scene;
        private PlayableGraph _graph;
        private readonly AnimationClipPlayable _playable;
        private readonly HumanPoseHandler _handler;
        private HumanPose _pose;

        public Rig()
        {
            _scene = EditorSceneManager.NewPreviewScene();
            var character = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath), _scene);
            PrefabUtility.UnpackPrefabInstance(character, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            character.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            foreach (var canvas in character.GetComponentsInChildren<Canvas>(true)) canvas.gameObject.SetActive(false);
            foreach (var t in character.GetComponentsInChildren<Transform>(true))
                if (t.name == "Controller_1 (3)") t.gameObject.SetActive(false); // the gamepad

            Animator = character.GetComponent<Animator>();
            _graph = PlayableGraph.Create("GuitaristPoseBaker");
            var output = AnimationPlayableOutput.Create(_graph, "Output", Animator);
            _playable = AnimationClipPlayable.Create(_graph, AssetDatabase.LoadAssetAtPath<AnimationClip>(BaseClipPath));
            output.SetSourcePlayable(_playable);
            _handler = new HumanPoseHandler(Animator.avatar, character.transform);
            _pose = new HumanPose();

            EvaluateBase(0f);
            var guitar = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(GuitarPrefabPath), _scene);
            Guitar = guitar.transform;
            Guitar.localScale = Vector3.one * GuitarScale;
            Vector3 x = NeckDirection.normalized;
            Vector3 z = Vector3.ProjectOnPlane(SoundboardDirection, x).normalized;
            Guitar.SetPositionAndRotation(GuitarPosition, Quaternion.LookRotation(z, Vector3.Cross(z, x)));
            Guitar.SetParent(Animator.GetBoneTransform(HumanBodyBones.Hips), true);
        }

        private void EvaluateBase(float t)
        {
            _playable.SetTime(t);
            _graph.Evaluate(0f);
        }

        public float[] SolveAt(float t, float[] warmStart, out string info)
        {
            EvaluateBase(t);
            _handler.GetHumanPose(ref _pose);
            float[] baseMuscles = (float[])_pose.muscles.Clone();
            if (warmStart != null)
                for (int i = LeftArmFirst; i < RightArmFirst + ArmMuscleCount; i++) _pose.muscles[i] = warmStart[i];
            for (int i = 0; i < FingerMuscleCount; i++)
            {
                _pose.muscles[LeftFingerFirst + i] = LeftFingers[i];
                _pose.muscles[RightFingerFirst + i] = RightFingers[i];
            }

            int rounds = warmStart == null ? 80 : 20;
            Vector3 leftTarget = Guitar.TransformPoint(new Vector3(FretX(t), 0f, 0f) + FretHandOffset);
            Vector3 rightTarget = Guitar.TransformPoint(Vector3.Lerp(StrumUp, StrumDown, StrumAmount(t)));
            Vector3 leftDir = Guitar.TransformDirection(FretFingerDirection).normalized;
            Vector3 rightDir = Guitar.TransformDirection(StrumFingerDirection).normalized;
            Solve(true, leftTarget, leftDir, baseMuscles, rounds);
            Solve(false, rightTarget, rightDir, baseMuscles, rounds);
            _handler.SetHumanPose(ref _pose);

            LastLeftError = (Animator.GetBoneTransform(HumanBodyBones.LeftHand).position - leftTarget).magnitude;
            LastRightError = (Animator.GetBoneTransform(HumanBodyBones.RightHand).position - rightTarget).magnitude;
            info = $"t={t:F2} left {LastLeftError * 100f:F1}cm dir {FingerDot(true, leftDir):F2}, right {LastRightError * 100f:F1}cm dir {FingerDot(false, rightDir):F2}";
            return (float[])_pose.muscles.Clone();
        }

        private float FingerDot(bool left, Vector3 dir)
        {
            var hand = Animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            var middle = Animator.GetBoneTransform(left ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);
            return Vector3.Dot((middle.position - hand.position).normalized, dir);
        }

        private float Cost(bool left, Vector3 target, Vector3 dir, float[] baseMuscles)
        {
            _handler.SetHumanPose(ref _pose);
            var hand = Animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            float cost = (hand.position - target).sqrMagnitude * 400f;
            cost += (1f - FingerDot(left, dir)) * 0.6f;
            int first = left ? LeftArmFirst : RightArmFirst;
            for (int i = first; i < first + ArmMuscleCount; i++)
                cost += 0.002f * (_pose.muscles[i] - baseMuscles[i]) * (_pose.muscles[i] - baseMuscles[i]);
            return cost;
        }

        // Coordinate descent over the arm's nine muscles, kept inside their natural [-1, 1] range.
        private void Solve(bool left, Vector3 target, Vector3 dir, float[] baseMuscles, int rounds)
        {
            int first = left ? LeftArmFirst : RightArmFirst;
            float step = rounds > 30 ? 0.3f : 0.08f;
            float best = Cost(left, target, dir, baseMuscles);
            for (int r = 0; r < rounds && step > 0.003f; r++)
            {
                bool improved = false;
                for (int i = first; i < first + ArmMuscleCount; i++)
                {
                    foreach (float delta in new[] { step, -step })
                    {
                        float old = _pose.muscles[i];
                        _pose.muscles[i] = Mathf.Clamp(old + delta, -1f, 1f);
                        float cost = Cost(left, target, dir, baseMuscles);
                        if (cost < best - 1e-6f) { best = cost; improved = true; break; }
                        _pose.muscles[i] = old;
                    }
                }
                if (!improved) step *= 0.5f;
            }
        }

        public void Render(Vector3 look, float yaw, string path)
        {
            var lightGo = new GameObject("Light");
            SceneManager.MoveGameObjectToScene(lightGo, _scene);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightGo.transform.rotation = Quaternion.Euler(35f, 160f, 0f);

            var camGo = new GameObject("Camera");
            SceneManager.MoveGameObjectToScene(camGo, _scene);
            var cam = camGo.AddComponent<Camera>();
            cam.scene = _scene;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.35f, 0.4f, 0.45f);
            cam.fieldOfView = 35f;
            cam.nearClipPlane = 0.05f;
            cam.transform.position = look + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * 2.6f + Vector3.up * 0.2f;
            cam.transform.LookAt(look);

            var rt = new RenderTexture(640, 640, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(640, 640, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 640, 640), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(lightGo);
        }

        public void Dispose()
        {
            if (_graph.IsValid()) _graph.Destroy();
            _handler?.Dispose();
            EditorSceneManager.ClosePreviewScene(_scene);
        }
    }
}
