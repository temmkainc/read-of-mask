using UnityEngine;

public class WearingMaskState : MaskState
{
    public WearingMaskState() : base() { }

    protected override void Initialize()
    {

    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Wearing Mask State Entered");
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Wearing Mask State Exited");
    }
}
