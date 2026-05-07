using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class CubeToEndTheDEMO : LetterCube
{
    [SerializeField] private CanvasGroup _showEndDemoCanvasGroup;
    [SerializeField] private TMP_Text[] _texts;
    [SerializeField] private float _textFadeDuration = 0.8f;
    [SerializeField] private float _textFadeDelay = 0.3f;
    [SerializeField] private OverlayHintsSceneContainer _overlayHintsSceneContainer;
    private bool _hasBeenTriggered;

    public override void Grab(Player player, Transform holdPoint)
    {
        base.Grab(player, holdPoint);

        if (_hasBeenTriggered) return;
        _hasBeenTriggered = true;

        foreach (var text in _texts)
            text.alpha = 0f;

        _overlayHintsSceneContainer.gameObject.SetActive(false);

        _showEndDemoCanvasGroup.alpha = 0f;
        _showEndDemoCanvasGroup
            .DOFade(1f, 5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                Sequence seq = DOTween.Sequence();

                foreach (var text in _texts)
                    seq.Append(text.DOFade(1f, _textFadeDuration).SetEase(Ease.InOutQuad))
                       .AppendInterval(_textFadeDelay);

                foreach (var text in _texts)
                    seq.Append(text.DOFade(0f, _textFadeDuration).SetEase(Ease.InOutQuad))
                       .AppendInterval(_textFadeDelay);

                seq.OnComplete(() =>
                    DOVirtual.DelayedCall(5f, () => SceneManager.LoadScene(0)));
            });
    }
}