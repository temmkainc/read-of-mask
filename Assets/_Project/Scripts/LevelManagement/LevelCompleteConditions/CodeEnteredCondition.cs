using System;
using UnityEngine;

public class CodeEnteredCondition : MonoBehaviour, ILevelCompleteCondition
{
    [SerializeField] private DomoPhoneMenuController _domoPhoneMenuController;
    public event Action OnConditionMet;

    private void Awake()
    {
        _domoPhoneMenuController.OnCodeCorrect += On_CodeCorrect;
    }

    private void On_CodeCorrect()
    {
        OnConditionMet?.Invoke();
    }
}
