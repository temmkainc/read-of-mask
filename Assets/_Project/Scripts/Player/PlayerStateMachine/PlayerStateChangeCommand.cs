using JetBrains.Annotations;
using UnityEngine;
using Zenject;

public sealed class PlayerStateChangeCommand : ISyncCommand, IUndoCommand
{
    private readonly StateType _stateType;

    private StateType _previousStateType;
    [Inject] private IPlayerStateManager _playerStateManager;

    public PlayerStateChangeCommand(
        StateType type)
    {
        _stateType = type;
    }

    public void Execute()
    {
        if (_playerStateManager.CurrentStateType == _stateType)
        {
            Debug.LogWarning($"You can't access the {_stateType} because you are already on it.");
            return;
        }

        _previousStateType = _playerStateManager.CurrentStateType;
        _playerStateManager.ChangeState(_stateType);
    }

    public void Undo()
    {
        _playerStateManager.ChangeState(_previousStateType);
    }
}
