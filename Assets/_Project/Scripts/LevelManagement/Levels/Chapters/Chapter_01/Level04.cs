using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Level04 : LevelBase
{
    [SerializeField] private AudioSource _fridgeBuzzSource;
    [SerializeField] private AudioSource _fridgeLaughingSource;

    [Inject] IMaskStateManager _maskStateManager;
    
    public override void Begin()
    {   
        AudioManager.Instance.PlayMusic(MusicTracks.Level04Music, fadeDuration: 0.5f);
        _maskStateManager.OnStateChanged += On_MaskStateChanged;
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {

    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 04 deactivated!");
    }
    private void On_MaskStateChanged(MaskStateType state)
    {
        if (state == MaskStateType.Wearing)
        {
            OnSightEnter();
        }
        else
        {
            OnSightExit();
        }
    }

    private void OnSightEnter()
    {
        if (_fridgeLaughingSource) _fridgeLaughingSource.Play();
    }

    private void OnSightExit()
    {
        if (_fridgeLaughingSource) _fridgeLaughingSource.Stop();
    }
}
