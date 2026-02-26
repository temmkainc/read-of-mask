using UnityEngine;
using UnityEngine.InputSystem;

public class InputsManager : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool ShowMiddleFingerPressed { get; private set; }
    public bool ShowPointingFingerPressed { get; private set; }
    public bool SprintHeld { get; private set; }

    public InputAction ShowMiddleFingerAction => _playerInputActions.Player.ShowMiddleFinger;
    public InputAction ShowPointingFingerAction => _playerInputActions.Player.ShowPointingFinger;
    public InputAction ToggleMaskAction => _playerInputActions.Player.ToggleMask;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
    }

    private void OnEnable() => _playerInputActions.Enable();
    private void OnDisable() => _playerInputActions.Disable();

    private void Update()
    {
        MoveInput = _playerInputActions.Player.Move.ReadValue<Vector2>();
        LookInput = _playerInputActions.Player.Look.ReadValue<Vector2>();
        JumpPressed = _playerInputActions.Player.Jump.triggered;
        SprintHeld = _playerInputActions.Player.Sprint.IsPressed();
        ShowMiddleFingerPressed = _playerInputActions.Player.ShowMiddleFinger.IsPressed();
        ShowPointingFingerPressed = _playerInputActions.Player.ShowPointingFinger.IsPressed();
    }
}