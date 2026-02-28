using ReadOfMask.Core.StateMachine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
public enum MaskStateType
{
    NotWearing,
    Wearing
}
public sealed class MaskStateManager : IMaskStateManager, IDisposable
{
    public event Action<MaskStateType> OnStateChanged;

    private readonly StateMachine<MaskState> _maskStateMachine = new();
    private readonly Dictionary<MaskStateType, MaskState> _states = new();

    private readonly InputManager _inputsManager;

    public MaskStateType CurrentStateType { get; private set; } = MaskStateType.NotWearing;

    private bool _isTransitioning;
    private MaskStateType? _pendingState;

    [Inject] private IPlayerStateManager _playerStateManager;

    public MaskStateManager(InputManager inputsManager, IPlayerStateManager playerStateManager, DiContainer diContainer)
    {
        _states[MaskStateType.NotWearing] = new NotWearingMaskState();
        _states[MaskStateType.Wearing] = new WearingMaskState();

        foreach (var state in _states.Values)
        {
            diContainer.Inject(state);
        }

        _inputsManager = inputsManager;
        _inputsManager.ToggleMaskAction.performed += On_MaskToggleRequested;

        _playerStateManager = playerStateManager;
        _playerStateManager.OnStateChanged += On_PlayerStateChanged;
    }

    private void On_PlayerStateChanged(PlayerStateType type)
    {
        if (CurrentStateType != MaskStateType.Wearing || type != PlayerStateType.Diary)
            return;

        On_MaskToggleRequested(new InputAction.CallbackContext());
    }

    public void Dispose()
    {
        _inputsManager.ToggleMaskAction.performed -= On_MaskToggleRequested;
        _playerStateManager.OnStateChanged -= On_PlayerStateChanged;
    }

    private void On_MaskToggleRequested(InputAction.CallbackContext context)
    {
        Debug.Log("Mask Toggle Requested");
        if (_isTransitioning)
            return;

        var target = CurrentStateType == MaskStateType.NotWearing
            ? MaskStateType.Wearing
            : MaskStateType.NotWearing;

        _pendingState = target;
        _isTransitioning = true;

        OnStateChanged?.Invoke(target);
    }

    public void ConfirmStateTransition()
    {
        if (_pendingState == null)
            return;

        CurrentStateType = _pendingState.Value;

        _maskStateMachine.ChangeState(_states[CurrentStateType]);

        _pendingState = null;
        _isTransitioning = false;
    }
}
