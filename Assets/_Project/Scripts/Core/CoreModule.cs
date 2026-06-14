using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Rendering;
public static class CoreModule 
{
    [Serializable]
    public struct ConfigData
    {
        [field: SerializeField] public List<LevelBase> Levels { get; private set; }
        [field: SerializeField] public CameraEffects CameraEffects { get; private set; }
        [field: SerializeField] public bool WithLevelsScenario { get; private set; } 
        [field: SerializeField] public EffectsContainer EffectsContainer { get; private set; }
        [field: SerializeField] public LevelVariables LevelVariables { get; private set; }
        [field: SerializeField] public AudioManager AudioManager { get; private set; }
    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.BindInterfacesAndSelfTo<GameOptions>().AsSingle();
        container.BindInterfacesAndSelfTo<Volume>().FromComponentInHierarchy().AsSingle().WhenInjectedInto<GameOptions>().NonLazy();
        container.Bind<InputManager>().AsSingle();
        container.Bind<OverlayHintsSceneContainer>().FromComponentInHierarchy().AsSingle();
        container.Bind<LevelVariables>().FromInstance(config.LevelVariables).AsSingle();
        container.Bind<CinemachineManager>().AsSingle();
        container.Bind<InteractionCinemachineCamera>().FromComponentInHierarchy().AsSingle();
        container.BindInterfacesTo<CommandBus>().AsSingle();
        container.BindInterfacesTo<CommandFactory>().AsSingle();
        container.BindInterfacesTo<LevelManager>().AsSingle().WithArguments(config.Levels, config.WithLevelsScenario);
        container.Bind<LockableActionsManager>().AsSingle();
        container.BindInstance(config.CameraEffects).AsSingle();
        container.BindInstance(config.EffectsContainer).AsSingle();
        container.BindInstance(config.AudioManager).AsSingle();
    }
}
