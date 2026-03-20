using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

public abstract class MinigameBase : MonoBehaviour, IMinigame
{
    public event Action OnMinigameExitedInternally;

    [SerializeField] protected GameObject _gameplayRoot;
    [SerializeField] protected GameObject _menuRoot;
    [SerializeField] protected List<InGameMenuItemBase> _menuButtons;
    [SerializeField] protected GameObject _pauseMenuRoot;
    [SerializeField] protected List<InGameMenuItemBase> _pauseMenuButtons;
    [SerializeField] protected Camera _camera;
    [Inject] protected InputManager _inputManager;

    protected InGameMenu _menu;
    protected InGameMenu _pauseMenu;

    protected bool _isPaused = true;

    protected InputAction _gamingInputAction;
    protected InputAction _actionInputAction;
    protected InputAction _pauseInputAction;

    public virtual void Initialize()
    {
        _gamingInputAction = _inputManager.GamingDirectionAction;
        _actionInputAction = _inputManager.GamingActionAction;
        _pauseInputAction = _inputManager.GamingPauseAction;

        _menu = new InGameMenu(
            _menuButtons.ConvertAll(x => (IInGameMenuItem)x),
            _gamingInputAction,
            _actionInputAction
        );
        _menu.OnItemSubmitted += OnMenuButtonSelected;

        _pauseMenu = new InGameMenu(
           _pauseMenuButtons.ConvertAll(x => (IInGameMenuItem)x),
           _gamingInputAction,
           _actionInputAction
       );
        _pauseMenu.OnItemSubmitted += OnPauseMenuButtonSelected;

        _menuRoot.SetActive(false); 
        _gameplayRoot.SetActive(false);
        _pauseMenuRoot.SetActive(false);
        _camera.gameObject.SetActive(false);
    }

    private void On_PauseInputPerformed(InputAction.CallbackContext context)
    {
        PauseGame();
    }

    public virtual void EnterGame()
    {
        _camera.gameObject.SetActive(true);
        _isPaused = true;
        _menuRoot.SetActive(true);
        _menu.EnterMenu();
    }

    public virtual void ExitGame()
    {
        _camera.gameObject.SetActive(false);
        _isPaused = true;
        _pauseInputAction.performed -= On_PauseInputPerformed;

        _menu.ExitMenu();
        _menuRoot.SetActive(false);

        _pauseMenu.ExitMenu();
        _pauseMenuRoot.SetActive(false);

        _gameplayRoot.SetActive(false);
    }

    public virtual void PauseGame()
    {
        _isPaused = true;
        _pauseMenuRoot.SetActive(true);
        _pauseMenu.EnterMenu();
    }

    public virtual void ResumeGame()
    {
        _isPaused = false;
        _pauseMenuRoot.SetActive(false);
        _pauseMenu.ExitMenu();
    }

    public virtual void StartGame() {
        _isPaused = false;
        _menu.ExitMenu();
        _menuRoot.SetActive(false);
        _gameplayRoot.SetActive(true);
        _pauseInputAction.performed += On_PauseInputPerformed;
    }

    public virtual void ExitToMenu()
    {
        _gameplayRoot.SetActive(false);
        
        _menuRoot.SetActive(true);
        _menu.EnterMenu();

        _pauseMenuRoot.SetActive(false);
        _pauseMenu.ExitMenu();
        _pauseInputAction.performed -= On_PauseInputPerformed;
    }

    public virtual void ExitGameInternally()
    {
        OnMinigameExitedInternally?.Invoke();
        ExitGame();
    }

    protected abstract void OnMenuButtonSelected(int index);

    protected abstract void OnPauseMenuButtonSelected(int index);
}