using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using DG.Tweening;

public class ClockWithMovingArrowsInteractable : LookCloserInteractableBase
{
    [SerializeField] private Transform _hourArrow;
    [SerializeField] private Transform _minuteArrow;
    [SerializeField] private Outline _hourOutline;
    [SerializeField] private Outline _minuteOutline;
    [SerializeField] private CanvasGroup _uiCanvasGroup;
    [SerializeField] private float _rotateDuration = 0.2f;
    [SerializeField] private int _correctHourStep;
    [SerializeField] private int _correctMinuteStep;
    [SerializeField] private Transform _cuckooWithNote;
    [SerializeField] private AudioSource _cuckooAudioSource;

    [Header("Cuckoo Animation")]
    [SerializeField] private float _cuckooForwardDistance = 0.3f;
    [SerializeField] private float _cuckooBackwardDistance = 0.15f;
    [SerializeField] private float _cuckooMoveDuration = 0.25f;
    [SerializeField] private int _cuckooCycles = 3;
    [SerializeField] private Ease _cuckooEase = Ease.InOutSine;
    [Inject] private InputManager _inputManager;

    private int _currentArrowIndex = 0;
    private int _arrowCount = 2;
    private int _hourStep = 0;
    private int _minuteStep = 0;
    private bool _isActive = false;
    private bool _isRotating = false;
    private bool _wasSetCorrectTime = false;

    private const float STEP_DEGREES = 30f;

    protected override void On_PlayerStateChanged(PlayerStateType type)
    {
        var wasLookCloser = _previousPlayerStateType == PlayerStateType.LookCloser;

        base.On_PlayerStateChanged(type);

        if (_wasSetCorrectTime)
            return;

        if (type == PlayerStateType.LookCloser)
        {
            _isActive = true;
            UpdateOutlines();
            _inputManager.LookCloserDirectionAction.performed += On_DirectionPerformed;
            _uiCanvasGroup.alpha = 1f;
        }
        else if (wasLookCloser)
        {
            _isActive = false;
            DisableAllOutlines();
            _inputManager.LookCloserDirectionAction.performed -= On_DirectionPerformed;
            _uiCanvasGroup.alpha = 0f;
        }
    }

    private void On_DirectionPerformed(InputAction.CallbackContext ctx)
    {
        if (!_isActive || _isRotating)
            return;

        Vector2 direction = ctx.ReadValue<Vector2>();

        if (direction.y > 0.5f)
        {
            _currentArrowIndex = (_currentArrowIndex - 1 + _arrowCount) % _arrowCount;
            UpdateOutlines();
        }
        else if (direction.y < -0.5f)
        {
            _currentArrowIndex = (_currentArrowIndex + 1) % _arrowCount;
            UpdateOutlines();
        }
        else if (direction.x > 0.5f)
        {
            RotateCurrentArrow(-1);
        }
        else if (direction.x < -0.5f)
        {
            RotateCurrentArrow(1);
        }
    }

    private void RotateCurrentArrow(int direction)
    {
        int delta = -direction;

        if (_currentArrowIndex == 0)
        {
            _hourStep = (_hourStep + delta + 12) % 12;
            RotateTo(_hourArrow, _hourStep * STEP_DEGREES);
        }
        else
        {
            _minuteStep = (_minuteStep + delta + 12) % 12;
            RotateTo(_minuteArrow, _minuteStep * STEP_DEGREES);
        }

        CheckCorrectTime();
    }

    private void RotateTo(Transform arrow, float targetZAngle)
    {
        AudioManager.Instance.PlaySFX(SfxClips.KeypadFocus, volumeScale: 0.2f, source: _cuckooAudioSource);
        _isRotating = true;
        Vector3 target = new Vector3(0f, 0f, targetZAngle);
        arrow.DOLocalRotate(target, _rotateDuration, RotateMode.Fast)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                _isRotating = false;
            });
    }

    private void CheckCorrectTime()
    {
        Debug.Log($"Hour: {_hourStep}/{_correctHourStep}, Minute: {_minuteStep}/{_correctMinuteStep}");
        if (_hourStep == _correctHourStep && _minuteStep == _correctMinuteStep)
            OnCorrectTimeSet();
    }

    protected virtual void OnCorrectTimeSet()
    {
        _isActive = false;
        _wasSetCorrectTime = true;

        DisableAllOutlines();

        _inputManager.LookCloserDirectionAction.performed -= On_DirectionPerformed;

        _uiCanvasGroup.alpha = 0f;

        PlayCuckooAnimation();
    }

    private void PlayCuckooAnimation()
    {
        AudioManager.Instance.PlaySFX(SfxClips.ChaosCuckoo, source: _cuckooAudioSource);
        Vector3 startPosition = _cuckooWithNote.localPosition;

        Vector3 forwardPosition =
            startPosition + Vector3.forward * _cuckooForwardDistance;

        Vector3 backwardPosition =
            forwardPosition - Vector3.forward * _cuckooBackwardDistance;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            _cuckooWithNote.DOLocalMove(forwardPosition, _cuckooMoveDuration)
                .SetEase(_cuckooEase));

        for (int i = 0; i < _cuckooCycles; i++)
        {
            sequence.Append(
                _cuckooWithNote.DOLocalMove(backwardPosition, _cuckooMoveDuration)
                    .SetEase(_cuckooEase));

            sequence.Append(
                _cuckooWithNote.DOLocalMove(forwardPosition, _cuckooMoveDuration)
                    .SetEase(_cuckooEase));
        }
    }

    private void UpdateOutlines()
    {    
        AudioManager.Instance.PlaySFX(SfxClips.KeypadFocus, volumeScale: 0.2f, source: _cuckooAudioSource);
        _hourOutline.enabled = _currentArrowIndex == 0;
        _minuteOutline.enabled = _currentArrowIndex == 1;
    }

    private void DisableAllOutlines()
    {
        _hourOutline.enabled = false;
        _minuteOutline.enabled = false;
    }
}