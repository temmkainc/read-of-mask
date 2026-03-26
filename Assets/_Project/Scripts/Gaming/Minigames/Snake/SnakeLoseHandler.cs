using System;
using UnityEngine;

public class SnakeLoseHandler : MonoBehaviour
{
    public event Action OnLose;

    private Vector2 _gridSize;
    private SnakeController _snake;

    public void Initialize(Vector2 gridSize, SnakeController snake)
    {
        _gridSize = gridSize;
        _snake = snake;
    }

    public void CheckLose()
    {
        Vector2Int pos = _snake.GetHeadPosition();

        if (pos.x < 0 || pos.x >= _gridSize.x ||
            pos.y < 0 || pos.y >= _gridSize.y)
        {
            OnLose?.Invoke();
            return;
        }

        if (_snake.CheckSelfCollision())
        {
            OnLose?.Invoke();
        }
    }
}