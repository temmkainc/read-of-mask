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
    [SerializeField] private float _fadeOutDuration = 0.25f;
    [SerializeField] private float _fadeInDuration = 0.5f;

    [Inject] private ObjectiveTracker _tracker;

    private Sequence _sequence;

    private void Awake()
    {
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
        _sequence = DOTween.Sequence().SetLink(gameObject);

        if (_canvasGroup.alpha > 0f)
            _sequence.Append(_canvasGroup.DOFade(0f, _fadeOutDuration));

        _sequence.AppendCallback(() => _label.text = text);

        if (string.IsNullOrEmpty(text))
            return;

        _sequence.Append(_canvasGroup.DOFade(1f, _fadeInDuration));
        if (_icon != null)
        {
            _icon.localScale = Vector3.one;
            _sequence.Join(_icon.DOPunchScale(Vector3.one * 0.25f, 0.45f, 6, 0.6f));
        }
    }
}
