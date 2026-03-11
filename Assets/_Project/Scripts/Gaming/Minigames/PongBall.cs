using UnityEngine;

public class PongBall : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;

    private Vector2 _direction;

    private float _topY;
    private float _bottomY;

    public System.Action<int> OnScore; // 1 = Computer, -1 = Player

    public void SetBoundaries(float topY, float bottomY)
    {
        _topY = topY;
        _bottomY = bottomY;
    }

    public void ResetBall()
    {
        transform.localPosition = Vector3.zero;

        float xDir = Random.value < 0.5f ? -1f : 1f;
        float yDir = Random.Range(-0.5f, 0.5f);
        _direction = new Vector2(xDir, yDir).normalized;
    }

    public void Move(float deltaTime)
    {
        // Move
        Vector3 pos = transform.localPosition;
        pos += (Vector3)(_direction * _speed * deltaTime);

        // Top/Bottom bounce
        if (pos.y >= _topY)
        {
            pos.y = _topY;
            _direction.y = -_direction.y;
        }
        else if (pos.y <= _bottomY)
        {
            pos.y = _bottomY;
            _direction.y = -_direction.y;
        }

        transform.localPosition = pos;
    }

    // Manual collision with paddle rectangle
    public void CheckPaddleCollision(PongPlayerPaddle paddle)
    {
        Rect paddleRect = paddle.GetRect();
        Vector2 ballPos = transform.localPosition;

        if (paddleRect.Contains(ballPos))
        {
            // hit position relative to paddle center (-1..1)
            float hitY = (ballPos.y - paddleRect.center.y) / (paddleRect.height / 2f);
            hitY = Mathf.Clamp(hitY, -1f, 1f);

            // keep X speed consistent
            float directionX = _direction.x > 0 ? 1f : -1f; // going right? left?
            _direction = new Vector2(-directionX, hitY).normalized;

            // push ball out of paddle to prevent sticking
            if (_direction.x > 0)
                transform.localPosition = new Vector3(paddleRect.xMax + 0.01f, ballPos.y, 0f);
            else
                transform.localPosition = new Vector3(paddleRect.xMin - 0.01f, ballPos.y, 0f);
        }
    }

    public void CheckScore(float leftX, float rightX)
    {
        float x = transform.localPosition.x;
        if (x < leftX)
        {
            OnScore?.Invoke(1); // Computer
            ResetBall();
        }
        else if (x > rightX)
        {
            OnScore?.Invoke(-1); // Player
            ResetBall();
        }
    }
}