using UnityEngine;

public class LetterCube : GrabbableObject
{
    [field: SerializeField] public char Letter { get; set; }

    public LetterCubeSlotReceiver CurrentLetterCubeHolder { get; private set; }

    public void SetCurrentHolder(LetterCubeSlotReceiver letterCubeHolder)
    {
        CurrentLetterCubeHolder = letterCubeHolder;
    }

    public override void Grab(Player player, Transform holdPoint)
    {
        base.Grab(player, holdPoint);
        if (CurrentLetterCubeHolder == null)
            return;

        CurrentLetterCubeHolder.Clear();
        CurrentLetterCubeHolder = null;
    }
}
