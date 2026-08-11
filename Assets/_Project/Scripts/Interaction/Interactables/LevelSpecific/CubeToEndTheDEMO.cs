using UnityEngine;
using DG.Tweening;
using TMPro;
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
    [SerializeField] private OverlayHintsSceneContainer _overlayHintsSceneContainer;
    [SerializeField] private LookCloserInteractableBase _chair;

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

                foreach (var text in _texts)
                {
                    if (text == null) continue;
                    seq.Append(text.DOFade(0f, _textFadeDuration).SetEase(Ease.InOutQuad))
                       .AppendInterval(_textFadeDelay);
                }

                seq.OnComplete(() =>
                    DOVirtual.DelayedCall(5f, () =>
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
