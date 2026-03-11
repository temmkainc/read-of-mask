using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Base class for all minigames: handles menu + enter/exit/pause/resume.
/// </summary>
public abstract class MinigameBase : MonoBehaviour, IMinigame
{
    public event Action OnMinigameExitedInternally;

    [SerializeField] protected GameObject _gameplayRoot;
    [SerializeField] protected GameObject _menuRoot;
    [SerializeField] protected List<Button> _menuButtons;
    [Inject] protected InputManager _inputManager;

    protected MinigameMenu _menu;
    protected bool _isPaused = true;

    protected InputAction _gamingInputAction;
    protected InputAction _actionInputAction;

    public virtual void Initialize()
    {
        _gamingInputAction = _inputManager.GamingDirectionAction;
        _actionInputAction = _inputManager.GamingActionAction;
        if (_menuButtons != null && _menuButtons.Count > 0)
        {
            _menu = new MinigameMenu(_menuButtons, _gamingInputAction, _actionInputAction);
            _menu.OnButtonSelected += OnMenuButtonSelected;
        }
        _menuRoot.SetActive(false); 
        _gameplayRoot.SetActive(false);
    }

    public virtual void EnterGame()
    {
        _menuRoot.SetActive(true);
        _isPaused = true;
        _menu?.EnterMenu();
    }

    public virtual void ExitGame()
    {
        _menu?.ExitMenu();
        _menuRoot.SetActive(false);
        _gameplayRoot.SetActive(false);
        _isPaused = true;
    }

    public virtual void PauseGame()
    {
        _isPaused = true;
    }

    public virtual void ResumeGame()
    {
        _isPaused = false;
    }

    public virtual void StartGame() {
        _isPaused = false;
        _menu?.ExitMenu();
        _menuRoot.SetActive(false);
        _gameplayRoot.SetActive(true);
    }

    public virtual void ExitGameInternally()
    {
        OnMinigameExitedInternally?.Invoke();
        ExitGame();
    }

    protected abstract void OnMenuButtonSelected(int index);
}