using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// The yard, reached through the dream gate at the end of Level07. The gate seals itself behind
/// the player once Level07 is over, so there's no going back inside.
/// </summary>
public class Level08 : LevelBase
{
    public override void Begin()
    {
        base.Begin();
        Debug.Log("Level 08 started!");
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 08 deactivated!");
    }
}
