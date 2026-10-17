using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

/// <summary>
/// Shows the current objective from ObjectiveTracker as on-screen text, fading out the old one and
/// fading in the new one. Hidden while there is no objective. Drop the ObjectiveHud prefab into a
/// scene's overlay canvas to use it.
/// </summary>
public class ObjectiveHud : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private CanvasGroup _canvasGroup;
    [Tooltip("Optional quest icon next to the text; it gives a small pop when a new objective appears.")]
    [SerializeField] private RectTransform _icon;
    [Tooltip("Fade out of the old objective when switching to the next one.")]
    [SerializeField] private float _fadeOutDuration = 0.25f;
    [SerializeField] private float _fadeInDuration = 0.5f;

    [Header("When the objectives run out (e.g. the sequence is done)")]
    [Tooltip("How long the last objective stays on screen before it starts to go.")]
    [SerializeField] private float _hideDelay = 0.4f;
    [SerializeField] private float _hideDuration = 1.1f;
    [Tooltip("How far (px) the text drifts up while it fades away.")]
    [SerializeField] private float _hideDrift = 16f;

    [Inject] private ObjectiveTracker _tracker;

    private Sequence _sequence;
    private RectTransform _rect;
    private Vector2 _restPosition;

    private void Awake()
    {
        _rect = (RectTransform)transform;
        _restPosition = _rect.anchoredPosition;
        _canvasGroup.alpha = 0f;
        _label.text = string.Empty;
    }

    private void OnEnable()
    {
        _tracker.OnObjectiveChanged += Show;
        Show(_tracker.Current);
    }

    private void OnDisable()
    {
        _tracker.OnObjectiveChanged -= Show;
        _sequence?.Kill();
    }

    private void Show(string text)
    {
        _sequence?.Kill();
        _rect.anchoredPosition = _restPosition;
        _sequence = DOTween.Sequence().SetLink(gameObject);

        if (string.IsNullOrEmpty(text))
        {
            Hide();
            return;
        }

        if (_canvasGroup.alpha > 0f)
            _sequence.Append(_canvasGroup.DOFade(0f, _fadeOutDuration));

        _sequence.AppendCallback(() => _label.text = text);
        _sequence.Append(_canvasGroup.DOFade(1f, _fadeInDuration));
        if (_icon != null)
        {
            _icon.localScale = Vector3.one;
            _sequence.Join(_icon.DOPunchScale(Vector3.one * 0.25f, 0.45f, 6, 0.6f));
        }
    }

    // No objective left: let the last one linger, then drift up and fade away gently.
    private void Hide()
    {
        if (_canvasGroup.alpha <= 0f)
        {
            _label.text = string.Empty;
            return;
        }

        _sequence.AppendInterval(_hideDelay);
        _sequence.Append(_canvasGroup.DOFade(0f, _hideDuration).SetEase(Ease.InOutSine));
        _sequence.Join(_rect.DOAnchorPosY(_restPosition.y + _hideDrift, _hideDuration).SetEase(Ease.OutSine));
        _sequence.AppendCallback(() =>
        {
            _label.text = string.Empty;
            _rect.anchoredPosition = _restPosition;
        });
    }
}
