using UnityEngine;
using ithappy.Animals_FREE;
using Zenject;
[RequireComponent(typeof(CatAI))]
public class CatInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private Transform _hideSpot;
    [Inject] private PlayerFirstPersonHandsController _handsController;
    private CatAI _cat;

    private void Awake()
    {
        _cat = GetComponent<CatAI>();
    }

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding)
            return;

        _handsController.PlayAngryAnimation();
        _cat.FleeTo(_hideSpot.position);
    }
}
