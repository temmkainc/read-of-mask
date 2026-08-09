using System;
using DG.Tweening;
using UnityEngine;

public class CurtainInteractable : MonoBehaviour, IInteractable, IHighlightable, IDynamicHintLabel
{
    [SerializeField] private Transform _transformToMove;
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    private bool _isOpen = false;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;

    public event Action OnHintChanged;

    [Header("Hint")]
    [SerializeField] private float _hintXOffset = 0.3f;
    [SerializeField] private float _hintYOffset = 0.5f;
    [SerializeField] private float _hintZOffset = 0f;

    public string HintLabel => _isOpen ? "Open Curtain" : "Close Curtain";

    public float HintYOffset => _hintYOffset;
    public float HintXOffset => _hintXOffset;
    public float HintZOffset => _hintZOffset;

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    private void Awake()
    {
        _originalPosition = _transformToMove.position;
        _originalScale = _transformToMove.localScale;
    }

    public void Interact(Player player = null)
    {
        if (player.Grabbing.IsHolding) return;

        _isOpen = !_isOpen;
        OnHintChanged?.Invoke();

        _transformToMove.DOKill();

        if (_isOpen)
        {
            AudioManager.Instance.PlaySFX(SfxClips.ShowerCurtainClose, volumeScale: 0.5f);
            _transformToMove.DOMove(_targetPoint.position, _duration).SetEase(_ease);
            _transformToMove.DOScale(_targetPoint.localScale, _duration).SetEase(_ease);
        }
        else
        {
            AudioManager.Instance.PlaySFX(SfxClips.ShowerCurtainOpen, volumeScale: 0.5f);
            _transformToMove.DOMove(_originalPosition, _duration).SetEase(_ease);
            _transformToMove.DOScale(_originalScale, _duration).SetEase(_ease);
        }
    }
}