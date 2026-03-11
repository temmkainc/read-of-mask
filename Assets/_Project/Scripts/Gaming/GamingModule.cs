using System;
using UnityEngine;
using Zenject;

public static class GamingModule
{
    [Serializable]
    public struct ConfigData
    {
        public PongMinigame PongMinigame;
    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.Bind<MinigameManager>().AsSingle().WithArguments(config);
    }
}
