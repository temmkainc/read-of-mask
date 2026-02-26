using UnityEngine;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions _controls;

    private void Awake()
    {
        _controls = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return _controls.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return _controls.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerJumpedThisFrame()
    {
        return _controls.Player.Jump.triggered;
    }
}
