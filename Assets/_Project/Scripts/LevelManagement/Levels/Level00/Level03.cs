using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level03 : LevelBase
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
        Debug.Log("Level 03 deactivated!");
    }
}
