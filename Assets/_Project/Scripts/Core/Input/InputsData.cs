using UnityEngine;
using UnityEngine.InputSystem;

public class InputsData : PersistentRuntimeData<InputsData>
{
    private InputActionMap _currentActionMap;
    public InputActionMap CurrentActionMap
    {
        get => _currentActionMap;
        set
        {
            if (_currentActionMap == value) return;

            _currentActionMap?.Disable();
            Debug.Log($"Trying to disable {_currentActionMap}, disabled = {!_currentActionMap.enabled}");
            _currentActionMap = value;
            _currentActionMap?.Enable();
            Debug.Log($"Trying to enable {_currentActionMap}, disabled = {_currentActionMap.enabled}");
        }
    }

    public enum ActionMapType
    {
        Player = 0,
        Diary = 1,
    }
}
