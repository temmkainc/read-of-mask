using UnityEngine;
using Zenject;
using DG.Tweening;

/// <summary>
/// Shows a brief fade in/out indicator whenever the game autosaves.
/// Setup (same pattern as OverlayHintsSceneContainer's hint texts):
///  1. Add a UI element (e.g. a small "Saving...'' label) under your canvas.
///  2. Add a CanvasGroup to it and set its alpha to 0.
///  3. Put this component on it (or any scene object) and assign that CanvasGroup below.
/// Note: saves currently happen synchronously (small JSON file), so this only fires
/// once the write has already finished - there's no separate "in progress'' state to show.
/// </summary>
public class SaveIndicatorUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _visibleDuration = 1.2f;
    [SerializeField] private float _fadeDuration = 0.25f;

    [Inject] private ISaveService _saveService;

    private Sequence _sequence;

    private void Awake()
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        _saveService.OnSaved += HandleSaved;
    }

    private void OnDisable()
    {
        _saveService.OnSaved -= HandleSaved;
        _sequence?.Kill();
    }

    private void HandleSaved()
    {
        if (_canvasGroup == null)
            return;

        _sequence?.Kill();
        _sequence = DOTween.Sequence()
            .Append(_canvasGroup.DOFade(1f, _fadeDuration))
            .AppendInterval(_visibleDuration)
            .Append(_canvasGroup.DOFade(0f, _fadeDuration));
    }
}
