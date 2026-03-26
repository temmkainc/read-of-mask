using UnityEngine;
using Zenject;

public class SnakeMinigame : MinigameBase
{
    [SerializeField] private SnakeController _snake;
    [SerializeField] private SnakeGrid _grid;
    [SerializeField] private SnakeFoodSpawner _foodSpawner;
    [SerializeField] private SnakeScoreUI _scoreUI;

    private SnakeInputHandler _inputHandler;
    private SnakeScoreSystem _scoreSystem;

    public override void Initialize()
    {
        base.Initialize();

        _scoreSystem = new SnakeScoreSystem();

        _snake.Initialize(_grid, _scoreSystem);


        _inputHandler = new SnakeInputHandler();
        _inputHandler.Initialize(_inputManager.GamingDirectionAction, _snake);

        _foodSpawner.Initialize(_grid, _snake);
        _foodSpawner.OnFoodEat += () =>
        {
            _scoreSystem.AddScore();
            _scoreUI.UpdateUI();   
        };
        _snake.OnMoved += _foodSpawner.CheckFood;

        _scoreUI.Initialize(_scoreSystem);

        _grid.GenerateWalls();
    }

    private void Update()
    {
        if (_isPaused)
            return;

        _snake.Tick(Time.deltaTime);
        if (!_grid.IsInsideGrid(_snake.GetHeadPosition()) || _snake.CheckSelfCollision())
        {
            StartGame();
        }
    }

    public override void StartGame()
    {
        base.StartGame();

        _scoreSystem.Reset();
        _scoreUI.UpdateUI();
        _snake.ResetState();
        _foodSpawner.SpawnFood();
    }

    public override void PauseGame()
    {
        base.PauseGame();
    }

    public override void ResumeGame()
    {
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