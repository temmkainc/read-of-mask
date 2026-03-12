using UnityEngine;

public class PongPlayerPaddle : MonoBehaviour
{
    [SerializeField] private float _speed = 6f;

    private float _moveInput;
    private float _minY;
    private float _maxY;

    private float _initialLocalY;
    private Collider2D _collider;

    public float Y => transform.localPosition.y; 

    private void Awake()
    {
        _initialLocalY = transform.localPosition.y;
        _collider = GetComponent<Collider2D>();
    }

    public void SetInput(float input) => _moveInput = input;

    public void Initialize(float minOffset, float maxOffset)
    {
        _minY = _initialLocalY + minOffset;
        _maxY = _initialLocalY + maxOffset;
    }

    public void Move(float deltaTime)
    {
        if (_moveInput == 0f) return;

        Vector3 pos = transform.localPosition;
        pos.y += _moveInput * _speed * deltaTime;
        pos.y = Mathf.Clamp(pos.y, _minY, _maxY);

        transform.localPosition = pos;
    }
    public float GetHalfHeight()
    {
        return _collider.bounds.extents.y;
    }
}