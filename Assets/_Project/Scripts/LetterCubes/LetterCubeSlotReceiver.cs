using System;
using UnityEngine;

public class LetterCubeSlotReceiver : SlotReceiver<LetterCube>
{
    public event Action<LetterCubeSlotReceiver> StateChanged;

    [SerializeField] private char _correctLetter;
    [SerializeField] private char _currentLetter;

    public bool IsCorrect => _correctLetter == _currentLetter;

    protected override void OnObjectInserted(LetterCube cube)
    {
        cube.SetCurrentSlot(this);
        SetCurrentLetter(cube.Letter);
    }

    public void SetCurrentLetter(char letter = char.MinValue)
    {
        _currentLetter = letter;
        StateChanged?.Invoke(this);
    }
    protected override void OnCleared()
    {
        SetCurrentLetter();
    }
}