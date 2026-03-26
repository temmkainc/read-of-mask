using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeInputHandler
{
    private InputAction _input;
    private SnakeController _snake;

    public void Initialize(InputAction input, SnakeController snake)
    {
        _input = input;
        _snake = snake;

        _input.performed += OnInput;
    }

    private void OnInput(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();

        if (dir.x > 0.5f) _snake.SetDirection(Vector2.right);
        else if (dir.x < -0.5f) _snake.SetDirection(Vector2.left);
        else if (dir.y > 0.5f) _snake.SetDirection(Vector2.up);
        else if (dir.y < -0.5f) _snake.SetDirection(Vector2.down);
    }

    public void Dispose()
    {
        _input.performed -= OnInput;
    }
}