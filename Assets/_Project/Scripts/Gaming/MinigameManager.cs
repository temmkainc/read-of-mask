using System;
using Zenject;

public enum MinigameType
{
    Pong,
    Snake,
    Breakout,
    Tetris,
    Pacman
}

public class MinigameManager : IInitializable
{
    private readonly PongMinigame _pongGame;
    private readonly BreakoutMinigame _breakoutMinigame;
    private readonly SnakeMinigame _snakeMinigame;

    public MinigameBase CurrentGame { get; private set; }
    public event Action OnMinigameExitedInternally;

    public MinigameManager(GamingModule.ConfigData config)
    {
        _pongGame = config.PongMinigame;
        _breakoutMinigame = config.BreakoutMinigame;
        _snakeMinigame = config.SnakeMinigame;
    }

    public void Initialize()
    {
        _pongGame.Initialize();
        _breakoutMinigame.Initialize();
        _snakeMinigame.Initialize();
    }

    public void EnterMinigame(MinigameType type)
    {
        ExitCurrentMinigame();

        CurrentGame = GetGame(type);
        CurrentGame.EnterGame();
        CurrentGame.OnMinigameExitedInternally += On_MinigameExitedInternally;
    }

    public void ExitCurrentMinigame()
    {
        if (CurrentGame == null)
            return;

        CurrentGame.ExitGame();
        CurrentGame = null;
    }

    private void On_MinigameExitedInternally()
    {
        CurrentGame = null;
        OnMinigameExitedInternally?.Invoke();
    }

    private MinigameBase GetGame(MinigameType type)
    {
        return type switch
        {
            MinigameType.Pong => _pongGame,
            MinigameType.Breakout => _breakoutMinigame,
            MinigameType.Snake => _snakeMinigame,
            _ => null
        };
    }

}