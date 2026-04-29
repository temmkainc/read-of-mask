using System;
using UnityEngine;

public class InteractableDoorOpenedCondition : MonoBehaviour, ILevelCompleteCondition
{
    [SerializeField] private DoorInteractable _doorInteractable;

    public event Action OnConditionMet;

    private void Awake()
    {
        _doorInteractable.OnDoorOpened += On_DoorOpened;
    }
    
    private void On_DoorOpened()
    {
        OnConditionMet?.Invoke();
    }
}
