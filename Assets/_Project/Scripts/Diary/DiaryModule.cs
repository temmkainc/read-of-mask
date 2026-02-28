using System;
using UnityEngine;
using Zenject;

public static class DiaryModule
{
    [Serializable]
    public struct ConfigData
    {

    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.Bind<Diary>().FromComponentInHierarchy().AsSingle();
    }
}
