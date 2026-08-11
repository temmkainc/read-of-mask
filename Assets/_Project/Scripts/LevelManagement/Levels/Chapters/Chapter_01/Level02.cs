using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level02 : LevelBase
{
    [SerializeField] private AudioSource _headmistressAudioSource;
    [SerializeField] private AudioSource _swingAudioSource;
    public override void Begin()
    {
        AudioManager.Instance.PlayMusic(MusicTracks.Level02Music, fadeDuration: 1f);
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressHall, _headmistressAudioSource);
        AudioManager.Instance.PlaySFX(SfxClips.SwingLooped, volumeScale: 0.7f, source: _swingAudioSource, isLooping: true);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 02 deactivated!");
    }
}
