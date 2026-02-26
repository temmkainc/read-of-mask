using System;
using UnityEngine;
using Zenject;

public static class CoreModule 
{
    [Serializable]
    public struct ConfigData
    {

    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.Bind<InputsManager>().FromNewComponentOnNewGameObject().AsSingle();
    }
}
