using UnityEngine;
using TMPro;
using DG.Tweening;
using Zenject;
using System;

public class ObfuscatedText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup _obfuscatedLabelCanvasGroup;
    [SerializeField] private CanvasGroup _translationLabelCanvasGroup;
    [SerializeField] private TMP_Text _translationTMP;
    [SerializeField] private TMP_Text _obfuscatedTMP;

    [Header("Data")]
    [SerializeField] private string _translation;

    [Header("Animation")]
    [SerializeField] private float _fadeDelay = 0.2f;
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private Ease _fadeEase = Ease.OutQuad;

    private Tween _obfuscatedTween;
    private Tween _translationTween;

    [Inject] IMaskStateManager _maskStateManager;

    private void Awake()
    {
        _obfuscatedTMP.text = _translation;
        _translationTMP.text = _translation;
        _obfuscatedLabelCanvasGroup.alpha = 1f;
        _translationLabelCanvasGroup.alpha = 0f;
        _maskStateManager.OnStateChanged += On_MaskStateChanged;
    }

    private void On_MaskStateChanged(MaskStateType state)
    {
        if (state == MaskStateType.Wearing)
        {
            OnSightEnter();
        }
        else
        {
            OnSightExit();
        }
    }

    public void OnSightEnter()
    {
        _translationTMP.text = _translation;

        KillTweens();

        _obfuscatedTween = _obfuscatedLabelCanvasGroup
            .DOFade(0f, _fadeDuration)
            .SetDelay(_fadeDelay)
            .SetEase(_fadeEase);

        _translationTween = _translationLabelCanvasGroup
            .DOFade(1f, _fadeDuration)
            .SetDelay(_fadeDelay)
            .SetEase(_fadeEase);
    }

    public void OnSightExit()
    {
        KillTweens();

        _obfuscatedTween = _obfuscatedLabelCanvasGroup
            .DOFade(1f, _fadeDuration)
            .SetDelay(_fadeDelay)
            .SetEase(_fadeEase);

        _translationTween = _translationLabelCanvasGroup
            .DOFade(0f, _fadeDuration)
            .SetDelay(_fadeDelay)
            .SetEase(_fadeEase);
    }

    private void KillTweens()
    {
        _obfuscatedTween?.Kill();
        _translationTween?.Kill();
    }

    private void OnDisable()
    {
        KillTweens();
    }
}
