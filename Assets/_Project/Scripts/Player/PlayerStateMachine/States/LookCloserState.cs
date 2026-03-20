using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Cysharp.Threading.Tasks;

public sealed class LookCloserState : PlayerState
{
    [Inject] private InputManager _inputsManager;

    public LookCloserState(PlayerStateData data) : base(data) { }

    public override void Enter()
    {
        base.Enter();
        _inputsManager.StopLookCloserAction.performed += On_StopLookCloserRequestedHandler;
    }

    public override void Exit()
    {
        base.Exit();
        _inputsManager.StopLookCloserAction.started -= On_StopLookCloserRequestedHandler;
    }

    private async UniTaskVoid SubscribeAfterFrame()
    {
        await UniTask.NextFrame();
        _inputsManager.StopLookCloserAction.started += On_StopLookCloserRequestedHandler;
    }

    private void On_StopLookCloserRequestedHandler(InputAction.CallbackContext context)
    {
        CommandBus.GoToPreviousPlayerState();
    }
}