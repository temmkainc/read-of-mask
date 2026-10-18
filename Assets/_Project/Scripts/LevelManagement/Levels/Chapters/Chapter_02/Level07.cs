using UnityEngine;

public class Level07 : LevelBase
{
    [Tooltip("The apartment's front door, which the player walked out of to get here. Locked when the game loads straight into this level, so the player can't go back inside.")]
    [SerializeField] private DoorInteractable _frontDoor;

    [Tooltip("Level of this level's music relative to the music volume setting.")]
    [SerializeField][Range(0f, 1f)] private float _musicVolume = 0.8f;

    public override void Begin()
    {
        base.Begin();
        Debug.Log("Level 07 started!");

        // Crossfades from Level06's music when played through; fades in when the game loads here.
        AudioManager.Instance.PlayMusic(MusicTracks.Level02Music, fadeDuration: 1f, volumeScale: _musicVolume);
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
