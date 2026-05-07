using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Cysharp.Threading.Tasks;

public sealed class LookCloserState : PlayerState
{
    [Inject] private InputManager _inputsManager;
    [InjectOptional] private OverlayHintsSceneContainer _overlayHintsSceneContainer;
    public LookCloserState(PlayerStateData data) : base(data) { }

    public override void Enter()
    {
        base.Enter();
        _overlayHintsSceneContainer?.OnLookCloserStateEntered();
        _inputsManager.StopLookCloserAction.performed += On_StopLookCloserRequestedHandler;
    }

    public override void Exit()
    {
        base.Exit();
        _overlayHintsSceneContainer?.OnLookCloserStateExited();
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