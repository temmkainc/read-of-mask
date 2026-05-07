using UnityEngine;
using Zenject;

public class UnlockDiaryInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    [Inject] private LockableActionsManager _lockableActionsManager;
    [Inject] private ICommandBus _commandBus;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding) 
            return;

        _lockableActionsManager.UnlockAction(LockableActionsManager.LockableActionType.OpenDiary);
        _commandBus.Register(() => new PlayerStateChangeCommand(PlayerStateType.Diary)).Execute();

        Destroy(gameObject);
    }
}
