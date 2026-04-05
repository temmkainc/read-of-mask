using System;
using UnityEngine;

public class LetterCubeSlotReceiver : SlotReceiver<LetterCube>
{
    public event Action<LetterCubeSlotReceiver> StateChanged;

    public char CurrentLetter { get; private set; }

    protected override void OnObjectInserted(LetterCube cube)
    {
        cube.SetCurrentSlot(this);
        SetCurrentLetter(cube.Letter);
    }

    public void SetCurrentLetter(char letter = char.MinValue)
    {
        CurrentLetter = letter;
        StateChanged?.Invoke(this);
    }
    public override void Clear()
    {
        base.Clear();
        SetCurrentLetter();
    }
}