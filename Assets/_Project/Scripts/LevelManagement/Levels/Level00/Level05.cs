using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level05 : LevelBase
{
    [SerializeField] private LetterCubeSlotReceiver _receiver;
    [SerializeField] private CubeToEndTheDEMO _letterCube;
    public override void Begin()
    {
        _receiver.ForceInsert(_letterCube);
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        AudioManager.Instance.PlayMusic(MusicTracks.Level05Music, fadeDuration: 0.5f);
        
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 05 deactivated!");
    }
}
