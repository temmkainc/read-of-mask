using System;

public class PongScoreSystem
{
    private int _playerScore;
    private int _computerScore;

    private readonly int _scoreToWin;

    public event Action<int, int> OnScoreChanged;
    public event Action<bool> OnGameFinished;

    public PongScoreSystem(int scoreToWin)
    {
        _scoreToWin = scoreToWin;
    }

    public void Reset()
    {
        _playerScore = 0;
        _computerScore = 0;
        OnScoreChanged?.Invoke(_playerScore, _computerScore);
    }

    public void PlayerScored()
    {
        _playerScore++;
        OnScoreChanged?.Invoke(_playerScore, _computerScore);

        if (_playerScore >= _scoreToWin)
            OnGameFinished?.Invoke(true);
    }

    public void ComputerScored()
    {
        _computerScore++;
        OnScoreChanged?.Invoke(_playerScore, _computerScore);

        if (_computerScore >= _scoreToWin)
            OnGameFinished?.Invoke(false);
    }
}