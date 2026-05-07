using System;
using UnityEngine;
using Zenject;

public class WordPassCompleteCondition : MonoBehaviour, ILevelCompleteCondition
{
    [SerializeField] private LetterCubeSlotReceiver[] _letterCubeSlotReceivers;
    [SerializeField] private string _targetWord;
    [Inject] private UnlockedLettersManager _unlockedLettersManager;
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

        for(int i = 0; i < _targetWord.Length; i++)
        {
            _unlockedLettersManager.UnlockLetter(_targetWord[i]);
        }
        OnConditionMet?.Invoke();
    }
}
