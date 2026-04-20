using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

/// <summary>
/// Manages the domophone numpad grid navigation and code entry logic.
/// Grid layout (configurable via inspector):
///   [1][2][3]
///   [4][5][6]
///   [7][8][9]
///   [*][0][#]   (* = Backspace, # = Enter)
///
/// Indices 0-9 = digits 1-9,0  |  10 = Backspace  |  11 = Enter
/// </summary>
public class DomoPhoneMenuController : MonoBehaviour
{
    [Header("Code settings")]
    [SerializeField] private string _correctCode = "1234";
    [SerializeField] private int _codeLength = 4;

    [Header("Grid layout")]
    [SerializeField] private int _columns = 3;
    // Assign in order: 1,2,3,4,5,6,7,8,9,0 then Backspace, Enter
    [SerializeField] private DomoPhoneMenuItem[] _menuItems;

    [Header("References")]
    [SerializeField] private DomoPhoneDisplay _display;

    [Header("Feedback")]
    [SerializeField] private float _errorDisplayDuration = 0.8f;

    [Inject] private InputManager _inputManager;

    public event Action OnCodeCorrect;

    private string _enteredCode = string.Empty;
    private int _focusedIndex = 0;
    private bool _isLocked = false;


    private void Awake()
    {
        for (int i = 0; i < _menuItems.Length; i++)
            _menuItems[i].Controller = this;

        _codeLength = Mathf.Max(1, _codeLength);
    }

    public void Activate()
    {
        _isLocked = false;
        _enteredCode = string.Empty;
        _focusedIndex = 0;
        _display?.Clear();
        UpdateFocus();
        RegisterInputs();
    }

    public void Deactivate()
    {
        UnregisterInputs();
    }


    private void RegisterInputs()
    {
        _inputManager.LookCloserDirectionAction.performed += OnDirection;
        _inputManager.LookCloserActionAction.performed += OnAction;
    }

    private void UnregisterInputs()
    {
        _inputManager.LookCloserDirectionAction.performed -= OnDirection;
        _inputManager.LookCloserActionAction.performed -= OnAction;
    }


    private void Update()
    {
        if (_isLocked) return;
        if (_inputManager.CurrentMap != InputManager.ActionMapType.LookCloser) return;

        for (int d = 0; d <= 9; d++)
        {
            if (Keyboard.current[d == 0 ? Key.Digit0 : (Key)(Key.Digit1 + d - 1)].wasPressedThisFrame)
            {
                InputDigit(d);
                return;
            }
        }

        if (Keyboard.current.backspaceKey.wasPressedThisFrame) InputBackspace();
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            InputEnter();
    }


    private void OnDirection(InputAction.CallbackContext ctx)
    {
        if (_isLocked) return;

        Vector2 dir = ctx.ReadValue<Vector2>();
        int rows = Mathf.CeilToInt((float)_menuItems.Length / _columns);

        int col = _focusedIndex % _columns;
        int row = _focusedIndex / _columns;

        if (dir.x > 0.5f) col = (col + 1) % _columns;
        else if (dir.x < -0.5f) col = (col - 1 + _columns) % _columns;
        else if (dir.y < -0.5f) row = (row + 1) % rows;
        else if (dir.y > 0.5f) row = (row - 1 + rows) % rows;

        int newIndex = row * _columns + col;

        newIndex = Mathf.Clamp(newIndex, 0, _menuItems.Length - 1);

        SetFocus(newIndex);
    }

    private void OnAction(InputAction.CallbackContext ctx)
    {
        if (_isLocked) return;
        _menuItems[_focusedIndex].OnSubmit();
    }

    private void SetFocus(int index)
    {
        _menuItems[_focusedIndex].OnFocus(false);
        _focusedIndex = index;
        _menuItems[_focusedIndex].OnFocus(true);
    }

    private void UpdateFocus()
    {
        for (int i = 0; i < _menuItems.Length; i++)
            _menuItems[i].OnFocus(i == _focusedIndex, playSound: false);
    }


    public void InputDigit(int digit)
    {
        if (_isLocked) return;
        if (_enteredCode.Length >= _codeLength) return;

        _enteredCode += digit.ToString();
        _display?.UpdateDisplay(_enteredCode);
    }

    public void InputBackspace()
    {
        if (_isLocked || _enteredCode.Length == 0) return;

        _enteredCode = _enteredCode[..^1];
        _display?.UpdateDisplay(_enteredCode);
    }

    public void InputEnter()
    {
        if (_isLocked) return;
        if (_enteredCode.Length < _codeLength) return;

        if (_enteredCode == _correctCode)
        {
            StartCoroutine(ShowSuccess());
        }
        else
        {
            StartCoroutine(ShowError());
        }
    }
    private IEnumerator ShowSuccess()
    {
        _isLocked = true;
        _display?.UpdateDisplay("OPEN");

        yield return new WaitForSeconds(_errorDisplayDuration);

        _isLocked = false;
        OnCodeCorrect?.Invoke();
    }

    private IEnumerator ShowError()
    {
        _isLocked = true;
        _display?.UpdateDisplay(_enteredCode, isError: true);

        yield return new WaitForSeconds(_errorDisplayDuration);

        _enteredCode = string.Empty;
        _display?.Clear();
        _isLocked = false;
    }
}