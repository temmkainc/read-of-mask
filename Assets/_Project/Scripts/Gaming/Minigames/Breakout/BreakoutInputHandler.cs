using UnityEngine;
using UnityEngine.InputSystem;

public class BreakoutInputHandler : MonoBehaviour
{
    private InputAction _moveInputAction;
    private BreakoutPlayerPaddle _paddle;

    public void Initialize(InputAction moveInput, BreakoutPlayerPaddle paddle)
    {
        _paddle = paddle;
        _moveInputAction = moveInput;
        _moveInputAction.performed += OnMovePerformed;
        _moveInputAction.canceled += OnMoveCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        _paddle.SetInput(input.x);
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
