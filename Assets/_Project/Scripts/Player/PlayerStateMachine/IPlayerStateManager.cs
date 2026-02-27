using UnityEngine;

public interface IPlayerStateManager
{
    public StateType CurrentStateType { get; }

    void ChangeState(StateType state);
}
