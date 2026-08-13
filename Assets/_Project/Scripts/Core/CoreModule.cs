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
        [field: SerializeField]
        [field: Tooltip("Scene to load once the last level in this scene's Levels list is completed. Drag the actual scene asset, not just its name, so renaming the scene later doesn't break this. Leave empty if this is the final scene in the game.")]
        public SceneField NextSceneName { get; private set; }
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
        container.BindInterfacesAndSelfTo<SaveService>().AsSingle();
        container.BindInterfacesAndSelfTo<LevelManager>().AsSingle().WithArguments(config.Levels, config.WithLevelsScenario, (string)config.NextSceneName);
        container.BindInterfacesAndSelfTo<LockableActionsManager>().AsSingle();
        container.BindInstance(config.CameraEffects).AsSingle();
        container.BindInstance(config.EffectsContainer).AsSingle();
        container.BindInstance(config.AudioManager).AsSingle();
    }
}
