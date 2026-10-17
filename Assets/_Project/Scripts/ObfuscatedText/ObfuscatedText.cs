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
            .SetEase(_fadeEase)
            .SetLink(_obfuscatedLabelCanvasGroup.gameObject);

        _translationTween = _translationLabelCanvasGroup
            .DOFade(1f, _fadeDuration)
            .SetDelay(_fadeDelay)
            .SetEase(_fadeEase)
            .SetLink(_translationLabelCanvasGroup.gameObject);
    }

    public void OnSightExit()
    {
        KillTweens();

        _obfuscatedTween = _obfuscatedLabelCanvasGroup
            .DOFade(1f, _fadeDuration)
            .SetDelay(_fadeDelay)
            .SetEase(_fadeEase)
            .SetLink(_obfuscatedLabelCanvasGroup.gameObject);

        _translationTween = _translationLabelCanvasGroup
            .DOFade(0f, _fadeDuration)
            .SetDelay(_fadeDelay)
            .SetEase(_fadeEase)
            .SetLink(_translationLabelCanvasGroup.gameObject);
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

    private void OnDestroy()
    {
        // The mask manager outlives this text (e.g. when a finished level is destroyed mid-scene)
        if (_maskStateManager != null)
            _maskStateManager.OnStateChanged -= On_MaskStateChanged;
    }
}
