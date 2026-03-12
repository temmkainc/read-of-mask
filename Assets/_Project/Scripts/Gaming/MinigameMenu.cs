using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Generic menu for minigames: navigates up/down and activates buttons.
/// </summary>
public class MinigameMenu
{
    private readonly List<Button> _buttons;
    private int _focusedIndex = 0;
    private bool _isActive = false;
    private readonly InputAction _directionInputAction;
    private readonly InputAction _actionInputAction;

    public Action<int> OnButtonSelected;

    public MinigameMenu(List<Button> buttons, InputAction directionInputAction, InputAction actionInputAction)
    {
        _buttons = buttons;
        _directionInputAction = directionInputAction;
        _actionInputAction = actionInputAction;
        UpdateFocus();
    }

    public void EnterMenu()
    {
        _directionInputAction.performed += On_DirectionInputPerformed;
        _actionInputAction.performed += On_ActionInputPerformed;
        _isActive = true;
        _focusedIndex = 0;
        UpdateFocus();
    }

    public void ExitMenu()
    {
        _directionInputAction.performed -= On_DirectionInputPerformed;
        _actionInputAction.performed -= On_ActionInputPerformed;
        _isActive = false;
        ClearFocus();
    }

    private void On_DirectionInputPerformed(InputAction.CallbackContext ctx)
    {
        if (!_isActive) return;
        if (!ctx.performed) return;

        Vector2 input = ctx.ReadValue<Vector2>();

        if (input.y > 0.5f) MoveFocusUp();
        else if (input.y < -0.5f) MoveFocusDown();
    }

    private void MoveFocusUp()
    {
        _focusedIndex--;
        if (_focusedIndex < 0) _focusedIndex = _buttons.Count - 1;
        UpdateFocus();
    }

    private void MoveFocusDown()
    {
        _focusedIndex++;
        if (_focusedIndex >= _buttons.Count) _focusedIndex = 0;
        UpdateFocus();
    }

    private void UpdateFocus()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            if (i == _focusedIndex)
            {
                _buttons[i].transform.localScale = Vector3.one * 1.3f;
            }
            else
            {
                _buttons[i].transform.localScale = Vector3.one;
            }
        }

        Debug.Log($"Focused button: {_buttons[_focusedIndex].name}");
    }

    private void ClearFocus()
    {
        foreach (var btn in _buttons)
        {
            btn.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Activate the currently focused button
    /// </summary>
    public void On_ActionInputPerformed(InputAction.CallbackContext ctx)
    {
        if (!_isActive) return;
        OnButtonSelected?.Invoke(_focusedIndex);
    }
}