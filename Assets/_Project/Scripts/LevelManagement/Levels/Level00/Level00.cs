using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Level00 : LevelBase
{
    [Inject] private CameraEffects _cameraEffects;
    [Inject] private EffectsContainer _effectsContainer;
    [SerializeField] private AudioSource _headmistressAudioSource;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private bool _withEyeEffect;

    private const float SECONDS_BEFORE_COMPLETE = 0.2f;
    private CancellationTokenSource _levelCts;

    // This level already plays its own fade/eye-open intro below - don't also play the generic load fade.
    public override bool HasCustomLoadTransition => true;

    public override void Begin()
    {
        _levelCts = new CancellationTokenSource();
        base.Begin();
        PlayIntroSequence(_levelCts.Token).Forget();
    }

    private async UniTask PlayIntroSequence(CancellationToken ct)
    {
        AudioManager.Instance.PlayMusic(MusicTracks.Level00Intro);

        if (_withEyeEffect)
        {
            _effectsContainer.EyeOpenEffect.gameObject.SetActive(true);
            _fadeImage.color = Color.white;
            await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
            _fadeImage
                .DOFade(0f, 4f)
                .SetEase(Ease.InOutQuad);
            await UniTask.WaitForSeconds(2f, cancellationToken: ct);
            _effectsContainer.EyeOpenEffect.Play().Forget();
        }
        await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);
        await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressIntroSpeech, _headmistressAudioSource, ct);
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
