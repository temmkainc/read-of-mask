using UnityEngine;

public sealed class GeneralState : PlayerState
{
    public GeneralState(PlayerStateData data) : base(data)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
