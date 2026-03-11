using UnityEngine;
using UnityEngine.InputSystem;

public class PongInputHandler : MonoBehaviour
{
    private InputAction _moveInputAction;
    private PongPlayerPaddle _paddle;

    public void Initialize(InputAction moveInput, PongPlayerPaddle paddle)
    {
        _paddle = paddle;
        _moveInputAction = moveInput;
        _moveInputAction.performed += OnMovePerformed;
        _moveInputAction.canceled += OnMoveCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        _paddle.SetInput(input.y);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _paddle.SetInput(0f);
    }

    public void Dispose()
    {
        _moveInputAction.performed -= OnMovePerformed;
        _moveInputAction.canceled -= OnMoveCanceled;
    }
}