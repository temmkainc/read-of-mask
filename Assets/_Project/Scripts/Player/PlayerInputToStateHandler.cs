using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInputToStateHandler : MonoBehaviour
{
    [Inject] private InputManager _inputsManager;
    [Inject] private ICommandBus _commandBus;
    [Inject] private LockableActionsManager _lockableActionsManager;
    [SerializeField] private FacelessCharacterMenu _menu;


    private void OnEnable()
    {
        _inputsManager.OpenDiaryAction.performed += On_OpenDiaryRequested;
        _inputsManager.PlayerOpenMenuAction.performed += On_OpenMenuRequested;
    }

    private void On_OpenMenuRequested(InputAction.CallbackContext context)
    {
        _menu.EnterMenu();
    }

    private void OnDisable()
    {
        _inputsManager.OpenDiaryAction.performed -= On_OpenDiaryRequested;
        _inputsManager.PlayerOpenMenuAction.performed -= On_OpenMenuRequested;
    }

    private void On_OpenDiaryRequested(InputAction.CallbackContext context)
    {
        if (_lockableActionsManager.IsActionLocked(LockableActionsManager.LockableActionType.OpenDiary))
            return;
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.Diary)).Execute();
    }
}
