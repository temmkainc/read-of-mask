using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level01 : LevelBase
{
    [SerializeField] private AudioSource _headmistressAudioSource;
    public override void Begin()
    {
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        AudioManager.Instance.PlayMusic(MusicTracks.Level01Music);
        await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressTransitionToTheOffice, _headmistressAudioSource);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 01 deactivated!");
    }
}
