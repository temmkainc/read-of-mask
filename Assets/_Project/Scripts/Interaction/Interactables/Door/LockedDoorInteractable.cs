using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using Zenject;

public class LockedDoorInteractable : MonoBehaviour, IInteractable, IHighlightable
{

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    [Header("Hint")]
    [SerializeField] private string _hintLabel = "Try to open";
    [SerializeField] private float _hintXOffset = 0f;
    [SerializeField] private float _hintYOffset = 0.3f; 
    [SerializeField] private float _hintZOffset = 0f;

    [Inject] private PlayerFirstPersonHandsController _handsController;

    public string HintLabel => _hintLabel;

    public float HintXOffset => _hintXOffset;
    public float HintYOffset => _hintYOffset;
    public float HintZOffset => _hintZOffset;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding)
            return;

        _handsController.PlayNotAllowedAnimation();
    }
}