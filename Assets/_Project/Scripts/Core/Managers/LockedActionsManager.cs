using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LockableActionsManager 
{
    [Inject] private OverlayHintsSceneContainer _overlayHintsSceneContainer;
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
        if(action == LockableActionType.ToggleMask)
            _overlayHintsSceneContainer.ToToggleMaskTMP.gameObject.SetActive(true);
        if(action == LockableActionType.OpenDiary)
            _overlayHintsSceneContainer.ToOpenDiaryTMP.gameObject.SetActive(true);
    }
}
