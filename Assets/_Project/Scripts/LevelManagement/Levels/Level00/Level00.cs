using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Zenject;

public class Level00 : LevelBase
{
    [Inject] private CameraEffects _cameraEffects;
    [SerializeField] private AudioSource _headmistressAudioSource;

    private const float SECONDS_BEFORE_COMPLETE = 0.2f;
    private CancellationTokenSource _levelCts;
    public override void Begin()
    {
        _levelCts = new CancellationTokenSource();
        base.Begin();
        PlayIntroSequence(_levelCts.Token).Forget();
    }

    private async UniTask PlayIntroSequence(CancellationToken ct)
    {
        AudioManager.Instance.PlayMusic(MusicTracks.Level00Intro);
        await UniTask.WaitForSeconds(1f, cancellationToken: ct);
        //await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressIntroSpeech, _headmistressAudioSource, ct);
        //await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressYouAreDear, _headmistressAudioSource, ct);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        _cameraEffects.ShakeExplosion();
        await UniTask.WaitForSeconds(SECONDS_BEFORE_COMPLETE);
        await base.BeforeCompleteAsync();
    }
    private void OnDisable()
    {
        _levelCts?.Cancel();
        _levelCts?.Dispose();
        _levelCts = null;
    }
}
