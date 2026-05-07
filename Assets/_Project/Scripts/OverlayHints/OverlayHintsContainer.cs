using Zenject;
using TMPro;
using UnityEngine;

public class OverlayHintsSceneContainer : MonoBehaviour
{
    [field: SerializeField] public TMP_Text ToInteractTMP { get; private set; }
    [field: SerializeField] public TMP_Text ToToggleMaskTMP { get; private set; }
    [field: SerializeField] public TMP_Text ToOpenDiaryTMP { get; private set; }
    [field: SerializeField] public TMP_Text ToThrowTMP { get; private set; }
    [field: SerializeField] public TMP_Text ToExitTMP { get; private set; }
    [Inject] private LockableActionsManager _lockableActionsManager;

    public void OnLookCloserStateEntered()
    {
        ToOpenDiaryTMP.gameObject.SetActive(false);
        ToToggleMaskTMP.gameObject.SetActive(false);
        ToExitTMP.gameObject.SetActive(true);
    }

    public void OnLookCloserStateExited()
    {
        if(!_lockableActionsManager.IsActionLocked(LockableActionsManager.LockableActionType.OpenDiary))
        {
            ToOpenDiaryTMP.gameObject.SetActive(true);
        }

        if(!_lockableActionsManager.IsActionLocked(LockableActionsManager.LockableActionType.ToggleMask))
        {
            ToToggleMaskTMP.gameObject.SetActive(true);
        }
        ToExitTMP.gameObject.SetActive(false);
    }
}
