using ReadOfMask.Core.StateMachine;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using Zenject;
using Zenject.Asteroids;

public enum PlayerStateType
{
    General,
    Diary,
    Gaming,
    LookCloser, 
}

public sealed class PlayerStateManager : IPlayerStateManager
{
    public event Action<PlayerStateType> OnStateChanged;
    public PlayerStateType CurrentStateType { get; private set; } = PlayerStateType.General;

    private readonly StateMachine<PlayerState> _playerStateMachine = new();
    private readonly Dictionary<PlayerStateType, PlayerState> _map = new();

    public PlayerStateManager(PlayerModule.ConfigData configData, InputManager inputsManager,  DiContainer diContainer)
    {
        _map[PlayerStateType.General] = new GeneralState(configData.GeneralState);
        _map[PlayerStateType.Diary] = new DiaryState(configData.DiaryState);
        _map[PlayerStateType.Gaming] = new GamingState(configData.InteractionState);
        _map[PlayerStateType.LookCloser] = new LookCloserState(configData.InteractionState);

        foreach (var state in _map.Values)
        {
            diContainer.Inject(state);
        }
    }

    public void ChangeState(PlayerStateType state)
    {
        Debug.Log($"PlayerStateManager: ChangeState to {state}");
        _playerStateMachine.ChangeState(_map[state]);
        CurrentStateType = state;
        OnStateChanged?.Invoke(CurrentStateType);
    }
}
