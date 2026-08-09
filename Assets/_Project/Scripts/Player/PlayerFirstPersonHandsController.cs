using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using System;

public class PlayerFirstPersonHandsController : MonoBehaviour
{
    [Inject] private InputManager _inputManager;

    [Inject] IMaskStateManager _maskStateManager;
    [Inject] IPlayerStateManager _playerStateManager;
    [Inject] PlayerController _playerController;

    [SerializeField] private Image _handsImage;

    private Animator _animator;
    private PlayerStateType _previousPlayerStateType;

    private Action _catPatFinishedCallback;

    private bool _holdingMiddleFingerLastFrame = false;
    private bool _holdingPointingFingerLastFrame = false;
    private bool _isLookCloserActive = false;

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

    private readonly int DoAngryHash = Animator.StringToHash("DoAngry");
    private readonly int DoNotAllowedHash = Animator.StringToHash("DoNotAllowed");
    private readonly int DoCatPatHash = Animator.StringToHash("DoCatPat");

    private readonly int MaskIdleHash = Animator.StringToHash("player_in_mask_idle");
    private readonly int MaskPutOnHash = Animator.StringToHash("player_put_mask_on");
    private readonly int MaskPutOffHash = Animator.StringToHash("player_put_mask_off");

    private const int MOVEMENT_LAYER_INDEX = 1;
    private const int GESTURES_LAYER_INDEX = 2;
    private const int MASK_LAYER_INDEX = 3;
    private const int GAMING_LAYER_INDEX = 4;

    private bool _isBlockingAnimationPlaying = false;


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

    private void Update()
    {
        _animator.SetFloat(SpeedHash, _playerController.MoveInput.magnitude);
        if (_isLookCloserActive)
            UpdateLookCloserHandsVisibility();
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

    public void ResetBlockingAnimation()
    {
        _isBlockingAnimationPlaying = false;
    }

    private void On_PlayerStateChanged(PlayerStateType type)
    {
        if (type == PlayerStateType.Gaming)
            On_EnterGamingState();
        else if (_previousPlayerStateType == PlayerStateType.Gaming)
            On_ExitGamingState();

        ResetBlockingAnimation();

        _isLookCloserActive = type == PlayerStateType.LookCloser;

        if (!_isLookCloserActive)
            _handsImage.color = Color.white;
        else
            _handsImage.color = Color.clear;

        _previousPlayerStateType = type;
    }

    private void On_EnterGamingState()
    {
        _animator.SetLayerWeight(MOVEMENT_LAYER_INDEX, 0);
        _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 0);
        _animator.SetLayerWeight(GAMING_LAYER_INDEX, _maskStateManager.CurrentStateType == MaskStateType.NotWearing ? 1 : 0);
        _animator.SetTrigger(DoStartGamingHash);
        _inputManager.GamingDirectionAction.performed += On_GamingDirectionPerformed;
    }

    private void On_ExitGamingState()
    {
        _animator.SetLayerWeight(MOVEMENT_LAYER_INDEX, 1);
        _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 1);
        _animator.SetLayerWeight(GAMING_LAYER_INDEX, 0);
        _animator.Play("Empty State", GAMING_LAYER_INDEX, 0f);
        _animator.ResetTrigger(DoStartGamingHash);
        _animator.ResetTrigger(DoGamingTopButtonHash);
        _animator.ResetTrigger(DoGamingBottomButtonHash);
        _animator.ResetTrigger(DoGamingLeftButtonHash);
        _animator.ResetTrigger(DoGamingRightButtonHash);
        _inputManager.GamingDirectionAction.performed -= On_GamingDirectionPerformed;
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

