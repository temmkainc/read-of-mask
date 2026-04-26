using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using Zenject;

public class LockedDoorInteractable : MonoBehaviour, IInteractable, IHighlightable
{

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    [Inject] private PlayerFirstPersonHandsController _handsController;
    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding)
            return;

        _handsController.PlayNotAllowedAnimation();
    }
}