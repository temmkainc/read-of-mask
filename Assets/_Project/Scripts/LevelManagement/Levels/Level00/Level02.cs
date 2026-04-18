using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level02 : LevelBase
{
    public override void Begin()
    {
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        // AudioManager.Instance.PlayMusic(MusicTracks.TuftaMusic);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 01 deactivated!");
    }
}
