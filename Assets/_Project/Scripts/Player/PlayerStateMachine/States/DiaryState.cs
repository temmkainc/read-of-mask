using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public sealed class DiaryState : PlayerState
{
    [Inject] private InputManager _inputsManager;
    [Inject] private Diary _diary;
    
    public int On_CloseDiaryRequested { get; private set; }

    public DiaryState(PlayerStateData data) : base(data)
    {

    }
    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Enter()
    {
        base.Enter();

        _diary.gameObject.SetActive(true);

        _inputsManager.CloseDiaryAction.started += On_CloseDiaryRequestedHandler;
    }

    private void On_CloseDiaryRequestedHandler(InputAction.CallbackContext context)
    {
        CommandBus.GoToPreviousPlayerState();
    }

    public override void Exit()
    {
        base.Exit();

        _diary.gameObject.SetActive(false);

        _inputsManager.CloseDiaryAction.started -= On_CloseDiaryRequestedHandler;
    }
}
