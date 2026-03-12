using System;
using Zenject;

public enum MinigameType
{
    Pong,
    Snake,
    Brekout,
    Tetris,
    Pacman
}

public class MinigameManager : IInitializable
{
    private readonly PongMinigame _pongGame;

    public MinigameBase CurrentGame { get; private set; }
    public event Action OnMinigameExitedInternally;

    public MinigameManager(GamingModule.ConfigData config)
    {
        _pongGame = config.PongMinigame;
    }

    public void Initialize()
    {
        _pongGame.Initialize();
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
            _ => null
        };
    }
}