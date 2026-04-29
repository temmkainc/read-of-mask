using UnityEngine;
using Zenject;

public class UnlockMaskInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    [Inject] private LockableActionsManager _lockableActionsManager;
    [Inject] private IMaskStateManager _maskStateManager;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding) 
            return;

        _lockableActionsManager.UnlockAction(LockableActionsManager.LockableActionType.ToggleMask);
        _maskStateManager.ChangeState(MaskStateType.Wearing);

        Destroy(gameObject);
    }
}
