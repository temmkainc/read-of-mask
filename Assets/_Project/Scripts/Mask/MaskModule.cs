using System;
using UnityEngine;
using Zenject;

public static class MaskModule
{
    [Serializable]
    public struct ConfigData
    {

    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.BindInterfacesTo<MaskStateManager>().AsSingle();
    }
}
