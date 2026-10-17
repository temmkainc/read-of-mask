using UnityEngine;
using Zenject;

/// <summary>
/// Enter Play Mode runs without a domain reload, so Zenject's static state survives between
/// play sessions. Clears the parts that hold containers or pending scene bindings; type
/// reflection caches are left alone (they stay valid and make the next session faster).
/// </summary>
public static class ZenjectStaticReset
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        StaticContext.Clear();
        SceneContext.ParentContainers = null;
        SceneContext.ExtraBindingsInstallMethod = null;
        SceneContext.ExtraBindingsLateInstallMethod = null;
    }
}
