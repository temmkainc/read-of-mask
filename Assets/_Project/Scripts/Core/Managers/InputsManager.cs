using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputsManager : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool ShowMiddleFingerPressed { get; private set; }
    public bool ShowPointingFingerPressed { get; private set; }
    public bool SprintHeld { get; private set; }

    [Header("Player Input Actions")]
    public InputAction ShowMiddleFingerAction => _playerInputActions.Player.ShowMiddleFinger;
    public InputAction ShowPointingFingerAction => _playerInputActions.Player.ShowPointingFinger;
    public InputAction ToggleMaskAction => _playerInputActions.Player.ToggleMask;
    public InputAction OpenDiaryAction => _playerInputActions.Player.OpenDiary;

    [Header("Diary Input Actions")]
    public InputAction CloseDiaryAction => _playerInputActions.Diary.Close;

    private PlayerInputActions _playerInputActions;
    private InputsData _inputsData => InputsData.Instance;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();
        //_inputsData.CurrentActionMap = _playerInputActions.Player;
        //_inputsData.CurrentActionMap.Enable();
        SetCursorLockState(CursorLockMode.Locked);
    }

    private void OnDestroy()
    {
        _playerInputActions.Dispose();
    }

    public void SwitchActionMap(InputsData.ActionMapType mapType)
    {
        _playerInputActions.Player.Disable();
        _playerInputActions.Diary.Disable();

        var newMap = FindActionMapByType(mapType);
        _inputsData.CurrentActionMap = newMap; 
    }

    public void SetCursorLockState(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
    }

    private InputActionMap FindActionMapByType(InputsData.ActionMapType mapType)
    {
        return mapType switch
        {
            InputsData.ActionMapType.Player => _playerInputActions.Player,
            InputsData.ActionMapType.Diary => _playerInputActions.Diary,
            _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
        };
    }

    private void Update()
    {
        UpdatePlayerMovement();
    }

    private void UpdatePlayerMovement()
    {
        if (_inputsData.CurrentActionMap?.name != "Player")
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            JumpPressed = false;
            SprintHeld = false;
            ShowMiddleFingerPressed = false;
            ShowPointingFingerPressed = false;
            return;
        }

        MoveInput = _playerInputActions.Player.Move.ReadValue<Vector2>();
        LookInput = _playerInputActions.Player.Look.ReadValue<Vector2>();
        JumpPressed = _playerInputActions.Player.Jump.triggered;
        SprintHeld = _playerInputActions.Player.Sprint.IsPressed();
        ShowMiddleFingerPressed = _playerInputActions.Player.ShowMiddleFinger.IsPressed();
        ShowPointingFingerPressed = _playerInputActions.Player.ShowPointingFinger.IsPressed();
    }
}