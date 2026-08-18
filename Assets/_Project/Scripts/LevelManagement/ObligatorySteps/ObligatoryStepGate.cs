using UnityEngine;
using Zenject;

/// <summary>
/// Wraps an existing interactable and dynamically decides whether interacting with it is
/// currently "correct" or "wrong", based purely on whatever the CURRENT level's active
/// ObligatorySequence step expects right now - not on any fixed per-object rule, and not tied to
/// a specific sequence instance. So the same door can be wrong in one step and correct in a
/// later one with zero rewiring, and this same component works correctly regardless of which
/// level (and which level's own ObligatorySequence) happens to be active when it's used.
///
/// Setup: put this on the same GameObject as the real interactable (a door, a prop, etc.),
/// assign that interactable to _wrappedInteractable, and give this gate an Action Id
/// (e.g. "BathroomDoor") matching whatever ObligatoryStep.ExpectedActionIds should accept it.
/// </summary>
public class ObligatoryStepGate : MonoBehaviour, IInteractable, IHighlightable
{
    [Tooltip("Label identifying what this object represents (e.g. 'BathroomDoor', 'Fridge', 'Bag'). Matched against the active step's Expected Action Ids.")]
    [SerializeField] private string _actionId;

    [Tooltip("The real interactable this gate wraps - must implement IInteractable, and may also implement IHighlightable.")]
    [SerializeField] private MonoBehaviour _wrappedInteractable;

    [Inject(Optional = true)] private LevelManager _levelManager;

    private IInteractable Wrapped => _wrappedInteractable as IInteractable;
    private IHighlightable WrappedHighlightable => _wrappedInteractable as IHighlightable;

    public bool CanHighlight(PlayerGrabbing grabbing)
        => WrappedHighlightable?.CanHighlight(grabbing) ?? true;

    /// <summary>Resets the wrapped door back to closed, if it is one. No-op for anything else
    /// (e.g. LockedDoorInteractable, which never visually opens in the first place).</summary>
    public void ResetWrappedDoorState()
    {
        if (_wrappedInteractable is DoorInteractable door)
            door.ResetToClosed();
    }

    public void Interact(Player player)
    {
        var step = _levelManager != null ? _levelManager.CurrentObligatorySequence?.CurrentStep : null;

        // No active obligatory step right now (current level has none, sequence not running,
        // or between levels) - behave like a normal, unrestricted interactable.
        if (step == null)
        {
            Wrapped?.Interact(player);
            return;
        }

        if (!step.IsExpectedAction(_actionId))
        {
            // Show "not allowed" feedback on the wrapped door (if it is one) instead of doing
            // nothing - the door itself stays shut, matching the script's "wrong doors show a
            // finger-wag and stay closed" behavior.
            if (_wrappedInteractable is DoorInteractable door)
                door.PlayNotAllowedFeedback();
            else if (_wrappedInteractable is LockedDoorInteractable lockedDoor)
                lockedDoor.Interact(player); // its own Interact() IS the not-allowed feedback - safe to call, never opens anything

            step.TriggerWrongAction();
            return;
        }

        Wrapped?.Interact(player);
        step.Complete();
    }
}
