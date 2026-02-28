using UnityEngine;

public interface IPlayerStateManager
{
    public event System.Action<PlayerStateType> OnStateChanged;
    public PlayerStateType CurrentStateType { get; }

    void ChangeState(PlayerStateType state);
}
