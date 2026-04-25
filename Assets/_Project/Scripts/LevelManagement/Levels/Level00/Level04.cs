using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level04 : LevelBase
{
    public override void Begin()
    {
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        // AudioManager.Instance.PlayMusic(MusicTracks.TuftaMusic);
        await UniTask.Delay(2000);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 03 deactivated!");
    }
}
