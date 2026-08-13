using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EffectsContainer : MonoBehaviour
{
    [field: SerializeField] public EyeOpenEffect EyeOpenEffect { get; private set; }

    [Header("Level Load Fade")]
    [Tooltip("Full-screen white Image used for the fade-from-white transition when a level loads (e.g. coming from the menu).")]
    [SerializeField] private Image _fadeImage;

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
            .OnComplete(() => _fadeImage.gameObject.SetActive(false));
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

        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Append(_fadeImage.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
        seq.AppendInterval(holdDuration);
        seq.Append(_fadeImage.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
        seq.OnComplete(() => _fadeImage.gameObject.SetActive(false));
    }
}
