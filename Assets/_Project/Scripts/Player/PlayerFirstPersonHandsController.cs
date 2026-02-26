using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class FirstPersonHandsController : MonoBehaviour
{
    [SerializeField] private InputsManager _inputHandler;

    [Inject] IMaskStateManager _maskStateManager;

    private Animator _animator;

    private bool _holdingMiddleFingerLastFrame = false;
    private bool _holdingPointingFingerLastFrame = false;

    private readonly int SpeedHash = Animator.StringToHash("Speed");
    private readonly int DoMiddleFingerHash = Animator.StringToHash("DoMiddleFinger");
    private readonly int IsHoldingMiddleFingerHash = Animator.StringToHash("IsHoldingMiddleFinger");
    private readonly int DoPointingFingerHash = Animator.StringToHash("DoPointingFinger");
    private readonly int IsHoldingPointingFingerHash = Animator.StringToHash("IsHoldingPointingFinger");

    private readonly int PutOnMaskHash = Animator.StringToHash("DoPutOnMask");
    private readonly int PutOffMaskHash = Animator.StringToHash("DoPutOffMask");


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _inputHandler.ShowMiddleFingerAction.performed += On_MiddleFingerPressed;
        _inputHandler.ShowMiddleFingerAction.canceled += On_MiddleFingerReleased;
        _inputHandler.ShowPointingFingerAction.performed += On_PointingFingerPressed;
        _inputHandler.ShowPointingFingerAction.canceled += On_PointingFingerReleased;

        _maskStateManager.OnStateChanged += On_MaskStateChanged;
    }
    public void On_MaskAnimationFinished()
    {
        _maskStateManager.ConfirmStateTransition();
    }

    private void On_MaskStateChanged(MaskStateType state)
    {
        switch (state)
        {
            case MaskStateType.NotWearing:
                _animator.SetTrigger(PutOffMaskHash);
                break;
            case MaskStateType.Wearing:
                _animator.SetTrigger(PutOnMaskHash);
                break;
        }
    }

    private void OnDestroy()
    {
        _inputHandler.ShowMiddleFingerAction.performed -= On_MiddleFingerPressed;
        _inputHandler.ShowMiddleFingerAction.canceled -= On_MiddleFingerReleased;
        _inputHandler.ShowPointingFingerAction.performed -= On_PointingFingerPressed;
        _inputHandler.ShowPointingFingerAction.canceled -= On_PointingFingerReleased;

        _maskStateManager.OnStateChanged -= On_MaskStateChanged;
    }

    private void Update()
    {
        _animator.SetFloat(SpeedHash, _inputHandler.MoveInput.magnitude);
    }

    private void On_MiddleFingerPressed(InputAction.CallbackContext ctx)
    {
        if (!_holdingMiddleFingerLastFrame)
        {
            _animator.SetTrigger(DoMiddleFingerHash);
        }

        _animator.SetBool(IsHoldingMiddleFingerHash, true);
        _holdingMiddleFingerLastFrame = true;
    }

    private void On_MiddleFingerReleased(InputAction.CallbackContext ctx)
    {
        _animator.SetBool(IsHoldingMiddleFingerHash, false);
        _holdingMiddleFingerLastFrame = false;
    }

    private void On_PointingFingerPressed(InputAction.CallbackContext ctx)
    {
        if (!_holdingPointingFingerLastFrame)
        {
            _animator.SetTrigger(DoPointingFingerHash);
        }

        _animator.SetBool(IsHoldingPointingFingerHash, true);
        _holdingPointingFingerLastFrame = true;
    }

    private void On_PointingFingerReleased(InputAction.CallbackContext ctx)
    {
        _animator.SetBool(IsHoldingPointingFingerHash, false);
        _holdingPointingFingerLastFrame = false;
    }
}