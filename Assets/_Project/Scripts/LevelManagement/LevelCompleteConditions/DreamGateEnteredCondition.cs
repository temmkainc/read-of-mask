using System;
using UnityEngine;

/// <summary>
/// Level-complete condition satisfied the moment the player steps into a DreamGate. Fires at the
/// start of the gate's transition (not the end), so the next level is already there by the
/// time the player comes out the other side.
/// </summary>
public class DreamGateEnteredCondition : MonoBehaviour, ILevelCompleteCondition
{
    [SerializeField] private DreamGate _gate;

    public event Action OnConditionMet;

    private void Awake()
    {
        _gate.OnEntered += On_GateEntered;
    }

    private void OnDestroy()
    {
        if (_gate != null)
            _gate.OnEntered -= On_GateEntered;
    }

    private void On_GateEntered()
    {
        OnConditionMet?.Invoke();
    }
}
