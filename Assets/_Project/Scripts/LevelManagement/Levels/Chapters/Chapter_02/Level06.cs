using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level06 : LevelBase
{
    public override void Begin()
    {
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        Debug.Log("Level 06 activated!");
        await UniTask.Delay(700);
        AudioManager.Instance.PlayMusic(MusicTracks.Level05Music, fadeDuration: 0.5f);
        await UniTask.Delay(2000);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 06 deactivated!");
    }
}
