using System;
using UnityEngine;

public class LetterCubeSlotReceiver : SlotReceiver<LetterCube>
{
    public event Action<LetterCubeSlotReceiver> StateChanged;
    public char CurrentLetter { get; private set; }

    [Header("Hint")]
    [SerializeField] private float _hintXOffset = 0f;
    [SerializeField] private float _hintYOffset = 0.5f;
    [SerializeField] private float _hintZOffset = -0.1f;

    public override string HintLabel => IsOccupied ? "Take out" : "Insert";
    public override float HintXOffset => _hintXOffset;
    public override float HintYOffset => _hintYOffset;
    public override float HintZOffset => _hintZOffset;

    public override bool CanHighlight(PlayerGrabbing grabbing)
        => grabbing.IsHolding && grabbing.TryGetHeld<LetterCube>(out _);

    protected override void OnObjectInserted(LetterCube cube)
    {
        cube.SetCurrentSlot(this);
        SetCurrentLetter(cube.Letter);
    }

    public virtual void SetCurrentLetter(char letter = char.MinValue)
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