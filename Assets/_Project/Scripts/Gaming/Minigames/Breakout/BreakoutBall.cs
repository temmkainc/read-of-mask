using UnityEngine;
using UnityGLTF.Interactivity.Schema;

public class BreakoutBall : MonoBehaviour
{
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _speedGrowthStep = 0.5f;
    [SerializeField] private float _maxSpeed = 35f;
    [SerializeField] private float _maxBounceAngle = 75f;

    private float _currentSpeed;

    private Rigidbody2D _rb;
    private Vector2 _storedVelocity;
    private bool _isPaused;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 dir)
    {
        _currentSpeed = _speed;
        _rb.linearVelocity = dir.normalized * _currentSpeed;
    }
    public void Pause()
    {
        if (_isPaused) return;

        _storedVelocity = _rb.linearVelocity;
        _rb.linearVelocity = Vector2.zero;

        _isPaused = true;
    }

    public void Resume()
    {
        if (!_isPaused) return;

        _rb.linearVelocity = _storedVelocity;
        _isPaused = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BreakoutPlayerPaddle paddle = collision.collider.GetComponent<BreakoutPlayerPaddle>();
        if (paddle == null)
            return;

        _currentSpeed = Mathf.Min(_currentSpeed + _speedGrowthStep, _maxSpeed);
        ReflectFromPaddle(paddle);
    }

    private void ReflectFromPaddle(BreakoutPlayerPaddle paddle)
    {
        float paddleCenter = paddle.transform.position.x;
        float hitPoint = transform.position.x;

        float paddleHalfWidth = paddle.GetHalfWidth();

        float offset = hitPoint - paddleCenter;
        float normalized = offset / paddleHalfWidth;

        normalized = Mathf.Clamp(normalized, -1f, 1f);

        float angle = normalized * _maxBounceAngle;

        float rad = angle * Mathf.Deg2Rad;

        Vector2 dir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));

        _rb.linearVelocity = dir.normalized * _currentSpeed;
    }
}