using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// The school, reached through the dream gate in the Level08 yard.
/// </summary>
public class Level09 : LevelBase
{
    public override void Begin()
    {
        base.Begin();
        Debug.Log("Level 09 started!");
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 09 deactivated!");
    }
}