        if (_maskStateManager.CurrentStateType == MaskStateType.NotWearing)
        {
            if (_playerStateManager.CurrentStateType == PlayerStateType.Gaming)
            {
                _animator.SetLayerWeight(GAMING_LAYER_INDEX, 1f);
                _animator.Play("Empty State", GAMING_LAYER_INDEX, 0f);
                _animator.SetTrigger(DoStartGamingHash);
            }
            else
            {
                _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 1f);
            }
        }
    }

    private void SetBlockingAnimation(int triggerHash)
    {
        if (_isBlockingAnimationPlaying) return;

        _animator.SetLayerWeight(MASK_LAYER_INDEX, 0f);
        _animator.SetLayerWeight(GAMING_LAYER_INDEX, 0f);
        _animator.SetLayerWeight(MOVEMENT_LAYER_INDEX, 0f);
        _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 1f);
        _animator.ResetTrigger(triggerHash);
        _animator.SetTrigger(triggerHash);
        _isBlockingAnimationPlaying = true;
    }
    private void OnBlockingAnimationFinished(int triggerHash)
    {
        if (triggerHash != 0)
            _animator.ResetTrigger(triggerHash);
        _isBlockingAnimationPlaying = false;

        bool isGaming = _playerStateManager.CurrentStateType == PlayerStateType.Gaming;
        bool isMasking = _maskStateManager.CurrentStateType == MaskStateType.Wearing;

        _animator.SetLayerWeight(MOVEMENT_LAYER_INDEX, isGaming ? 0f : 1f);
        _animator.SetLayerWeight(GESTURES_LAYER_INDEX, isGaming ? 0f : 1f);
        _animator.SetLayerWeight(GAMING_LAYER_INDEX, isGaming && !isMasking ? 1f : 0f);
        _animator.SetLayerWeight(MASK_LAYER_INDEX, 0f);
    }

    public void PlayAngryAnimation() => SetBlockingAnimation(DoAngryHash);
    public void On_AngryAnimationFinished() => OnBlockingAnimationFinished(DoAngryHash);
    public void PlayCatPatAnimation(Action onFinished = null)
    {
        SetBlockingAnimation(DoCatPatHash);
        _catPatFinishedCallback = onFinished;
    }
    public void On_CatPatAnimationFinished()
    {
        OnBlockingAnimationFinished(DoCatPatHash);
        _catPatFinishedCallback?.Invoke();
        _catPatFinishedCallback = null;
    }

    public void PlayNotAllowedAnimation() { SetBlockingAnimation(DoNotAllowedHash); AudioManager.Instance.PlaySFX(SfxClips.NotAllowed, volumeScale: 1f); }
    public void On_NotAllowedAnimationFinished() => OnBlockingAnimationFinished(DoNotAllowedHash);

    private void On_MaskStateChanged(MaskStateType state)
    {
        _animator.SetLayerWeight(MASK_LAYER_INDEX, 1f);
        _animator.ResetTrigger(DoMiddleFingerHash);
        _animator.ResetTrigger(DoPointingFingerHash);
        _animator.SetBool(IsHoldingMiddleFingerHash, false);
        _animator.SetBool(IsHoldingPointingFingerHash, false);
        _holdingMiddleFingerLastFrame = false;
        _holdingPointingFingerLastFrame = false;

        switch (state)
        {
            case MaskStateType.Wearing:
                _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 0f);
                if (_playerStateManager.CurrentStateType == PlayerStateType.Gaming)
                {
                    _animator.SetLayerWeight(GAMING_LAYER_INDEX, 0f);
                }
                _animator.SetTrigger(PutOnMaskHash);
                break;

            case MaskStateType.NotWearing:
                _animator.SetLayerWeight(GESTURES_LAYER_INDEX, 0f);
                _animator.SetTrigger(PutOffMaskHash);
                break;
        }
    }


    private void On_MiddleFingerPressed(InputAction.CallbackContext ctx)
    {
        if (_isBlockingAnimationPlaying) return;

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
        if (_isBlockingAnimationPlaying) return;

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

    private void UpdateLookCloserHandsVisibility()
    {
        if (_animator.GetLayerWeight(MASK_LAYER_INDEX) <= 0f)
        {
            _handsImage.color = Color.clear;
            return;
        }

        var stateInfo = _animator.GetCurrentAnimatorStateInfo(MASK_LAYER_INDEX);
        bool shouldShow = stateInfo.shortNameHash == MaskPutOnHash
                       || stateInfo.shortNameHash == MaskIdleHash
                       || stateInfo.shortNameHash == MaskPutOffHash;

        _handsImage.color = shouldShow ? Color.white : Color.clear;
    }

}