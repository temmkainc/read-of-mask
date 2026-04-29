using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level04 : LevelBase
{
    [SerializeField] private AudioSource _fridgeBuzzSource;
    public override void Begin()
    {   
        AudioManager.Instance.PlayMusic(MusicTracks.Level04Music);
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {

    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 03 deactivated!");
    }
}
