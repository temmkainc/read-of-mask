using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using Zenject;

public class DoorInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private Transform _transformToRotate;
    [SerializeField] private Axis _rotationAxis = Axis.Y;
    [SerializeField] private float _openAnglePush = 90f;
    [SerializeField] private float _openAnglePull = -90f;
    [SerializeField] private bool _isDependableFromSide = true;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Ease _ease = Ease.InOutQuad;

    [Inject] private PlayerFirstPersonHandsController _handsController;

    public enum Axis { X, Y, Z }

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    private bool _isOpen = false;
    private bool _disabled = false;
    private Tween _tween;
    private float _currentTargetAngle = 0f;
    private Vector3 _closedRotation;

    private void Awake()
    {
        _closedRotation = _transformToRotate.localEulerAngles;
    }

    public void Interact(Player player)
    {
        if (_disabled) {
            _handsController.PlayNotAllowedAnimation();
            return;
        }

        _isOpen = !_isOpen;
        _tween?.Kill();

        float closedAngle = GetAxis(_closedRotation);
        float targetAngle;

        if (_isOpen)
        {
            float openAngle;
            if (_isDependableFromSide)
            {
                Vector3 toPlayer = player.transform.position - _transformToRotate.position;
                bool isInFront = Vector3.Dot(_transformToRotate.forward, toPlayer) > 0f;
                openAngle = isInFront ? _openAnglePush : _openAnglePull;
            }
            else
            {
                openAngle = _openAnglePush;
            }

            targetAngle = closedAngle + openAngle;
            _currentTargetAngle = targetAngle;
        }
        else
        {
            targetAngle = closedAngle;
            _currentTargetAngle = closedAngle;
        }

        float currentAngle = GetAxis(_transformToRotate.localEulerAngles);
        if (currentAngle > 180f) currentAngle -= 360f;

        float totalTravel = Mathf.Abs(targetAngle - currentAngle);
        float maxTravel = Mathf.Max(Mathf.Abs(_openAnglePush), Mathf.Abs(_openAnglePull));
        float remaining = Mathf.Clamp01(totalTravel / maxTravel);
        float scaledDuration = _duration * remaining;

        _tween = _transformToRotate
            .DOLocalRotate(WithAxis(_closedRotation, targetAngle), scaledDuration)
            .SetEase(_ease);
    }

    public async Task CloseAndDisable()
    {
        _disabled = true;
        _tween?.Kill();

        if (_isOpen)
        {
            _isOpen = false;

            float closedAngle = GetAxis(_closedRotation);
            _currentTargetAngle = closedAngle;

            float currentAngle = GetAxis(_transformToRotate.localEulerAngles);
            if (currentAngle > 180f) currentAngle -= 360f;

            float totalTravel = Mathf.Abs(closedAngle - currentAngle);
            float maxTravel = Mathf.Max(Mathf.Abs(_openAnglePush), Mathf.Abs(_openAnglePull));
            float remaining = Mathf.Clamp01(totalTravel / maxTravel);
            float scaledDuration = _duration * remaining;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            _tween = _transformToRotate
                .DOLocalRotate(_closedRotation, scaledDuration)
                .SetEase(_ease)
                .OnComplete(() => tcs.SetResult(true));

            await tcs.Task;
        }
    }

    private float GetAxis(Vector3 v) => _rotationAxis switch
    {
        Axis.X => v.x,
        Axis.Y => v.y,
        Axis.Z => v.z,
        _ => v.y
    };

    private Vector3 GetAxisVector() => _rotationAxis switch
    {
        Axis.X => _transformToRotate.right,
        Axis.Y => _transformToRotate.up,
        Axis.Z => _transformToRotate.forward,
        _ => _transformToRotate.up
    };

    private Vector3 WithAxis(Vector3 v, float value) => _rotationAxis switch
    {
        Axis.X => new Vector3(value, v.y, v.z),
        Axis.Y => new Vector3(v.x, value, v.z),
        Axis.Z => new Vector3(v.x, v.y, value),
        _ => new Vector3(v.x, value, v.z)
    };
}