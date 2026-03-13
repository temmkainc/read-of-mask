using System;
using UnityEngine;
using Zenject;

public static class GamingModule
{
    [Serializable]
    public struct ConfigData
    {
        public PongMinigame PongMinigame;
        public BreakoutMinigame BreakoutMinigame;
    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.BindInterfacesAndSelfTo<MinigameManager>().AsSingle().WithArguments(config);
    }
}
