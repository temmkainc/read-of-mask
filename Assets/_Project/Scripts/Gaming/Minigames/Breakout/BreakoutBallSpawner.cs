using System.Collections.Generic;
using UnityEngine;

public class BreakoutBallSpawner : MonoBehaviour
{
    [SerializeField] private BreakoutBall _ballPrefab;

    private readonly List<BreakoutBall> _balls = new();

    public void StartRound(bool resetSpeed = true)
    {
        ClearBalls();

        SpawnBall(Vector2.up);
    }

    public void SpawnBall(Vector2 dir)
    {
        var ball = Instantiate(_ballPrefab, transform.position, Quaternion.identity);
        ball.Launch(dir);

        _balls.Add(ball);
    }
    public void PauseBalls()
    {
        foreach (var ball in _balls)
            ball.Pause();
    }

    public void ResumeBalls()
    {
        foreach (var ball in _balls)
            ball.Resume();
    }

    private void ClearBalls()
    {
        foreach (var ball in _balls)
            Destroy(ball.gameObject);

        _balls.Clear();
    }
}