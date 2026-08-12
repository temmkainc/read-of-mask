using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Level06 : LevelBase
{
    [SerializeField] private LookCloserInteractableBase _bed;
    [Inject] private Player _player;
     
    public override void Begin()
    {
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        // Wait a frame so the Player (and its Grabbing component) is guaranteed to have finished
        // its own Awake/Start before we interact with it - Begin() can run before that on the
        // very first frame of the scene.
        await UniTask.Yield();

        _bed.Interact(_player);
        await UniTask.Delay(1000);
        AudioManager.Instance.PlayMusic(MusicTracks.Level01Music, fadeDuration: 0.5f);
        await UniTask.Delay(2000);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 06 deactivated!");
    }
}
