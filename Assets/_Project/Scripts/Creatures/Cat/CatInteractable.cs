using UnityEngine;
using ithappy.Animals_FREE;
[RequireComponent(typeof(CatAI))]
public class CatInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private Transform _hideSpot;
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

        _cat.FleeTo(_hideSpot.position);
    }
}
