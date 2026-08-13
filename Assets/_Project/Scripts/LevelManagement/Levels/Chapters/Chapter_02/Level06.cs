using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Level06 : LevelBase
{
    [SerializeField] private LookCloserInteractableBase _bed;
    [Inject] private Player _player;
    [Inject] private IPlayerStateManager _playerStateManager;
    [Inject] private ICommandBus _commandBus;

    public override void Begin()
    {
        base.Begin();

        // Listen specifically to the bed's own exit event, not the generic player-state
        // broadcast - other LookCloser-based objects in the room (chairs, benches, etc.) fire
        // that same broadcast, and would otherwise incorrectly complete this wake-up step too.
        _bed.OnExitedLookCloser += On_BedExited;

        // Music starts once here, on the genuine first Begin() - NOT replayed on RestartLevel(),
        // so a wrong-action/stalling restart doesn't re-trigger the music each time.
        AudioManager.Instance.PlayMusic(MusicTracks.Level01Music, fadeDuration: 0.5f);

        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        await UniTask.Yield();

        _bed.Interact(_player);
        await UniTask.Delay(1000);
        await UniTask.Delay(2000);
    }

    // Restarting the level (e.g. after an ObligatorySequence wrong action) re-plays the bed
    // interaction only - teleport back to spawn, then interact with the bed again - without
    // re-triggering the music, which is already playing from the original Begin().
    public override void RestartLevel()
    {
        base.RestartLevel();

        // If the player is still mid-interaction with the bed (e.g. this restart was triggered
        // by stalling in bed too long, rather than a wrong action elsewhere), force a clean exit
        // first. Re-entering LookCloser while already in it - without exiting first - leaves a
        // duplicate, unpaired state/input subscription behind, which is what was breaking mouse
        // look after a stalling-triggered restart.
        if (_playerStateManager.CurrentStateType == PlayerStateType.LookCloser)
            _commandBus.GoToPreviousPlayerState();

        PlayIntroSequence().Forget();
    }

    // "Waking up" = getting out of bed = leaving the LookCloser state the bed interaction put
    // the player into. Completes the wake-up obligatory step (if one is active) the moment that
    // happens - and only when it's specifically the bed's own session ending.
    private void On_BedExited()
    {
        ObligatorySequence?.CurrentStep?.Complete();
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 06 deactivated!");
    }

    private void OnDestroy()
    {
        _bed.OnExitedLookCloser -= On_BedExited;
    }
}
