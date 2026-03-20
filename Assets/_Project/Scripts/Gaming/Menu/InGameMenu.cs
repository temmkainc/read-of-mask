using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameMenu
{
    private readonly List<IInGameMenuItem> _items;
    private int _focusedIndex = 0;
    private bool _isActive = false;
    private readonly InputAction _directionInputAction;
    private readonly InputAction _actionInputAction;

    public Action<int> OnItemSubmitted;

    public InGameMenu(List<IInGameMenuItem> items, InputAction directionInputAction, InputAction actionInputAction)
    {
        _items = items;
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

        Vector2 input = ctx.ReadValue<Vector2>();

        if (input.y > 0.5f) MoveFocusUp();
        else if (input.y < -0.5f) MoveFocusDown();
        else if (input.x < -0.5f) _items[_focusedIndex].OnLeft();
        else if (input.x > 0.5f) _items[_focusedIndex].OnRight();
    }

    private void MoveFocusUp()
    {
        _focusedIndex--;
        if (_focusedIndex < 0) _focusedIndex = _items.Count - 1;
        UpdateFocus();
    }

    private void MoveFocusDown()
    {
        _focusedIndex++;
        if (_focusedIndex >= _items.Count) _focusedIndex = 0;
        UpdateFocus();
    }

    private void UpdateFocus()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].OnFocus(i == _focusedIndex);
        }
    }

    private void ClearFocus()
    {
        foreach (var btn in _items)
        {
            btn.OnFocus(false);
        }
    }

    public void On_ActionInputPerformed(InputAction.CallbackContext ctx)
    {
        if (!_isActive) return;

        _items[_focusedIndex].OnSubmit();
        OnItemSubmitted?.Invoke(_focusedIndex);
    }
}