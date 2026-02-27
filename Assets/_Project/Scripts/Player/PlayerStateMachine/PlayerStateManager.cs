using ReadOfMask.Core.StateMachine;
using System.Buffers;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using Zenject;
using Zenject.Asteroids;

public enum StateType
{
    General,
    Diary,
}

public sealed class PlayerStateManager : IPlayerStateManager
{
    public StateType CurrentStateType { get; private set; }

    private readonly StateMachine<PlayerState> _playerStateMachine = new();
    private static readonly Dictionary<StateType, PlayerState> _map = new();

    public PlayerStateManager(PlayerModule.ConfigData configData, InputsManager inputsManager,  DiContainer diContainer)
    {
        _map[StateType.General] = new GeneralState(configData.GeneralState);
        _map[StateType.Diary] = new DiaryState(configData.DiaryState);

        foreach (var state in _map.Values)
        {
            diContainer.Inject(state);
        }

        ChangeState(StateType.General);
    }

    public void ChangeState(StateType state)
    {
        Debug.Log($"Changing state to {state}");
        _playerStateMachine.ChangeState(_map[state]);
        CurrentStateType = state;
    }
    
    public static PlayerStateData GetStateData(StateType stateType)
    {
        return _map.TryGetValue(stateType, out var state)
            ? state.Data
            : throw new KeyNotFoundException($"State data for {stateType} not found.");
    }
}
