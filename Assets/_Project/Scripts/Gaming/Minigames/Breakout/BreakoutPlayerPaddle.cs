using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BreakoutPlayerPaddle : MonoBehaviour
{
    [SerializeField] private float _speed = 6f;

    private float _moveInput;
    private float _minX;
    private float _maxX;

    private float _initialLocalX;
    private Collider2D _collider;

    private void Awake()
    {
        _initialLocalX = transform.localPosition.x;
        _collider = GetComponent<Collider2D>();
    }

    public void Reset()
    {
        Vector3 pos = transform.localPosition;
        pos.x = _initialLocalX;
        transform.localPosition = pos;
    }

    public void SetInput(float input) => _moveInput = input;

    public void Initialize(float minOffset, float maxOffset)
    {
        _minX = _initialLocalX + minOffset;
        _maxX = _initialLocalX + maxOffset;
    }

    public void Move(float deltaTime)
    {
        if (_moveInput == 0f) return;

        Vector3 pos = transform.localPosition;
        pos.x += _moveInput * _speed * deltaTime;
        pos.x = Mathf.Clamp(pos.x, _minX, _maxX);

        transform.localPosition = pos;
    }
    public float GetHalfWidth()
    {
        return _collider.bounds.extents.x;
    }
}
