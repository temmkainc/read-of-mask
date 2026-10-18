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

        // Level06 starts this track and it simply carries on into this level - but when the game
        // loads straight into Level07, Level06 never began, so start it here.
        AudioManager.Instance.PlayMusic(MusicTracks.Level01Music, fadeDuration: 0.5f);

        if (_frontDoor == null)
            return;

        _frontDoor.ResetToClosed();
        _frontDoor.SetDisabled(true);
    }
}
