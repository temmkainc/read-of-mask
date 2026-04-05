using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Level00 : LevelBase
{
    [Inject] private CameraEffects _cameraEffects;

    private const float SECONDS_BEFORE_COMPLETE = 0.2f;
    public override void Begin()
    {
        base.Begin();
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        _cameraEffects.ShakeExplosion();
        await UniTask.WaitForSeconds(SECONDS_BEFORE_COMPLETE);
        await base.BeforeCompleteAsync();
    }
}
