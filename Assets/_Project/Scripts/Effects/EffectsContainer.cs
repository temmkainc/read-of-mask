using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EffectsContainer : MonoBehaviour
{
    [field: SerializeField] public EyeOpenEffect EyeOpenEffect { get; private set; }

    [Header("Level Load Fade")]
    [Tooltip("Full-screen white Image used for the fade-from-white transition when a level loads (e.g. coming from the menu).")]
    [SerializeField] private Image _fadeImage;

    [Header("Obligatory Sequence Rollback")]
    [Tooltip("Global Volume (weight 0 at rest) holding the rollback look: vignette, chromatic aberration, lens distortion, grain.")]
    [SerializeField] private Volume _rollbackVolume;
    [Tooltip("Rising swell played while the correction line is heard, before the white-out.")]
    [SerializeField] private AudioClip _rollbackSwell;
    [Tooltip("Cinematic hit played as the screen goes white.")]
    [SerializeField] private AudioClip _rollbackHit;
    [SerializeField, Range(0f, 1f)] private float _rollbackTensionWeight = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _rollbackMusicDuck = 0.2f;
    [SerializeField] private float _rollbackFadeIn = 0.45f;
    [SerializeField] private float _rollbackHold = 0.35f;
    [SerializeField] private float _rollbackFadeOut = 1.2f;

    private Tween _rollbackTween;

    /// <summary>
    /// Plays a fade from opaque white to transparent, for a smooth transition into a level
    /// (e.g. coming from the main menu, or resuming a save). No-ops if no fade image is assigned.
    /// </summary>
    public void FadeFromWhite(float duration = 5f)
    {
        if (_fadeImage == null)
            return;

        _fadeImage.gameObject.SetActive(true);
        _fadeImage.color = Color.white;
        _fadeImage
            .DOFade(0f, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => _fadeImage.gameObject.SetActive(false))
            .SetLink(_fadeImage.gameObject);
    }

    /// <summary>
    /// A quick white flash - fade in, brief hold, fade out. Distinct from FadeFromWhite (the slow
    /// one-time load transition) - this is for short, in-gameplay correction cues (e.g. an
    /// obligatory-step rollback), so it needs to be fast and not visually confusable with loading.
    /// </summary>
    public void FlashWhite(float fadeInDuration = 0.15f, float holdDuration = 0.1f, float fadeOutDuration = 0.25f)
    {
        if (_fadeImage == null)
            return;

        _fadeImage.gameObject.SetActive(true);
        _fadeImage.color = new Color(1f, 1f, 1f, 0f);

        DG.Tweening.Sequence seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(_fadeImage.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDuration);
        seq.Append(_fadeImage.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
        seq.OnComplete(() => _fadeImage.gameObject.SetActive(false));
    }

    /// <summary>
    /// First half of an obligatory-sequence rollback: while the correction line plays, the rollback
    /// look creeps in, a swell rises and the music ducks. Follow with PlayRollback.
    /// </summary>
    public void BeginRollbackTension(float duration)
    {
        if (AudioManager.Instance != null)
        {
            if (_rollbackSwell != null)
                AudioManager.Instance.PlaySFX(_rollbackSwell);
            AudioManager.Instance.DuckMusic(_rollbackMusicDuck, duration, _rollbackFadeIn + _rollbackHold, _rollbackFadeOut);
        }

        _rollbackTween?.Kill();
        if (_rollbackVolume != null)
        {
            _rollbackTween = DOTween.To(() => _rollbackVolume.weight, w => _rollbackVolume.weight = w, _rollbackTensionWeight, duration)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject);
        }
    }

    /// <summary>
    /// The rollback itself: a cinematic hit, white-out, <paramref name="onPeak"/> at full white (reset the
    /// level there, out of sight), a short hold, then a slow fade back in.
    /// </summary>
    public void PlayRollback(Action onPeak)
    {
        if (_fadeImage == null)
        {
            onPeak?.Invoke();
            return;
        }

        _rollbackTween?.Kill();
        if (_rollbackHit != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(_rollbackHit);

        _fadeImage.gameObject.SetActive(true);
        var color = Color.white;
        color.a = 0f;
        _fadeImage.color = color;

        var seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(_fadeImage.DOFade(1f, _rollbackFadeIn).SetEase(Ease.InQuad));
        if (_rollbackVolume != null)
            seq.Join(DOTween.To(() => _rollbackVolume.weight, w => _rollbackVolume.weight = w, 1f, _rollbackFadeIn));
        seq.AppendCallback(() =>
        {
            // never leave the screen stuck white if the reset throws
            try { onPeak?.Invoke(); }
            catch (Exception e) { Debug.LogException(e); }
        });
        seq.AppendInterval(_rollbackHold);
        seq.Append(_fadeImage.DOFade(0f, _rollbackFadeOut).SetEase(Ease.OutCubic));
        if (_rollbackVolume != null)
            seq.Join(DOTween.To(() => _rollbackVolume.weight, w => _rollbackVolume.weight = w, 0f, _rollbackFadeOut).SetEase(Ease.OutCubic));
        seq.OnComplete(() => _fadeImage.gameObject.SetActive(false));
        _rollbackTween = seq;
    }
}
