using System;
using UnityEngine;

public class WordPassCompleteCondition : MonoBehaviour, ILevelCompleteCondition
{
    [SerializeField] private LetterCubeSlotReceiver[] _letterCubeSlotReceivers;
    [SerializeField] private string _targetWord;

    public event Action OnConditionMet;

    private void Awake()
    {
        foreach (var receiver in _letterCubeSlotReceivers)
        {
            receiver.StateChanged += On_CubeSlotReceiverStateChanged;
        }
    }

    private void On_CubeSlotReceiverStateChanged(LetterCubeSlotReceiver receiver)
    {
        CheckWordMatch();
    }

    private void CheckWordMatch()
    {
        for(int i = 0; i < _letterCubeSlotReceivers.Length; i++)
        {
            if (_letterCubeSlotReceivers[i].CurrentLetter != _targetWord[i])
                return;
        }
        OnConditionMet?.Invoke();
    }
}
