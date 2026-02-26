using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputSystem_Actions _input;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool ShowMiddleFingerPressed { get; private set; }
    public bool ShowPointingFingerPressed { get; private set; }
    public bool SprintHeld { get; private set; }

    public InputAction ShowMiddleFingerAction => _input.Player.ShowMiddleFinger;
    public InputAction ShowPointingFingerAction => _input.Player.ShowPointingFinger;

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void Update()
    {
        MoveInput = _input.Player.Move.ReadValue<Vector2>();
        LookInput = _input.Player.Look.ReadValue<Vector2>();
        JumpPressed = _input.Player.Jump.triggered;
        SprintHeld = _input.Player.Sprint.IsPressed();
        ShowMiddleFingerPressed = _input.Player.ShowMiddleFinger.IsPressed();
        ShowPointingFingerPressed = _input.Player.ShowPointingFinger.IsPressed();
    }
}