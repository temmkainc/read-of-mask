using System;
using UnityEngine;

/// <summary>
/// Level-complete condition satisfied once CubeToEndTheDEMO's ending sequence finishes.
/// Place this on the same GameObject as this level's LevelBase (same as the other
/// LevelCompleteConditions), and assign the cube in the scene.
/// </summary>
public class EndSequenceCompleteCondition : MonoBehaviour, ILevelCompleteCondition
{
    [SerializeField] private CubeToEndTheDEMO _cube;

    public event Action OnConditionMet;

    private void Awake()
    {
        _cube.OnEndSequenceComplete += On_EndSequenceComplete;
    }

    private void On_EndSequenceComplete()
    {
        OnConditionMet?.Invoke();
    }
}
