using Cysharp.Threading.Tasks.Triggers;
using System;
using UnityEngine;

public class BreakoutMinigame : MinigameBase
{
    [SerializeField] private BreakoutPlayerPaddle _playerPaddle;
    [SerializeField] private float _minX = -10f;
    [SerializeField] private float _maxX = 10f;
    [SerializeField] private BreakoutInputHandler _inputHandler;
    [SerializeField] private BreakoutBallSpawner _ballSpawner;
    [SerializeField] private BreakoutBrickGrid _brickGrid;
    [SerializeField] private BreakoutLoseZone _loseZone;
    [SerializeField] private BreakoutScoreUI _scoreUI;

    private BreakoutScoreSystem _scoreSystem;

    public override void Initialize()
    {
        base.Initialize();
        _playerPaddle.Initialize(_minX, _maxX);
        _inputHandler.Initialize(_inputManager.GamingDirectionAction, _playerPaddle);
        _scoreSystem = new BreakoutScoreSystem();
        _brickGrid.Initialize(_scoreSystem, _ballSpawner);
        _loseZone.OnLose += On_GameFinished;
        _scoreUI.Initialize(_scoreSystem);
    }

    private void On_GameFinished()
    {
        StartGame();
    }

    private void Update()
    {
        if (_isPaused)
            return;

        _playerPaddle.Move(Time.deltaTime);
    }

    public override void StartGame()
    {
        base.StartGame();

        _playerPaddle.Reset();
        _brickGrid.ResetGrid();
        _scoreSystem.Reset();
        _ballSpawner.StartRound();
    }

    public override void PauseGame()
    {
        base.PauseGame();
        _ballSpawner.PauseBalls();
    }

    public override void ResumeGame()
    {
        _ballSpawner.ResumeBalls();
        base.ResumeGame();
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
}