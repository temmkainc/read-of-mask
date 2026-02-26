using UnityEngine;

public sealed class NotWearingMaskState : MaskState
{
    public NotWearingMaskState() : base() { }

    protected override void Initialize()
    {

    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Not Wearing Mask State Entered");
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Not Wearing Mask State Exited");
    }
}
