using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Level00 : LevelBase
{
    [Inject] private CameraEffects _cameraEffects;
    [SerializeField] private AudioSource _headmistressAudioSource;

    private const float SECONDS_BEFORE_COMPLETE = 0.2f;
    public override void Begin()
    {
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        //AudioManager.Instance.PlayMusic(MusicTracks.Level00Intro);
        await UniTask.WaitForSeconds(1f);
        await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressIntroSpeech, _headmistressAudioSource);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        _cameraEffects.ShakeExplosion();
        await UniTask.WaitForSeconds(SECONDS_BEFORE_COMPLETE);
        await base.BeforeCompleteAsync();
    }
}
