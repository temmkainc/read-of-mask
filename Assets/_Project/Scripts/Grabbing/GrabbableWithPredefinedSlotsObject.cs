using UnityEngine;

public class GrabbableWithPredefinedSlotsObject : GrabbableObject
{
    [SerializeField] private SlotReceiver[] _allowedSlots;

    public override void Grab(Player player, Transform holdPoint)
    {
        base.Grab(player, holdPoint);
        SetSlotsHighlighted(true);
    }

    public override void Release(Vector3 throwForce, bool isExternal = false)
    {
        base.Release(throwForce, isExternal);
        SetSlotsHighlighted(false);
    }

    private void SetSlotsHighlighted(bool active)
    {
        if (_allowedSlots == null) return;
        foreach (var slot in _allowedSlots)
            slot?.Show(active);
    }
}