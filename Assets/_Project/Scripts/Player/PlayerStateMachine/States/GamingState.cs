using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Cysharp.Threading.Tasks;

public sealed class GamingState : PlayerState
{
    [Inject] private InputManager _inputsManager;

    public GamingState(PlayerStateData data) : base(data) { }

    public override void Enter()
    {
        base.Enter();
        SubscribeAfterFrame().Forget();
    }

    public override void Exit()
    {
        base.Exit();
        _inputsManager.StopGamingAction.started -= On_StopInteractionRequestedHandler;
    }

    private async UniTaskVoid SubscribeAfterFrame()
    {
        await UniTask.NextFrame();
        _inputsManager.StopGamingAction.started += On_StopInteractionRequestedHandler;
    }

    private void On_StopInteractionRequestedHandler(InputAction.CallbackContext context)
    {
        CommandBus.GoToPreviousPlayerState();
    }
}