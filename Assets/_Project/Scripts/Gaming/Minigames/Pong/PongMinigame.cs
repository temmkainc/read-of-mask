using System;
using UnityEngine;

public class PongMinigame : MinigameBase
{
    [SerializeField] private PongPlayerPaddle _playerPaddle;
    [SerializeField] private PongPlayerPaddle _computerPaddle;
    [SerializeField] private float _minY = -10f;
    [SerializeField] private float _maxY = 10f;

    [SerializeField] private PongBall _ball;
    [SerializeField] private PongGoal _leftGoal;
    [SerializeField] private PongGoal _rightGoal;
    [SerializeField] private PongScoreUI _scoreUI;

    [SerializeField] private PongInputHandler _inputHandler;

    private PongScoreSystem _scoreSystem;

    private const int GOALS_TO_WIN = 5;

    public override void Initialize()
    {
        base.Initialize();
        _inputHandler.Initialize(_inputManager.GamingDirectionAction, _playerPaddle);
        _playerPaddle.Initialize(_minY, _maxY);
        _computerPaddle.Initialize(_minY, _maxY);
        _scoreSystem = new PongScoreSystem(GOALS_TO_WIN);
        _scoreUI.Initialize(_scoreSystem);

        _leftGoal.OnGoal += On_PlayerScore;
        _rightGoal.OnGoal += On_ComputerScore;

        _scoreSystem.OnGameFinished += On_GameFinished;
    }

    private void On_PlayerScore()
    {
        _scoreSystem.PlayerScored();
        _ball.StartRound();
    }
    private void On_ComputerScore()
    {
        _scoreSystem.ComputerScored();
        _ball.StartRound();
    }

    private void On_GameFinished(bool playerWon)
    {
        StartGame();
    }

    protected override void OnMenuButtonSelected(int index)
    {
        switch (index)
        {
            case 0: StartGame(); break;
            case 1: ExitGameInternally(); break;
        }
    }

    protected override void OnPauseMenuButtonSelected(int index)
    {
        switch (index)
        {
            case 0: ResumeGame(); break;
            case 1: ExitToMenu(); break;
        }
    }

    public override void StartGame()
    {
        _scoreSystem.Reset();
        base.StartGame();
        ResumeGame();
        _ball.StartRound();
    }

    public override void PauseGame()
    {
        base.PauseGame();
        _ball.Pause();
    }

    public override void ResumeGame()
    {
        _ball.Resume();
        base.ResumeGame();
    }

    private void Update()
    {
        if (_isPaused)
            return;

        var dt = Time.deltaTime;
        _playerPaddle.Move(dt);

        float targetY = _ball.transform.localPosition.y;
        float diff = targetY - _computerPaddle.Y;

        float reactionThreshold = 1.3f;

        if (Mathf.Abs(diff) < reactionThreshold)
        {
            _computerPaddle.SetInput(0);
        }
        else
        {
            float input = Mathf.Clamp(diff * 0.3f, -1f, 1f);
            _computerPaddle.SetInput(input);
        }

        _computerPaddle.Move(dt);
    }
}