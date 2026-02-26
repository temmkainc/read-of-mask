using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonHandsController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler _inputHandler;

    private Animator _animator;

    private bool _holdingMiddleFingerLastFrame = false;
    private bool _holdingPointingFingerLastFrame = false;

    private readonly int SpeedHash = Animator.StringToHash("Speed");
    private readonly int DoMiddleFingerHash = Animator.StringToHash("DoMiddleFinger");
    private readonly int IsHoldingMiddleFingerHash = Animator.StringToHash("IsHoldingMiddleFinger");
    private readonly int DoPointingFingerHash = Animator.StringToHash("DoPointingFinger");
    private readonly int IsHoldingPointingFingerHash = Animator.StringToHash("IsHoldingPointingFinger");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _inputHandler.ShowMiddleFingerAction.performed += OnMiddleFingerPressed;
        _inputHandler.ShowMiddleFingerAction.canceled += OnMiddleFingerReleased;
        _inputHandler.ShowPointingFingerAction.performed += OnPointingFingerPressed;
        _inputHandler.ShowPointingFingerAction.canceled += OnPointingFingerReleased;
    }

    private void OnDestroy()
    {
        _inputHandler.ShowMiddleFingerAction.performed -= OnMiddleFingerPressed;
        _inputHandler.ShowMiddleFingerAction.canceled -= OnMiddleFingerReleased;
        _inputHandler.ShowPointingFingerAction.performed -= OnPointingFingerPressed;
        _inputHandler.ShowPointingFingerAction.canceled -= OnPointingFingerReleased;
    }

    private void Update()
    {
        _animator.SetFloat(SpeedHash, _inputHandler.MoveInput.magnitude);
    }

    private void OnMiddleFingerPressed(InputAction.CallbackContext ctx)
    {
        if (!_holdingMiddleFingerLastFrame)
        {
            _animator.SetTrigger(DoMiddleFingerHash);
        }

        _animator.SetBool(IsHoldingMiddleFingerHash, true);
        _holdingMiddleFingerLastFrame = true;
    }

    private void OnMiddleFingerReleased(InputAction.CallbackContext ctx)
    {
        _animator.SetBool(IsHoldingMiddleFingerHash, false);
        _holdingMiddleFingerLastFrame = false;
    }

    private void OnPointingFingerPressed(InputAction.CallbackContext ctx)
    {
        if (!_holdingPointingFingerLastFrame)
        {
            _animator.SetTrigger(DoPointingFingerHash);
        }

        _animator.SetBool(IsHoldingPointingFingerHash, true);
        _holdingPointingFingerLastFrame = true;
    }

    private void OnPointingFingerReleased(InputAction.CallbackContext ctx)
    {
        _animator.SetBool(IsHoldingPointingFingerHash, false);
        _holdingPointingFingerLastFrame = false;
    }
}