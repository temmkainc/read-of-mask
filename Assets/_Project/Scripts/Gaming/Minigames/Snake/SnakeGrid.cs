using UnityEngine;

public class SnakeGrid : MonoBehaviour
{
    [SerializeField] private int _width = 20;
    [SerializeField] private int _height = 20;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private Transform _wallPrefab;
    [SerializeField] private Transform _wallParent;

    public void GenerateWalls()
    {
        ClearWalls();

        for (int x = -1; x <= _width; x++)
        {
            SpawnWall(new Vector2Int(x, -1));
            SpawnWall(new Vector2Int(x, _height));
        }

        for (int y = 0; y < _height; y++)
        {
            SpawnWall(new Vector2Int(-1, y));
            SpawnWall(new Vector2Int(_width, y));
        }
    }
    private void SpawnWall(Vector2Int gridPos)
    {
        Transform wall = Instantiate(_wallPrefab, _wallParent);

        wall.localPosition = GridToLocal(gridPos);
    }

    private void ClearWalls()
    {
        if (_wallParent == null) return;

        for (int i = _wallParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_wallParent.GetChild(i).gameObject);
        }
    }

    public Vector2Int GetRandomPosition()
    {
        return new Vector2Int(
            Random.Range(0, _width),
            Random.Range(0, _height)
        );
    }

    public Vector3 GridToLocal(Vector2Int gridPos)
    {
        float offsetX = -(_width / 2f) * _cellSize;
        float offsetY = -(_height / 2f) * _cellSize;

        return new Vector3(
            offsetX + gridPos.x * _cellSize,
            offsetY + gridPos.y * _cellSize,
            0f
        );
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return transform.TransformPoint(GridToLocal(gridPos));
    }

    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _width &&
               pos.y >= 0 && pos.y < _height;
    }
}