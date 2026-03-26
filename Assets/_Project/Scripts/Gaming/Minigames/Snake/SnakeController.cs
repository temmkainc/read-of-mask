using System;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _bodyPrefab;
    [SerializeField] private float _moveInterval = 0.2f;

    private List<Vector2Int> _bodyPositions = new();
    private List<Transform> _bodyParts = new();

    private Vector2Int _direction = Vector2Int.right;
    private Vector2Int _headGridPos;

    private float _timer;

    private SnakeGrid _grid;
    private SnakeScoreSystem _score;

    private Vector2Int _lastTailPosition;

    public event Action<Vector2Int> OnMoved;

    public void Initialize(SnakeGrid grid, SnakeScoreSystem score)
    {
        _grid = grid;
        _score = score;
    }

    public void SetDirection(Vector2 dir)
    {
        Vector2Int newDir = new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));

        if (newDir == -_direction) return;
        _direction = newDir;
    }

    public void Tick(float deltaTime)
    {
        _timer += deltaTime;

        if (_timer < _moveInterval)
            return;

        _timer = 0f;

        Move();
    }

    private void Move()
    {
        Vector2Int previousHeadPos = _headGridPos;

        _lastTailPosition = _bodyPositions.Count > 0
            ? _bodyPositions[^1]
            : previousHeadPos;

        _headGridPos += _direction;

        for (int i = _bodyPositions.Count - 1; i > 0; i--)
        {
            _bodyPositions[i] = _bodyPositions[i - 1];
        }

        if (_bodyPositions.Count > 0)
            _bodyPositions[0] = previousHeadPos;

        UpdateVisuals();

        OnMoved?.Invoke(_headGridPos);
    }

    private void UpdateVisuals()
    {
        _head.localPosition = _grid.GridToLocal(_headGridPos);

        for (int i = 0; i < _bodyParts.Count; i++)
        {
            _bodyParts[i].localPosition = _grid.GridToLocal(_bodyPositions[i]);
        }
    }

    public void Grow()
    {
        _bodyPositions.Add(_lastTailPosition);

        Transform part = Instantiate(_bodyPrefab, transform);
        part.localPosition = _grid.GridToLocal(_lastTailPosition);
        _bodyParts.Add(part);
    }

    public bool CheckSelfCollision()
    {
        for (int i = 0; i < _bodyPositions.Count; i++)
        {
            if (_bodyPositions[i] == _headGridPos)
                return true;
        }
        return false;
    }

    public Vector2Int GetHeadPosition() => _headGridPos;

    public void ResetState()
    {
        foreach (var part in _bodyParts)
            Destroy(part.gameObject);

        _bodyParts.Clear();
        _bodyPositions.Clear();

        _headGridPos = Vector2Int.zero;
        _direction = Vector2Int.right;

        UpdateVisuals();
    }
}