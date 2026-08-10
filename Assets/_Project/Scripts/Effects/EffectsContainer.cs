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
    public void FadeFromWhite(float duration = 3f)
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
}
