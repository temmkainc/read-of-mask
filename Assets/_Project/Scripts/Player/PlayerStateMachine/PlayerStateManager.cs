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
}

public sealed class PlayerStateManager : IPlayerStateManager
{
    public event Action<PlayerStateType> OnStateChanged;
    public PlayerStateType CurrentStateType { get; private set; } = PlayerStateType.General;

    private readonly StateMachine<PlayerState> _playerStateMachine = new();
    private static readonly Dictionary<PlayerStateType, PlayerState> _map = new();

    public PlayerStateManager(PlayerModule.ConfigData configData, InputManager inputsManager,  DiContainer diContainer)
    {
        _map[PlayerStateType.General] = new GeneralState(configData.GeneralState);
        _map[PlayerStateType.Diary] = new DiaryState(configData.DiaryState);

        foreach (var state in _map.Values)
        {
            diContainer.Inject(state);
        }
    }

    public void ChangeState(PlayerStateType state)
    {
        _playerStateMachine.ChangeState(_map[state]);
        CurrentStateType = state;
        OnStateChanged?.Invoke(CurrentStateType);
    }
    
    public static PlayerStateData GetStateData(PlayerStateType stateType)
    {
        return _map.TryGetValue(stateType, out var state)
            ? state.Data
            : throw new KeyNotFoundException($"State data for {stateType} not found.");
    }
}
