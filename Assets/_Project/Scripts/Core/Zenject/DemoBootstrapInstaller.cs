using UnityEngine;
using Zenject;

public class DemoBootstrapInstaller : MonoInstaller
{
    [SerializeField] private PlayerModule.ConfigData _playerModuleConfigData;

    public override void InstallBindings()
    {
        Container.Bind<IPlayerStateManager>().To<PlayerStateManager>().AsSingle()
            .WithArguments(_playerModuleConfigData).NonLazy();

        Container.Bind<CinemachineManager>().AsSingle();
        Container.Bind<InteractionCinemachineCamera>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesTo<CommandBus>().AsSingle();
        Container.BindInterfacesTo<CommandFactory>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameOptions>().AsSingle();
        Container.Bind<InputManager>().AsSingle();
    }
}