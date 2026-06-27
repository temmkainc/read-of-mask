using UnityEngine;
using Zenject;

public class UnlockMaskInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Hint")]
    [SerializeField] private float _hintXOffset = 0.3f;
    [SerializeField] private float _hintYOffset = 1.5f;
    [SerializeField] private float _hintZOffset = 0f;
    
    public string HintLabel => "Take";
    public float HintYOffset => _hintYOffset;
    public float HintXOffset => _hintXOffset;
    public float HintZOffset => _hintZOffset;


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
