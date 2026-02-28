using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }

    public InputAction ShowMiddleFingerAction => _actions.Player.ShowMiddleFinger;
    public InputAction ShowPointingFingerAction => _actions.Player.ShowPointingFinger;
    public InputAction ToggleMaskAction => _actions.Player.ToggleMask;
    public InputAction OpenDiaryAction => _actions.Player.OpenDiary;
    public InputAction CloseDiaryAction => _actions.Diary.Close;

    public ActionMapType CurrentMap { get; private set; } = ActionMapType.Player;

    public enum ActionMapType { Player = 0, Diary = 1 }

    private InputSystem_Actions _actions;

    private void Awake()
    {
        _actions = new InputSystem_Actions();
        _actions.Enable();
        _actions.Diary.Disable();
        SetCursorLockState(CursorLockMode.Locked);
    }

    private void OnDestroy() => _actions.Dispose();

    public void SwitchActionMap(ActionMapType mapType)
    {
        if (CurrentMap == mapType) return;

        FindActionMapByType(CurrentMap).Disable();
        CurrentMap = mapType;
        FindActionMapByType(CurrentMap).Enable();
    }

    public void SetCursorLockState(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
    }

    private InputActionMap FindActionMapByType(ActionMapType mapType)
    {
        return mapType switch
        {
            ActionMapType.Player => _actions.Player.Get(),
            ActionMapType.Diary => _actions.Diary.Get(),
            _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
        };
    }

    private void Update() => UpdatePlayerMovement();

    private void UpdatePlayerMovement()
    {
        if (CurrentMap != ActionMapType.Player)
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            JumpPressed = false;
            SprintHeld = false;
            return;
        }

        MoveInput = _actions.Player.Move.ReadValue<Vector2>();
        LookInput = _actions.Player.Look.ReadValue<Vector2>();
        JumpPressed = _actions.Player.Jump.triggered;
        SprintHeld = _actions.Player.Sprint.IsPressed();
    }
}