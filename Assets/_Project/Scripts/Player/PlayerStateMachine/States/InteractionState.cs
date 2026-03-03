using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Cysharp.Threading.Tasks;

public sealed class InteractionState : PlayerState
{
    [Inject] private InputManager _inputsManager;

    public InteractionState(PlayerStateData data) : base(data) { }

    public override void Enter()
    {
        base.Enter();
        SubscribeAfterFrame().Forget();
    }

    public override void Exit()
    {
        base.Exit();
        _inputsManager.StopInteractionAction.started -= On_StopInteractionRequestedHandler;
    }

    private async UniTaskVoid SubscribeAfterFrame()
    {
        await UniTask.NextFrame();
        _inputsManager.StopInteractionAction.started += On_StopInteractionRequestedHandler;
    }

    private void On_StopInteractionRequestedHandler(InputAction.CallbackContext context)
    {
        CommandBus.GoToPreviousPlayerState();
    }
}