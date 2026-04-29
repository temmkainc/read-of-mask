using System.Collections.Generic;
using UnityEngine;

public class LockableActionsManager 
{
    public enum LockableActionType
    {
        None,
        ToggleMask,
        OpenDiary,
    }

    private Dictionary<LockableActionType, bool> _lockedActions = new();

    LockableActionsManager()
    {
        foreach (LockableActionType action in System.Enum.GetValues(typeof(LockableActionType)))
        {
            _lockedActions[action] = true;
        }
    }

    public bool IsActionLocked(LockableActionType action)
    {
        return _lockedActions.TryGetValue(action, out var isLocked) && isLocked;
    }

    public void UnlockAction(LockableActionType action)
    {
        _lockedActions[action] = false;
    }
}
