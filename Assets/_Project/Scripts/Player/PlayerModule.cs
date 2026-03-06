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
        [field: SerializeField] public PlayerStateData InteractionState { get; private set; }
        [field: SerializeField] public PlayerLookTarget.Config LookTargetConfig { get; private set; }

    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.Bind<IPlayerStateManager>().To<PlayerStateManager>().AsSingle()
            .WithArguments(config).NonLazy();

        container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
        container.Bind<PlayerGrabbing>().FromComponentInHierarchy().AsSingle();
        container.Bind<PlayerLookTarget>().AsSingle().WithArguments(config.LookTargetConfig).NonLazy();
        container.BindInterfacesAndSelfTo<HighlightManager>().AsSingle();
    }
}
