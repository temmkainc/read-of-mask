using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level02 : LevelBase
{
    [SerializeField] private AudioSource _headmistressAudioSource;
    public override void Begin()
    {
        AudioManager.Instance.PlayMusic(MusicTracks.Level02Music, fadeDuration: 1f);
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressHall, _headmistressAudioSource);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 02 deactivated!");
    }
}
