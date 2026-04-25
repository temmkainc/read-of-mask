using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelBase : MonoBehaviour
{
    public event Action OnAllConditionsMet;
    public event Action OnLevelComplete;

    [SerializeField] private GameObject[] _objectsToDeactivateOnFinish;
    [SerializeField] private LevelDoor _levelDoor;
    [SerializeField] private LevelDoor _levelDoorToClose;

    private List<ILevelCompleteCondition> _conditions = new();
    private int _conditionsMet;


    private void Awake()
    {
        GetComponents(_conditions);
    }

    public virtual void Begin()
    {
        gameObject.SetActive(true);
        SubscribeToConditions();
    }

    protected virtual async UniTask BeforeCompleteAsync()
    {
        await UniTask.Yield();
        UnsubscribeFromConditions();
        DeactivateObjects();
        Complete();
    }

    public void DeactivateLevel()
    {
        gameObject.SetActive(false);
    }

    private void DeactivateObjects()
    {
        foreach (var obj in _objectsToDeactivateOnFinish)
        {
            obj.SetActive(false);
        }
    }

    private void SubscribeToConditions()
    {
        _conditionsMet = 0;
        foreach (var condition in _conditions)
        {
            condition.OnConditionMet += On_ConditionMet;
        }
    }

    private void UnsubscribeFromConditions()
    {
        foreach (var condition in _conditions)
        {
            condition.OnConditionMet -= On_ConditionMet;
        }
    }

    private void On_ConditionMet()
    {
        _conditionsMet++;
        if (_conditionsMet < _conditions.Count)
            return;
        
        OnAllConditionsMet?.Invoke();
        BeforeCompleteAsync().Forget();
    }

    protected virtual void Complete()
    {
        OnLevelComplete?.Invoke();
        
        if(_levelDoor == null)
            return;

        _levelDoor.Open();
    }
}
