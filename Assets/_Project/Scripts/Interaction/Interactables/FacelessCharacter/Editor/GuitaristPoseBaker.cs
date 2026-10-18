using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    public static Vector3 GuitarPosition = new Vector3(0.16f, 1.10f, 0.21f);
    public static Vector3 NeckDirection = new Vector3(-0.92f, 0.36f, -0.05f);
    public static Vector3 SoundboardDirection = new Vector3(0f, 0.18f, 1f);
    public static float GuitarScale = 1.2f;
    // How much of the upper body's sway the guitar rides along with (0 = fixed to the hips, 1 = fixed
    // to the chest), and how far it rocks with each strum.
    public static float GuitarFollowsChest = 0.7f;
    public static float StrumRockDegrees = 1.5f;
    // How much of Sitting_Idle's spine/chest sway to keep (it's a "looking around" idle that twists the
    // torso further than a seated player would, pulling the fretting hand off the neck). The head and
    // neck keep their full motion.
    public static float TorsoMotion = 0.7f;
    // Gap (guitar units) kept between the body and the guitar's back / lower edge.
    public static float BodyClearance = 0.025f;

    // Hand (wrist) targets and finger directions in the guitar's own space
    // (+X towards the headstock, -Y towards the bass strings / player's head, +Z out of the soundboard).
    public static float[] ChordFretX = { 0.52f, 0.54f, 0.50f, 0.48f };
    // Fretting hand: upright under the neck with the palm towards the player, fingers rising past the
    // treble edge and curling back onto the fretboard; the thumb is tucked so it never pokes through the neck.
    public static Vector3 FretHandOffset = new Vector3(0f, 0.12f, 0f);
    public static Vector3 FretFingerDirection = new Vector3(0f, -1f, 0.15f);
    public static Vector3 FretPalmDirection = new Vector3(0f, 0.1f, -1f);
    // Where the middle finger's last joint and the thumb tip should end up.
    public static Vector3 FretFingertipOffset = new Vector3(0f, -0.005f, 0.03f);
    public static Vector3 FretThumbOffset = new Vector3(0.02f, -0.02f, -0.05f);
    // How strongly the solver chases each fretting target (wrist, fingertip, thumb).
    public static float FretWristWeight = 300f, FretFingertipWeight = 600f, FretThumbWeight = 0f;
    // Solver passes per frame (warm-started from the previous frame); more keeps up with faster body motion.
    public static int SolverRoundsPerFrame = 45;
    // Strumming hand: hanging over the strings, fingers pointing down, palm towards the soundboard.
    public static Vector3 StrumUp = new Vector3(-0.03f, -0.15f, 0.10f);
    public static Vector3 StrumDown = new Vector3(-0.03f, -0.05f, 0.10f);
    public static Vector3 StrumFingerDirection = new Vector3(0f, 0.85f, -0.5f);
    public static Vector3 StrumPalmDirection = new Vector3(0f, -0.5f, -0.85f);

    // Timing: one down-up strum per beat, one chord per bar of four beats.
    public static float BeatLength = 0.875f;
    public static float ClipLength = 14f;
    public static float ArmSampleRate = 15f;
    public static float BodySampleRate = 30f;

    // Finger muscles: thumb 1, thumb spread, thumb 2, thumb 3, then index/middle/ring/little (1, spread, 2, 3).
    // Negative "Stretched" values curl a finger towards the palm.
    // Fretting: fingers arched over the fretboard, thumb curled in.
    public static float[] LeftFingers = { -0.3f, -0.6f, -0.4f, -0.3f, -0.3f, 0f, -0.8f, -0.4f, -0.35f, 0f, -0.8f, -0.4f, -0.4f, 0f, -0.8f, -0.4f, -0.45f, 0f, -0.8f, -0.4f };
    // Loose fist with the thumb resting on the curled index, as if holding a pick.
    public static float[] RightFingers = { -0.2f, -0.3f, -0.3f, -0.2f, -0.6f, 0f, -0.8f, -0.6f, -0.55f, 0f, -0.7f, -0.6f, -0.55f, 0f, -0.7f, -0.6f, -0.55f, 0f, -0.7f, -0.6f };

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
        // The guitar moves with the body too (see Rig.PlaceGuitar): local position xyz + rotation xyzw.
        var guitarCurves = new AnimationCurve[7];
        for (int i = 0; i < guitarCurves.Length; i++) guitarCurves[i] = new AnimationCurve();
        float[] previous = null, first = null;
        Vector3 firstGuitarPos = Vector3.zero, firstGuitarWorld = Vector3.zero;
        Quaternion firstGuitarRot = Quaternion.identity, lastGuitarRot = Quaternion.identity;
        float worstLeft = 0f, worstRight = 0f, guitarTravel = 0f;
        // Run through the loop once without recording, so the recorded pass starts warm from where the
        // loop ends - otherwise the cold first frame can settle on a different arm pose and the hands
        // jump at the loop seam.
        for (int s = 0; s < armSamples; s++)
            previous = rig.SolveAt(s / ArmSampleRate, previous, out _);
        for (int s = 0; s < armSamples; s++)
        {
            float t = s / ArmSampleRate;
            previous = rig.SolveAt(t, previous, out _);
            worstLeft = Mathf.Max(worstLeft, rig.LastLeftError);
            worstRight = Mathf.Max(worstRight, rig.LastRightError);
            for (int i = 0; i < ArmMuscleCount; i++)
            {
                armCurves[i].AddKey(t, previous[LeftArmFirst + i]);
                armCurves[ArmMuscleCount + i].AddKey(t, previous[RightArmFirst + i]);
            }

            Vector3 guitarPos = rig.Guitar.localPosition;
            Quaternion guitarRot = rig.Guitar.localRotation;
            if (first == null)
            {
                first = previous;
                firstGuitarPos = guitarPos;
                firstGuitarWorld = rig.Guitar.position;
                firstGuitarRot = lastGuitarRot = guitarRot;
            }
            // q and -q are the same rotation; keep the sign consistent so the curves don't flip.
            if (Quaternion.Dot(guitarRot, lastGuitarRot) < 0f) guitarRot = Negate(guitarRot);
            lastGuitarRot = guitarRot;
            guitarTravel = Mathf.Max(guitarTravel, (rig.Guitar.position - firstGuitarWorld).magnitude);
            AddGuitarKeys(guitarCurves, t, guitarPos, guitarRot);
        }
        // Close the loop on the first frame's pose.
        for (int i = 0; i < ArmMuscleCount; i++)
        {
            armCurves[i].AddKey(ClipLength, first[LeftArmFirst + i]);
            armCurves[ArmMuscleCount + i].AddKey(ClipLength, first[RightArmFirst + i]);
        }
        AddGuitarKeys(guitarCurves, ClipLength, firstGuitarPos,
            Quaternion.Dot(firstGuitarRot, lastGuitarRot) < 0f ? Negate(firstGuitarRot) : firstGuitarRot);

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
            bool torso = IsTorsoMuscle(name);
            float mean = torso ? CurveMean(source, baseLength) : 0f;
            var curve = new AnimationCurve();
            for (int s = 0; s <= bodySamples; s++)
            {
                float t = s / BodySampleRate;
                float v = source.Evaluate(t % baseLength);
                curve.AddKey(t, torso ? mean + (v - mean) * TorsoMotion : v);
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
        string guitarPath = AnimationUtility.CalculateTransformPath(rig.Guitar, rig.Animator.transform);
        string[] guitarProps = { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z", "m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w" };
        for (int i = 0; i < guitarProps.Length; i++)
            SetCurve(clip, EditorCurveBinding.FloatCurve(guitarPath, typeof(Transform), guitarProps[i]), guitarCurves[i]);

        var settings = AnimationUtility.GetAnimationClipSettings(baseClip);
        settings.loopTime = true;
        settings.stopTime = ClipLength;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (isNew) AssetDatabase.CreateAsset(clip, OutClipPath);
        else EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        return $"Baked {OutClipPath}: {clip.length:F2}s, worst wrist error left {worstLeft * 100f:F1} cm, right {worstRight * 100f:F1} cm, guitar sways up to {guitarTravel * 100f:F1} cm";
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
        => SetCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), property), curve);

    private static void SetCurve(AnimationClip clip, EditorCurveBinding binding, AnimationCurve curve)
    {
        for (int k = 0; k < curve.length; k++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, k, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, k, AnimationUtility.TangentMode.ClampedAuto);
        }
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static void AddGuitarKeys(AnimationCurve[] curves, float t, Vector3 position, Quaternion rotation)
    {
        curves[0].AddKey(t, position.x);
        curves[1].AddKey(t, position.y);
        curves[2].AddKey(t, position.z);
        curves[3].AddKey(t, rotation.x);
        curves[4].AddKey(t, rotation.y);
        curves[5].AddKey(t, rotation.z);
        curves[6].AddKey(t, rotation.w);
    }

    private static Quaternion Negate(Quaternion q) => new Quaternion(-q.x, -q.y, -q.z, -q.w);

    // Guitar.obj's body outline: half its width at a point along the neck axis (two bouts, see the mesh).
    private const float GuitarBodyMinX = -0.31f, GuitarBodyMaxX = 0.265f, GuitarBodyBackZ = -0.095f;
    private static float GuitarBodyHalfWidth(float x)
    {
        const float lowerCentre = -0.12f, lowerRadius = 0.19f, upperCentre = 0.13f, upperRadius = 0.135f;
        float lower = Mathf.Abs(x - lowerCentre) < lowerRadius ? Mathf.Sqrt(lowerRadius * lowerRadius - (x - lowerCentre) * (x - lowerCentre)) : 0f;
        float upper = Mathf.Abs(x - upperCentre) < upperRadius ? Mathf.Sqrt(upperRadius * upperRadius - (x - upperCentre) * (x - upperCentre)) : 0f;
        return Mathf.Max(lower, upper);
    }

    // Spine, Chest and UpperChest muscles (the first nine humanoid muscles).
    private const int TorsoMuscleCount = 9;
    private static bool IsTorsoMuscle(string name)
    {
        for (int i = 0; i < TorsoMuscleCount; i++)
            if (HumanTrait.MuscleName[i] == name) return true;
        return false;
    }

    private static float CurveMean(AnimationCurve curve, float length)
    {
        const int samples = 200;
        float sum = 0f;
        for (int i = 0; i < samples; i++) sum += curve.Evaluate(length * i / samples);
        return sum / samples;
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
        private readonly GameObject _character;
        private readonly AnimationClip _baseClip;
        private readonly HumanPoseHandler _handler;
        private HumanPose _pose;
        private Vector3 _fretTip, _fretThumb;
        private Vector3 _guitarRestLocalPos, _guitarPos0, _chestPos0;
        private Quaternion _guitarRestLocalRot, _guitarRot0, _chestRot0;
        private readonly float[] _torsoMean = new float[TorsoMuscleCount];
        private readonly SkinnedMeshRenderer _body;
        private readonly bool[] _vertexOnTorso, _vertexOnThigh;
        private readonly UnityEngine.Mesh _bakedBody = new UnityEngine.Mesh();

        private Transform Chest
        {
            get
            {
                var upper = Animator.GetBoneTransform(HumanBodyBones.UpperChest);
                return upper != null ? upper : Animator.GetBoneTransform(HumanBodyBones.Chest);
            }
        }

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
            _character = character;
            _baseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(BaseClipPath);
            _handler = new HumanPoseHandler(Animator.avatar, character.transform);
            _pose = new HumanPose();

            // Which body vertices belong to the torso and the thighs, by their strongest bone.
            foreach (var renderer in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (_body == null || renderer.sharedMesh.vertexCount > _body.sharedMesh.vertexCount) _body = renderer;
            var torsoBones = new[] { HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest };
            var thighBones = new[] { HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg };
            var boneWeights = _body.sharedMesh.boneWeights;
            _vertexOnTorso = new bool[boneWeights.Length];
            _vertexOnThigh = new bool[boneWeights.Length];
            for (int i = 0; i < boneWeights.Length; i++)
            {
                var bone = _body.bones[boneWeights[i].boneIndex0];
                foreach (var b in torsoBones) if (bone == Animator.GetBoneTransform(b)) _vertexOnTorso[i] = true;
                foreach (var b in thighBones) if (bone == Animator.GetBoneTransform(b)) _vertexOnThigh[i] = true;
            }

            var baseClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(BaseClipPath);
            foreach (var binding in AnimationUtility.GetCurveBindings(baseClip))
                for (int i = 0; i < TorsoMuscleCount; i++)
                    if (binding.propertyName == HumanTrait.MuscleName[i])
                        _torsoMean[i] = CurveMean(AnimationUtility.GetEditorCurve(baseClip, binding), baseClip.length);

            EvaluateBase(0f);
            var guitar = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(GuitarPrefabPath), _scene);
            Guitar = guitar.transform;
            Guitar.localScale = Vector3.one * GuitarScale;
            Vector3 x = NeckDirection.normalized;
            Vector3 z = Vector3.ProjectOnPlane(SoundboardDirection, x).normalized;
            Guitar.SetPositionAndRotation(GuitarPosition, Quaternion.LookRotation(z, Vector3.Cross(z, x)));
            Guitar.SetParent(Animator.GetBoneTransform(HumanBodyBones.Hips), true);
            _guitarRestLocalPos = Guitar.localPosition;
            _guitarRestLocalRot = Guitar.localRotation;

            // Chest and guitar at the first frame: PlaceGuitar measures the body's sway from here.
            _handler.GetHumanPose(ref _pose);
            DampTorso();
            _handler.SetHumanPose(ref _pose);
            _chestPos0 = Chest.position;
            _chestRot0 = Chest.rotation;
            _guitarPos0 = Guitar.position;
            _guitarRot0 = Guitar.rotation;
        }

        // The guitar rests on the thigh but rides along with most of the upper body's sway and rocks a
        // little with each strum, so it never looks bolted in place while the body moves around it.
        private void PlaceGuitar(float t)
        {
            var hips = Animator.GetBoneTransform(HumanBodyBones.Hips);
            Vector3 restPos = hips.TransformPoint(_guitarRestLocalPos);
            Quaternion restRot = hips.rotation * _guitarRestLocalRot;

            Quaternion chestDelta = Chest.rotation * Quaternion.Inverse(_chestRot0);
            Vector3 followPos = Chest.position + chestDelta * (_guitarPos0 - _chestPos0);
            Quaternion followRot = chestDelta * _guitarRot0;

            Vector3 position = Vector3.Lerp(restPos, followPos, GuitarFollowsChest);
            Quaternion rotation = Quaternion.Slerp(restRot, followRot, GuitarFollowsChest);
            float rock = (StrumAmount(t) - 0.5f) * 2f * StrumRockDegrees; // rolls about the neck with the strum
            Guitar.SetPositionAndRotation(position, rotation * Quaternion.AngleAxis(rock, Vector3.right));
            KeepGuitarOutOfBody();
        }

        // Pushes the guitar forward until the belly/chest no longer sink into its back, and up until it
        // rests on the thigh instead of sinking into it.
        private void KeepGuitarOutOfBody()
        {
            for (int pass = 0; pass < 3; pass++)
            {
                _body.BakeMesh(_bakedBody, true);
                var vertices = _bakedBody.vertices;
                Matrix4x4 toGuitar = Guitar.worldToLocalMatrix * _body.transform.localToWorldMatrix;
                float pushForward = 0f, pushUp = 0f;
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (!_vertexOnTorso[i] && !_vertexOnThigh[i]) continue;
                    Vector3 p = toGuitar.MultiplyPoint3x4(vertices[i]);
                    if (p.x < GuitarBodyMinX || p.x > GuitarBodyMaxX) continue;
                    float halfWidth = GuitarBodyHalfWidth(p.x);
                    if (Mathf.Abs(p.y) > halfWidth + BodyClearance) continue;
                    if (p.z < GuitarBodyBackZ - BodyClearance || p.z > 0.03f) continue;
                    if (_vertexOnTorso[i])
                        pushForward = Mathf.Max(pushForward, p.z - GuitarBodyBackZ + BodyClearance);
                    else if (p.y > 0f) // thigh under the lower edge (+Y points towards the floor)
                        pushUp = Mathf.Max(pushUp, Mathf.Min(0.05f, halfWidth + BodyClearance - p.y));
                }
                if (pushForward <= 0.0005f && pushUp <= 0.0005f) break;
                Guitar.position += Guitar.TransformVector(new Vector3(0f, -pushUp, pushForward));
            }
        }

        // Same torso damping the baked clip gets (see TorsoMotion).
        private void DampTorso()
        {
            for (int i = 0; i < TorsoMuscleCount; i++)
                _pose.muscles[i] = _torsoMean[i] + (_pose.muscles[i] - _torsoMean[i]) * TorsoMotion;
        }

        private void EvaluateBase(float t)
        {
            // SampleAnimation writes the pose straight away; a PlayableGraph's writes lag a frame, which
            // left the solver working against the previous frame's body.
            _baseClip.SampleAnimation(_character, t);
            _character.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public float[] SolveAt(float t, float[] warmStart, out string info)
        {
            EvaluateBase(t);
            _handler.GetHumanPose(ref _pose);
            DampTorso();
            float[] baseMuscles = (float[])_pose.muscles.Clone();
            if (warmStart != null)
                for (int i = LeftArmFirst; i < RightArmFirst + ArmMuscleCount; i++) _pose.muscles[i] = warmStart[i];
            for (int i = 0; i < FingerMuscleCount; i++)
            {
                _pose.muscles[LeftFingerFirst + i] = LeftFingers[i];
                _pose.muscles[RightFingerFirst + i] = RightFingers[i];
            }
            _handler.SetHumanPose(ref _pose);
            PlaceGuitar(t);

            int rounds = warmStart == null ? 80 : SolverRoundsPerFrame;
            Vector3 leftTarget = Guitar.TransformPoint(new Vector3(FretX(t), 0f, 0f) + FretHandOffset);
            Vector3 rightTarget = Guitar.TransformPoint(Vector3.Lerp(StrumUp, StrumDown, StrumAmount(t)));
            Vector3 leftDir = Guitar.TransformDirection(FretFingerDirection).normalized;
            Vector3 rightDir = Guitar.TransformDirection(StrumFingerDirection).normalized;
            Vector3 leftPalm = Guitar.TransformDirection(FretPalmDirection).normalized;
            Vector3 rightPalm = Guitar.TransformDirection(StrumPalmDirection).normalized;
            _fretTip = Guitar.TransformPoint(new Vector3(FretX(t), 0f, 0f) + FretFingertipOffset);
            _fretThumb = Guitar.TransformPoint(new Vector3(FretX(t), 0f, 0f) + FretThumbOffset);
            Solve(true, leftTarget, leftDir, leftPalm, baseMuscles, rounds);
            Solve(false, rightTarget, rightDir, rightPalm, baseMuscles, rounds);
            _handler.SetHumanPose(ref _pose);

            LastLeftError = (Animator.GetBoneTransform(HumanBodyBones.LeftHand).position - leftTarget).magnitude;
            LastRightError = (Animator.GetBoneTransform(HumanBodyBones.RightHand).position - rightTarget).magnitude;
            info = $"t={t:F2} left {LastLeftError * 100f:F1}cm fingers {FingerDot(true, leftDir):F2} palm {Vector3.Dot(PalmNormal(true), leftPalm):F2}, " +
                   $"right {LastRightError * 100f:F1}cm fingers {FingerDot(false, rightDir):F2} palm {Vector3.Dot(PalmNormal(false), rightPalm):F2}";
            return (float[])_pose.muscles.Clone();
        }

        private float FingerDot(bool left, Vector3 dir)
        {
            var hand = Animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            var middle = Animator.GetBoneTransform(left ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);
            return Vector3.Dot((middle.position - hand.position).normalized, dir);
        }

        private static readonly HumanBodyBones[] NeckCheckedBones =
        {
            HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal,
            HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftMiddleDistal, HumanBodyBones.LeftRingDistal, HumanBodyBones.LeftLittleDistal,
        };

        // How deep (guitar units) a point is inside the neck, padded by roughly a finger's thickness; 0 when outside.
        private float NeckPenetration(Vector3 world)
        {
            Vector3 p = Guitar.InverseTransformPoint(world);
            if (p.x < 0.11f || p.x > 0.72f) return 0f;
            float depth = Mathf.Min(0.035f - Mathf.Abs(p.y), Mathf.Min(p.z + 0.035f, 0.025f - p.z));
            return Mathf.Max(0f, depth);
        }

        // Direction the palm faces (in the T-pose both palms face down).
        private Vector3 PalmNormal(bool left)
        {
            var hand = Animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand).position;
            var index = Animator.GetBoneTransform(left ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal).position;
            var little = Animator.GetBoneTransform(left ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal).position;
            Vector3 n = Vector3.Cross(index - hand, little - hand).normalized;
            return left ? n : -n;
        }

        private float Cost(bool left, Vector3 target, Vector3 dir, Vector3 palm, float[] baseMuscles)
        {
            _handler.SetHumanPose(ref _pose);
            var hand = Animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            float cost;
            if (left)
            {
                // The fretting hand is placed by where the fingers land; the wrist target is only a guide.
                cost = (hand.position - target).sqrMagnitude * FretWristWeight;
                cost += (Animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal).position - _fretTip).sqrMagnitude * FretFingertipWeight;
                cost += (Animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal).position - _fretThumb).sqrMagnitude * FretThumbWeight;
                cost += (1f - FingerDot(true, dir)) * 0.2f;
                cost += (1f - Vector3.Dot(PalmNormal(true), palm)) * 0.3f;
                // Nothing may sink into the neck.
                foreach (var bone in NeckCheckedBones)
                {
                    float depth = NeckPenetration(Animator.GetBoneTransform(bone).position);
                    cost += depth * depth * 20000f;
                }
            }
            else
            {
                cost = (hand.position - target).sqrMagnitude * 400f;
                cost += (1f - FingerDot(false, dir)) * 0.6f;
                cost += (1f - Vector3.Dot(PalmNormal(false), palm)) * 0.6f;
            }
            int first = left ? LeftArmFirst : RightArmFirst;
            for (int i = first; i < first + ArmMuscleCount; i++)
                cost += 0.002f * (_pose.muscles[i] - baseMuscles[i]) * (_pose.muscles[i] - baseMuscles[i]);
            return cost;
        }

        // Coordinate descent over the arm's nine muscles, kept inside their natural [-1, 1] range.
        private void Solve(bool left, Vector3 target, Vector3 dir, Vector3 palm, float[] baseMuscles, int rounds)
        {
            int first = left ? LeftArmFirst : RightArmFirst;
            float step = rounds > 30 ? 0.3f : 0.08f;
            float best = Cost(left, target, dir, palm, baseMuscles);
            for (int r = 0; r < rounds && step > 0.003f; r++)
            {
                bool improved = false;
                for (int i = first; i < first + ArmMuscleCount; i++)
                {
                    foreach (float delta in new[] { step, -step })
                    {
                        float old = _pose.muscles[i];
                        _pose.muscles[i] = Mathf.Clamp(old + delta, -1f, 1f);
                        float cost = Cost(left, target, dir, palm, baseMuscles);
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
            _handler?.Dispose();
            EditorSceneManager.ClosePreviewScene(_scene);
        }
    }
}
