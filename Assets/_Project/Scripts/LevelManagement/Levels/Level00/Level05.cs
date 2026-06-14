using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level05 : LevelBase
{
    [SerializeField] private LetterCubeSlotReceiver _receiver;
    [SerializeField] private AudioSource _headmistressAudioSource;
    [SerializeField] private CubeToEndTheDEMO _letterCube;
    public override void Begin()
    {
        _receiver.ForceInsert(_letterCube);
        base.Begin();
        PlayIntroSequence().Forget();
    }

    private async UniTask PlayIntroSequence()
    {
        await UniTask.Delay(700);
        AudioManager.Instance.PlayMusic(MusicTracks.Level05Music, fadeDuration: 0.5f);
        await UniTask.Delay(2000);
        await AudioManager.Instance.PlayVoicelineAsync(Voicelines.HeadmistressTakeOut, _headmistressAudioSource);
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 05 deactivated!");
    }
}
