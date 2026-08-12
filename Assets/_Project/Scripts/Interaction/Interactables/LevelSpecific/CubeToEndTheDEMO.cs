using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

public class CubeToEndTheDEMO : LetterCube, IDynamicHintLabel
{
    [SerializeField] private AudioSource _headmistressAudioSource;
    [SerializeField] private CanvasGroup _showEndDemoCanvasGroup;
    [SerializeField] private GameObject _faceless;
    [SerializeField] private TMP_Text[] _texts;
    [SerializeField] private float _textFadeDuration = 0.8f;
    [SerializeField] private float _textFadeDelay = 0.3f;
    [Tooltip("How long the text stays fully visible after all lines have faded in, before it starts fading back out.")]
    [SerializeField] private float _textDisplayDuration = 2.5f;
    [SerializeField] private OverlayHintsSceneContainer _overlayHintsSceneContainer;
    [SerializeField] private LookCloserInteractableBase _chair;

    [Header("Ending Backdrop")]
    [Tooltip("Full-screen backdrop behind the ending text. Tweened from its current color to white right after the text fades out, so the screen ends on white (matching the white fade-in when the next chapter loads) instead of staying on its original color.")]
    [SerializeField] private Image _endDemoBackdropImage;
    [SerializeField] private float _backdropToWhiteDuration = 1f;

    [Tooltip("Delay after the backdrop turns white before actually transitioning to the next chapter scene.")]
    [SerializeField] private float _sceneTransitionDelay = 1f;

    private bool _hasBeenTriggered;

    public override string HintLabel => !_hasBeenTriggered ? "Take out" : "Grab";
    public override bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;
    public event Action OnHintChanged;

    /// <summary>Fired once the ending text sequence (and its trailing delay) has fully finished.
    /// Listened to by a level-complete condition on this level, so the normal LevelBase/LevelManager
    /// flow (including handing off to the next chapter scene) runs from there.</summary>
    public event Action OnEndSequenceComplete;

    public override void Grab(Player player, Transform holdPoint)
    {
        base.Grab(player, holdPoint);
        if (_hasBeenTriggered) return;
        _hasBeenTriggered = true;
        OnHintChanged?.Invoke();

        foreach (var text in _texts)
        {
            if (text != null)
                text.alpha = 0f;
        }

        AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressGettingStarted, _headmistressAudioSource).Forget();
        _overlayHintsSceneContainer.gameObject.SetActive(false);

        _faceless.SetActive(true);
        _chair.IsEnabled = false;

        _showEndDemoCanvasGroup.alpha = 0f;
        DOVirtual.DelayedCall(16.5f, () => PlayEndDemoSequence());

    }

    public void PlayEndDemoSequence()
    {
        AudioManager.Instance.PlaySFX(SfxClips.ImpactEndgame);
        _showEndDemoCanvasGroup
            .DOFade(1f, 1f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                Sequence seq = DOTween.Sequence();

                foreach (var text in _texts)
                {
                    if (text == null) continue;
                    seq.Append(text.DOFade(1f, _textFadeDuration).SetEase(Ease.InOutQuad))
                       .AppendInterval(_textFadeDelay);
                }

                seq.AppendInterval(_textDisplayDuration);

                foreach (var text in _texts)
                {
                    if (text == null) continue;
                    seq.Append(text.DOFade(0f, _textFadeDuration).SetEase(Ease.InOutQuad))
                       .AppendInterval(_textFadeDelay);
                }

                if (_endDemoBackdropImage != null)
                {
                    seq.Append(_endDemoBackdropImage.DOColor(Color.white, _backdropToWhiteDuration).SetEase(Ease.InOutQuad));
                }

                seq.OnComplete(() =>
                    DOVirtual.DelayedCall(_sceneTransitionDelay, () =>
                    {
                        AudioManager.Instance.StopMusic();
                        OnEndSequenceComplete?.Invoke();
                    }));
            });
    }
    public override void Release(Vector3 throwForce, bool isExternal = false)
    {
        base.Release(throwForce, isExternal);
        OnHintChanged?.Invoke();
    }
}
