using UnityEngine;

/// <summary>
/// State of the full-screen "moving between dreams" effect: a fast spin and swirl, a gaussian
/// blur and a white-out. Whoever plays the transition sets these values every frame;
/// DreamTransitionFeature draws them, and does nothing while the effect is off.
/// </summary>
public static class DreamTransition
{
    private static readonly int AmountId = Shader.PropertyToID("_DreamAmount");
    private static readonly int WhiteId = Shader.PropertyToID("_DreamWhite");
    private static readonly int SpinId = Shader.PropertyToID("_DreamSpin");

    /// <summary>True while any part of the effect is visible.</summary>
    public static bool IsActive { get; private set; }

    /// <summary>Current white-out, 0..1.</summary>
    public static float White { get; private set; }

    /// <param name="amount">Blur, swirl, zoom and colour shift (0 = none, 1 = full).</param>
    /// <param name="white">White-out (0 = none, 1 = the screen is fully white).</param>
    /// <param name="spin">Rotation of the whole picture, in radians.</param>
    public static void Set(float amount, float white, float spin)
    {
        IsActive = amount > 0.0001f || white > 0.0001f || Mathf.Abs(Mathf.DeltaAngle(0f, spin * Mathf.Rad2Deg)) > 0.01f;
        White = white;
        Shader.SetGlobalFloat(AmountId, amount);
        Shader.SetGlobalFloat(WhiteId, white);
        Shader.SetGlobalFloat(SpinId, spin);
    }

    public static void Clear() => Set(0f, 0f, 0f);

    // domain reload is off, so don't let a transition cut short by leaving Play mode carry over
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsActive = false;
        White = 0f;
    }
}
