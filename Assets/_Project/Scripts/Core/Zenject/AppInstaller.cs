using UnityEngine;
using Zenject;

public class AppInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle();
    }
}