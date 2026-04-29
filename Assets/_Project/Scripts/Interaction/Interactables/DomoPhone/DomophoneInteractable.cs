using UnityEngine;

public class DomoPhoneInteractable : LookCloserInteractableBase
{
    [SerializeField] private DomoPhoneMenuController _menuController;

    public override void Interact(Player player = null)
    {
        if (player.Grabbing.IsHolding) return;

        base.Interact(player);
    }

    protected override void On_PlayerStateChanged(PlayerStateType type)
    {
        var previous = _previousPlayerStateType; 
        base.On_PlayerStateChanged(type);

        if (type == PlayerStateType.LookCloser)
        {
            _menuController.Activate();
        }
        else if (previous == PlayerStateType.LookCloser)
        {
            _menuController.Deactivate();
        }
    }
}