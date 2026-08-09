using UnityEngine;
using Zenject;
using DG.Tweening;

public class UnlockMaskInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Hint")]
    [SerializeField] private float _hintXOffset = 0.3f;
    [SerializeField] private float _hintYOffset = 1.5f;
    [SerializeField] private float _hintZOffset = 0f;

    [Header("Pointing Finger")]
    [SerializeField] private Transform _pointingFingerTransform;
    [SerializeField] private Vector3 _positionDelta = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private Vector3 _rotationDelta = new Vector3(0f, 0f, 10f);
    [SerializeField] private float _animationDuration = 0.8f;

    public string HintLabel => "Take";
    public float HintYOffset => _hintYOffset;
    public float HintXOffset => _hintXOffset;
    public float HintZOffset => _hintZOffset;
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    [Inject] private LockableActionsManager _lockableActionsManager;
    [Inject] private IMaskStateManager _maskStateManager;

    private Sequence _fingerSequence;

    private void Start()
    {
        // Mask was already unlocked in a previous session - don't spawn the pickup again.
        if (!_lockableActionsManager.IsActionLocked(LockableActionsManager.LockableActionType.ToggleMask))
        {
            if (_pointingFingerTransform != null)
                Destroy(_pointingFingerTransform.gameObject);

            Destroy(gameObject);
            return;
        }

        if (_pointingFingerTransform == null) return;

        Vector3 startPosition = _pointingFingerTransform.localPosition;
        Vector3 startRotation = _pointingFingerTransform.localEulerAngles;

        _fingerSequence = DOTween.Sequence();
        _fingerSequence
            .Append(_pointingFingerTransform.DOLocalMove(startPosition + _positionDelta, _animationDuration).SetEase(Ease.InOutSine))
            .Join(_pointingFingerTransform.DOLocalRotate(startRotation + _rotationDelta, _animationDuration).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Interact(Player player)
    {
        if (player.Grabbing.IsHolding) return;

        _lockableActionsManager.UnlockAction(LockableActionsManager.LockableActionType.ToggleMask);
        _maskStateManager.ChangeState(MaskStateType.Wearing);

        _fingerSequence?.Kill();

        if (_pointingFingerTransform != null)
            Destroy(_pointingFingerTransform.gameObject);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _fingerSequence?.Kill();
    }
}
