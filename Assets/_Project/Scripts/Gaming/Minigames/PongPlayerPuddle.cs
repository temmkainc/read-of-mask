using UnityEngine;

public class PongPlayerPaddle : MonoBehaviour
{
    [SerializeField] private float _speed = 6f;

    private float _moveInput;
    private float _minY;
    private float _maxY;

    private float _initialLocalY;

    private SpriteRenderer _sr;

    public float Y => transform.localPosition.y; // Restore this for AI

    private void Awake()
    {
        _initialLocalY = transform.localPosition.y;
        _sr = GetComponent<SpriteRenderer>();
    }

    public void SetInput(float input) => _moveInput = input;

    public void SetBounds(float minOffset, float maxOffset)
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

    public Rect GetRect()
    {
        Vector3 pos = transform.localPosition;
        Vector3 size = GetComponent<SpriteRenderer>().size; 
        return new Rect(
            pos.x - size.x / 2f,
            pos.y - size.y / 2f,
            size.x,
            size.y
        );
    }
}