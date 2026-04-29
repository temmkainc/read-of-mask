using DG.Tweening;
using UnityEngine;

public class CurtainInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private Transform _transformToMove;
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    private bool _isOpen = false;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;

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

        _transformToMove.DOKill();

        if (_isOpen)
        {
            _transformToMove.DOMove(_targetPoint.position, _duration).SetEase(_ease);
            _transformToMove.DOScale(_targetPoint.localScale, _duration).SetEase(_ease);
        }
        else
        {
            _transformToMove.DOMove(_originalPosition, _duration).SetEase(_ease);
            _transformToMove.DOScale(_originalScale, _duration).SetEase(_ease);
        }
    }
}