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

        if (!string.IsNullOrEmpty(text))
            _sequence.Append(_canvasGroup.DOFade(1f, _fadeInDuration));
    }
}
