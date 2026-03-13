using System;

public class BreakoutScoreSystem
{
    private int _playerScore;

    private const int SCORE_FOR_BRICK = 100;

    public event Action<int> OnScoreChanged;

    public void Reset()
    {
        _playerScore = 0;
        OnScoreChanged?.Invoke(_playerScore);
    }

    public void AddScore()
    {
        _playerScore += SCORE_FOR_BRICK;
        OnScoreChanged?.Invoke(_playerScore);
    }
}