using System;
using UnityEngine;

/// <summary>
/// Level-complete condition satisfied once the level's ObligatorySequence finishes (every step
/// in the routine completed in order). Place this on the same GameObject as this level's
/// LevelBase (same as the other LevelCompleteConditions), and assign the sequence in the scene.
/// </summary>
public class ObligatorySequenceCompleteCondition : MonoBehaviour, ILevelCompleteCondition
{
    [SerializeField] private ObligatorySequence _sequence;

    public event Action OnConditionMet;

    private void Awake()
    {
        _sequence.OnSequenceComplete += On_SequenceComplete;
    }

    private void On_SequenceComplete()
    {
        OnConditionMet?.Invoke();
    }

    private void OnDestroy()
    {
        _sequence.OnSequenceComplete -= On_SequenceComplete;
    }
}
