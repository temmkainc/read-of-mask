using Cysharp.Threading.Tasks;
using UnityEngine;

public class Level01 : LevelBase
{
    public override void Begin()
    {
        base.Begin();
        Debug.Log("Level 01 activated!");
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 01 deactivated!");
    }
}
