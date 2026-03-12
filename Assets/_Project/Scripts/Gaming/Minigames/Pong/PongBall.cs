using UnityEngine;

public class PongBall : MonoBehaviour
{
    [SerializeField] private float _initialSpeed = 15f;
    [SerializeField] private float _speedIncrease = 2f;
    [SerializeField] private float _maxSpeed = 30f;

    private float _currentSpeed;
    private Vector2 _initialPosition;
    private Rigidbody2D _rigidbody2D;

    private Vector2 _storedVelocity;
    private bool _isPaused = false;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _initialPosition = transform.localPosition;
        _currentSpeed = _initialSpeed;
    }

    public void Pause()
    {
        if (_isPaused) return;
        _storedVelocity = _rigidbody2D.linearVelocity;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _isPaused = true;
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _rigidbody2D.linearVelocity = _storedVelocity;
        _isPaused = false;
    }

    public void StartRound()
    {
        transform.localPosition = _initialPosition;
        _currentSpeed = _initialSpeed;

        bool isRight = Random.value >= 0.5f;
        float x = isRight ? 1f : -1f;
        float y = Random.Range(-0.5f, 0.5f);

        Vector2 dir = new Vector2(x, y).normalized;
        _rigidbody2D.linearVelocity = dir * _currentSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PongPlayerPaddle paddle = collision.collider.GetComponent<PongPlayerPaddle>();
        if (paddle == null) return;

        _currentSpeed = Mathf.Min(_currentSpeed + _speedIncrease, _maxSpeed);

        float dirX = Mathf.Sign(_rigidbody2D.linearVelocity.x);

        float currentY = _rigidbody2D.linearVelocity.y / _rigidbody2D.linearVelocity.magnitude;
        float deltaY = Random.Range(-0.25f, 0.25f);
        float newY = Mathf.Clamp(currentY + deltaY, -1f, 1f);

        Vector2 dir = new Vector2(dirX, newY).normalized;
        _rigidbody2D.linearVelocity = dir * _currentSpeed;
    }
}