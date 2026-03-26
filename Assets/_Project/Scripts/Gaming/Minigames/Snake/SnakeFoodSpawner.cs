using System;
using UnityEngine;

public class SnakeFoodSpawner : MonoBehaviour
{
    public event Action OnFoodEat;
    [SerializeField] private Transform _foodPrefab;

    private Transform _currentFood;
    private Vector2Int _foodGridPos;

    private SnakeGrid _grid;
    private SnakeController _snake;

    public void Initialize(SnakeGrid grid, SnakeController snake)
    {
        _grid = grid;
        _snake = snake;
    }

    public void SpawnFood()
    {
        if (_currentFood != null)
            Destroy(_currentFood.gameObject);

        _foodGridPos = _grid.GetRandomPosition();

        _currentFood = Instantiate(_foodPrefab, _grid.transform);
        _currentFood.localPosition = _grid.GridToLocal(_foodGridPos);
    }

    public void CheckFood(Vector2Int headPos)
    {
        if (headPos == _foodGridPos)
        {
            _snake.Grow();
            SpawnFood();
            OnFoodEat?.Invoke();
        }
    }
}