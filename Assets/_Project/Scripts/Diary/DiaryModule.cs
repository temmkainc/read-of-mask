using System;
using TMPro;
using UnityEngine;
using Zenject;

public static class DiaryModule
{
    [Serializable]
    public struct ConfigData
    {
        [field: SerializeField] public TMP_FontAsset LockedLettersFontAsset {  get; private set; }
    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.Bind<Diary>().FromComponentInHierarchy().AsSingle();
        container.Bind<UnlockedLettersManager>().AsSingle().WithArguments(config.LockedLettersFontAsset);
    }
}
