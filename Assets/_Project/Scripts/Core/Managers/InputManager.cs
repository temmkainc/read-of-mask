using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : IDisposable
{
    [Header("Player Map Actions")]
    public InputAction InteractAction => _actions.Player.Interact;
    public InputAction ShowMiddleFingerAction => _actions.Player.ShowMiddleFinger;
    public InputAction ShowPointingFingerAction => _actions.Player.ShowPointingFinger;
    public InputAction OpenDiaryAction => _actions.Player.OpenDiary;
    public InputAction PlayerLookAction => _actions.Player.Look;
    public InputAction PlayerMoveAction => _actions.Player.Move;
    public InputAction PlayerSprintAction => _actions.Player.Sprint;
    public InputAction PlayerJumpAction => _actions.Player.Jump;
    public InputAction PlayerGrabAction => _actions.Player.Grab;
    public InputAction PlayerThrowAction => _actions.Player.Throw;
    [Header("Global Map Actions")]
    public InputAction ToggleMaskAction => _actions.Global.ToggleMask;
    [Header("Diary Map Actions")]
    public InputAction CloseDiaryAction => _actions.Diary.Close;
    [Header("Gaming Map Actions")]
    public InputAction StopGamingAction => _actions.Gaming.Stop;
    public InputAction GamingDirectionAction => _actions.Gaming.Input;
    public InputAction GamingLookAction => _actions.Gaming.Look;
    public InputAction GamingActionAction => _actions.Gaming.Action;
    public InputAction GamingPauseAction => _actions.Gaming.Pause;
    [Header("LookCloser Map Actions")]
    public InputAction StopLookCloserAction => _actions.LookCloser.Stop;
    public InputAction LookCloserLookAction => _actions.LookCloser.Look;


    public ActionMapType CurrentMap { get; private set; } = ActionMapType.Player;
    public event Action<ActionMapType> OnActionMapChanged;

    public enum ActionMapType { Player = 0, Diary = 1, Gaming = 2, LookCloser = 3,}

    private InputSystem_Actions _actions;

    public InputManager()
    {
        _actions = new InputSystem_Actions();
        _actions.Enable();
        _actions.Player.Enable();
        _actions.Diary.Disable();
        _actions.Gaming.Disable();
        _actions.LookCloser.Disable();
        SetCursorLockState(CursorLockMode.Locked);
    }
    public void Dispose()
    {
        _actions.Dispose();
    }
    public void SetCursorLockState(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
    }

    public void SwitchActionMap(ActionMapType mapType)
    {
        if (CurrentMap == mapType) return;

        FindActionMapByType(CurrentMap).Disable();
        CurrentMap = mapType;
        FindActionMapByType(CurrentMap).Enable();
        OnActionMapChanged?.Invoke(CurrentMap);
    }

    private InputActionMap FindActionMapByType(ActionMapType mapType)
    {
        return mapType switch
        {
            ActionMapType.Player => _actions.Player.Get(),
            ActionMapType.Diary => _actions.Diary.Get(),
            ActionMapType.Gaming => _actions.Gaming.Get(),
            ActionMapType.LookCloser => _actions.LookCloser.Get(),
            _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
        };
    }
}