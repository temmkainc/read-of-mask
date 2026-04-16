using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInputToStateHandler : MonoBehaviour
{
    [Inject] private InputManager _inputsManager;
    [Inject] private ICommandBus _commandBus;
    [SerializeField] private FacelessCharacterMenu _menu;


    private void OnEnable()
    {
        _inputsManager.OpenDiaryAction.started += On_OpenDiaryRequested;
        _inputsManager.PlayerOpenMenuAction.started += On_OpenMenuRequested;
    }

    private void On_OpenMenuRequested(InputAction.CallbackContext context)
    {
        _menu.EnterMenu();
    }

    private void OnDisable()
    {
        _inputsManager.OpenDiaryAction.started -= On_OpenDiaryRequested;
    }

    private void On_OpenDiaryRequested(InputAction.CallbackContext context)
    {
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.Diary)).Execute();
    }
}
