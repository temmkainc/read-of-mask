using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LockableActionsManager : IInitializable
{
    [Inject] private OverlayHintsSceneContainer _overlayHintsSceneContainer;
    [Inject] private ISaveService _saveService;

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

    public void Initialize()
    {
        // Restore actions that were already unlocked in a previous play session.
        foreach (LockableActionType action in System.Enum.GetValues(typeof(LockableActionType)))
        {
            if (action == LockableActionType.None)
                continue;

            if (!_saveService.IsActionUnlocked(action))
                continue;

            _lockedActions[action] = false;
            ShowHint(action);
        }
    }

    public bool IsActionLocked(LockableActionType action)
    {
        return _lockedActions.TryGetValue(action, out var isLocked) && isLocked;
    }

    public void UnlockAction(LockableActionType action)
    {
        bool wasLocked = IsActionLocked(action);

        _lockedActions[action] = false;
        ShowHint(action);

        if (wasLocked)
            _saveService.SetActionUnlocked(action, true);
    }

    private void ShowHint(LockableActionType action)
    {
        if (action == LockableActionType.ToggleMask)
            _overlayHintsSceneContainer.ToToggleMaskTMP.gameObject.SetActive(true);
        if (action == LockableActionType.OpenDiary)
            _overlayHintsSceneContainer.ToOpenDiaryTMP.gameObject.SetActive(true);
    }
}
