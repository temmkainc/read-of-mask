using UnityEngine;

public class Level07 : LevelBase
{
    [Tooltip("The apartment's front door, which the player walked out of to get here. Locked when the game loads straight into this level, so the player can't go back inside.")]
    [SerializeField] private DoorInteractable _frontDoor;

    public override void Begin()
    {
        base.Begin();
        Debug.Log("Level 07 started!");
    }

    public override void OnStartedFromLoad()
    {
        base.OnStartedFromLoad();

        if (_frontDoor == null)
            return;

        _frontDoor.ResetToClosed();
        _frontDoor.SetDisabled(true);
    }
}
