using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// The school, reached through the dream gate in the Level08 yard.
/// </summary>
public class Level09 : LevelBase
{
    private bool _batched;

    public override void Begin()
    {
        base.Begin();
        Debug.Log("Level 09 started!");

        // the generated school is thousands of small pieces - batch the fixed ones once, while the
        // gate's white-out (or the load) hides the moment it takes
        if (!_batched)
        {
            _batched = true;
            int count = StaticGeometryBatcher.Combine(gameObject);
            Debug.Log($"Level 09: static-batched {count} meshes.");
        }
    }

    protected override async UniTask BeforeCompleteAsync()
    {
        await base.BeforeCompleteAsync();
        Debug.Log("Level 09 deactivated!");
    }
}
