using UnityEngine;

public class BreakoutBrickGrid : MonoBehaviour
{
    [SerializeField] private BreakoutBrick _brickPrefab;
    [SerializeField] private int _rows = 4;
    [SerializeField] private int _columns = 8;
    [SerializeField] private float _offsetX = 4.5f;
    [SerializeField] private float _offsetY = 4.5f;

    [SerializeField] private int _currentBrickCount;
    
    private BreakoutScoreSystem _scoreSystem;
    private BreakoutBallSpawner _ballSpawner;

    private int _totalBrickCount;


    private void Awake()
    {
        _totalBrickCount = _rows * _columns;
    }

    public void Initialize(BreakoutScoreSystem scoreSystem, BreakoutBallSpawner ballSpawner)
    {
        _scoreSystem = scoreSystem;
        _ballSpawner = ballSpawner;
    }

    public void ResetGrid()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                BreakoutBrick brick = Instantiate(_brickPrefab, transform);
                brick.transform.localPosition = new Vector3(x * _offsetX, y * -_offsetY, 0);
                brick.OnDestroyed += On_BrickDestroyed;
            }
        }

        _currentBrickCount = _totalBrickCount;
    }

    private void On_BrickDestroyed()
    {
        _currentBrickCount--;
        _scoreSystem.AddScore();
        if (_currentBrickCount > 0)
            return;

        _ballSpawner.StartRound(resetSpeed: false);
        ResetGrid();
    }
}