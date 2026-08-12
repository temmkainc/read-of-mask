using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level03 : LevelBase
{
    public override void Begin()
    {
        AudioManager.Instance.PlayMusic(MusicTracks.Level02Music, fadeDuration: 1f);
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        await UniTask.Delay(2000);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 03 deactivated!");
    }
}
