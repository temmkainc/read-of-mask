using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public static class CoreModule 
{
    [Serializable]
    public struct ConfigData
    {
        [field: SerializeField] public List<LevelBase> Levels { get; private set; }
        [field: SerializeField] public CameraEffects CameraEffects { get; private set; }
        [field: SerializeField] public bool WithLevelsScenario { get; private set; } 
        [field: SerializeField] public EffectsContainer EffectsContainer { get; private set; }
    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.BindInterfacesAndSelfTo<GameOptions>().AsSingle();

        container.Bind<InputManager>().AsSingle();
        container.Bind<CinemachineManager>().AsSingle();
        container.Bind<InteractionCinemachineCamera>().FromComponentInHierarchy().AsSingle();
        container.BindInterfacesTo<CommandBus>().AsSingle();
        container.BindInterfacesTo<CommandFactory>().AsSingle();
        container.BindInterfacesTo<LevelManager>().AsSingle().WithArguments(config.Levels, config.WithLevelsScenario);
        container.BindInstance(config.CameraEffects).AsSingle();
        container.BindInstance(config.EffectsContainer).AsSingle();
    }
}
