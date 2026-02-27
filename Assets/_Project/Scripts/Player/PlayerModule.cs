using System;
using UnityEngine;
using Zenject;

public static class PlayerModule
{
    [Serializable]
    public struct ConfigData
    {
        [field: SerializeField] public PlayerStateData GeneralState { get; private set;  }
        [field: SerializeField] public PlayerStateData DiaryState { get; private set; }
    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.Bind<IPlayerStateManager>().To<PlayerStateManager>().AsSingle()
            .WithArguments(config).NonLazy();
    }
}
