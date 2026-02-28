using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInputToStateHandler : MonoBehaviour
{
    [Inject] private InputManager _inputsManager;
    [Inject] private ICommandBus _commandBus;

    private void OnEnable()
    {
        _inputsManager.OpenDiaryAction.started += On_OpenDiaryRequested;
    }

    private void OnDisable()
    {
        _inputsManager.OpenDiaryAction.started -= On_OpenDiaryRequested;
    }

    private void On_OpenDiaryRequested(InputAction.CallbackContext context)
    {
        _commandBus.Register(() => new PlayerStateChangeCommand(StateType.Diary)).Execute();
    }
}
