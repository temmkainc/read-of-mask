using UnityEngine;
using Zenject;

/// <summary>
/// Trigger-volume counterpart to ObligatoryStepGate, for cases where the "wrong action" is
/// simply walking somewhere rather than interacting with an object (e.g. wandering into the
/// wrong room). Same dynamic correctness check against the CURRENT level's active
/// ObligatorySequence - not tied to a specific sequence instance, so this works correctly
/// regardless of which level happens to be active.
///
/// Setup: put this on a trigger collider covering the zone, set an Action Id, and make sure the
/// player has the expected layer/tag your project's trigger detection already relies on.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ObligatoryStepTriggerGate : MonoBehaviour
{
    [Tooltip("Label identifying what this zone represents (e.g. 'KitchenZone', 'WrongBedroomZone'). Matched against the active step's Expected Action Ids.")]
    [SerializeField] private string _actionId;

    [Tooltip("If true, entering this zone while it's the expected action completes the step. If false, entering while expected is just ignored (use this for zones that are only ever 'wrong', with no corresponding step to complete).")]
    [SerializeField] private bool _completesStepWhenExpected = true;

    [Inject(Optional = true)] private LevelManager _levelManager;
    [Inject(Optional = true)] private PlayerFirstPersonHandsController _handsController;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Player>(out _))
            return;

        var step = _levelManager != null ? _levelManager.CurrentObligatorySequence?.CurrentStep : null;
        if (step == null)
            return;

        if (!step.IsExpectedAction(_actionId))
        {
            // Same "not allowed" feedback as a wrong door, for consistency - there's no physical
            // object to react here, but the player should still get the same visual cue.
            _handsController?.PlayNotAllowedAnimation();

            step.TriggerWrongAction();
            return;
        }

        if (_completesStepWhenExpected)
            step.Complete();
    }
}
