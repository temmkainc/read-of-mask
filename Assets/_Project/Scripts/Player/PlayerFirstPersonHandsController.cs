using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class FirstPersonHandsController : MonoBehaviour
{
    [Inject] private InputManager _inputManager;

    [Inject] IMaskStateManager _maskStateManager;
    [Inject] IPlayerStateManager _playerStateManager;

    private Animator _animator;
    private PlayerStateType _previousPlayerStateType;

    private bool _holdingMiddleFingerLastFrame = false;
    private bool _holdingPointingFingerLastFrame = false;

    private readonly int SpeedHash = Animator.StringToHash("Speed");
    private readonly int DoMiddleFingerHash = Animator.StringToHash("DoMiddleFinger");
    private readonly int IsHoldingMiddleFingerHash = Animator.StringToHash("IsHoldingMiddleFinger");
    private readonly int DoPointingFingerHash = Animator.StringToHash("DoPointingFinger");
    private readonly int IsHoldingPointingFingerHash = Animator.StringToHash("IsHoldingPointingFinger");

    private readonly int PutOnMaskHash = Animator.StringToHash("DoPutOnMask");
    private readonly int PutOffMaskHash = Animator.StringToHash("DoPutOffMask");

    private readonly int DoStartGamingHash = Animator.StringToHash("DoStartGaming");
    private readonly int DoGamingTopButtonHash = Animator.StringToHash("DoGamingTopButton");
    private readonly int DoGamingBottomButtonHash = Animator.StringToHash("DoGamingBottomButton");
    private readonly int DoGamingLeftButtonHash = Animator.StringToHash("DoGamingLeftButton");
    private readonly int DoGamingRightButtonHash = Animator.StringToHash("DoGamingRightButton");

    private const int MOVEMENT_LAYER_INDEX = 1;
    private const int GESTURES_LAYER_INDEX = 2;
    private const int MASK_LAYER_INDEX = 3;
    private const int GAMING_LAYER_INDEX = 4;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _inputManager.ShowMiddleFingerAction.performed += On_MiddleFingerPressed;
        _inputManager.ShowMiddleFingerAction.canceled += On_MiddleFingerReleased;
        _inputManager.ShowPointingFingerAction.performed += On_PointingFingerPressed;
        _inputManager.ShowPointingFingerAction.canceled += On_PointingFingerReleased;

        _maskStateManager.OnStateChanged += On_MaskStateChanged;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
    }

    private void OnDestroy()
    {
        _inputManager.ShowMiddleFingerAction.performed -= On_MiddleFingerPressed;
        _inputManager.ShowMiddleFingerAction.canceled -= On_MiddleFingerReleased;
        _inputManager.ShowPointingFingerAction.performed -= On_PointingFingerPressed;
        _inputManager.ShowPointingFingerAction.canceled -= On_PointingFingerReleased;

        _maskStateManager.OnStateChanged -= On_MaskStateChanged;
        _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
    }

    private void On_PlayerStateChanged(PlayerStateType type)
    {
        if (type == PlayerStateType.Interaction)
            On_EnterInteractionState();
        else if (_previousPlayerStateType == PlayerStateType.Interaction)
            On_ExitInteractionState();

        _previousPlayerStateType = type;
    }

    private void On_EnterInteractionState()
    {
        _animator.SetLayerWeight(MOVEMENT_LAYER_INDEX, 0);
        _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 0);
        _animator.SetLayerWeight(MASK_LAYER_INDEX, 0);
        _animator.SetLayerWeight(GAMING_LAYER_INDEX, 1);
        _animator.SetTrigger(DoStartGamingHash);
        _inputManager.InteractionDirectionAction.performed += On_GamingDirectionPerformed;
    }

    private void On_ExitInteractionState()
    {
        _animator.SetLayerWeight(MOVEMENT_LAYER_INDEX, 1);
        _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 1);
        _animator.SetLayerWeight(MASK_LAYER_INDEX, 1);
        _animator.SetLayerWeight(GAMING_LAYER_INDEX, 0);
        _animator.Play("Empty State", GAMING_LAYER_INDEX, 0f);
        _animator.ResetTrigger(DoStartGamingHash);
        _animator.ResetTrigger(DoGamingTopButtonHash);
        _animator.ResetTrigger(DoGamingBottomButtonHash);
        _animator.ResetTrigger(DoGamingLeftButtonHash);
        _animator.ResetTrigger(DoGamingRightButtonHash);
        _inputManager.InteractionDirectionAction.performed -= On_GamingDirectionPerformed;
    }

    private void On_GamingDirectionPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 direction = ctx.ReadValue<Vector2>();

        if (direction.y > 0) _animator.SetTrigger(DoGamingTopButtonHash);
        else if (direction.y < 0) _animator.SetTrigger(DoGamingBottomButtonHash);
        else if (direction.x < 0) _animator.SetTrigger(DoGamingLeftButtonHash);
        else if (direction.x > 0) _animator.SetTrigger(DoGamingRightButtonHash);
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

    private void Update()
    {
        _animator.SetFloat(SpeedHash, _inputManager.MoveInput.magnitude);
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