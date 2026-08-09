using UnityEngine;
using ithappy.Animals_FREE;
using Zenject;

public class CatInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    public enum CatInteractionType
    {
        Scare,
        Pet
    }

    [SerializeField] private CatInteractionType _interactionType;
    [SerializeField] private Transform _hideSpot;
    [SerializeField] private bool _isMoving = true;
    [Inject] private PlayerFirstPersonHandsController _handsController;
    private CatAI _cat;

    private void Awake()
    {
        if (!_isMoving)
            return;
        _cat = GetComponent<CatAI>();
    }

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    public string HintLabel => _interactionType == CatInteractionType.Pet ? "Pet cat" : "Scare cat";

    public float HintYOffset => 1f;

    public void Interact(Player player)
    {
        if (player.Grabbing.IsHolding)
            return;

        if (_interactionType == CatInteractionType.Pet)
        {
            gameObject.SetActive(false);
            _handsController.PlayCatPatAnimation(() => gameObject.SetActive(true));
            return;
        }

        _handsController.PlayAngryAnimation();
        if (_hideSpot == null)
            return;

        _cat.FleeTo(_hideSpot.position);
    }
}
