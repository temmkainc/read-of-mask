using System;
using TMPro;
using UnityEngine;
using Zenject;
using BookCurlPro;

public static class DiaryModule
{
    [Serializable]
    public struct ConfigData
    {
        [field: SerializeField] public TMP_FontAsset LockedLettersFontAsset {  get; private set; }
        [field: SerializeField] public BookPro _book { get; private set; }
    }

    public static void Install(DiContainer container, ConfigData config)
    {
        container.Bind<Diary>().FromComponentInHierarchy().AsSingle();
        container.Bind<BookPro>().FromInstance(config._book).AsSingle();
        container.Bind<UnlockedLettersManager>().AsSingle().WithArguments(config.LockedLettersFontAsset);
    }
}
