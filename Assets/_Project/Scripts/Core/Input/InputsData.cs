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
            _currentActionMap = value;
            _currentActionMap?.Enable();
        }
    }

    public enum ActionMapType
    {
        Player = 0,
        Diary = 1,
    }
}
