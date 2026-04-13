using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameMenu
{
    private readonly List<IInGameMenuItem> _items;
    private readonly int _visibleCount;
    private int _focusedIndex = 0;
    private int _scrollOffset = 0;    
    private bool _isActive = false;
    private readonly InputAction _directionInputAction;
    private readonly InputAction _actionInputAction;

    public Action<int> OnItemSubmitted;
    private bool _suppressFirstFocusSound;

    public InGameMenu(List<IInGameMenuItem> items, InputAction directionInputAction,
                      InputAction actionInputAction, int visibleCount = int.MaxValue)
    {
        _items = items;
        _visibleCount = Mathf.Min(visibleCount, items.Count);
        _directionInputAction = directionInputAction;
        _actionInputAction = actionInputAction;
    }

    public void EnterMenu()
    {
        _directionInputAction.performed += On_DirectionInputPerformed;
        _actionInputAction.performed += On_ActionInputPerformed;
        _isActive = true;
        _focusedIndex = 0;
        _scrollOffset = 0;
        _suppressFirstFocusSound = true;
        RefreshView();
        _suppressFirstFocusSound = false;
    }

    public void ExitMenu(bool clearFocus = true)
    {
        _directionInputAction.performed -= On_DirectionInputPerformed;
        _actionInputAction.performed -= On_ActionInputPerformed;
        _isActive = false;
        if (!clearFocus)
            return;
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

        if (_focusedIndex < 0)
        {
            _focusedIndex = _items.Count - 1;
            _scrollOffset = _items.Count - _visibleCount;
        }
        else if (_focusedIndex < _scrollOffset)
        {
            _scrollOffset--;
        }

        RefreshView();
    }

    private void MoveFocusDown()
    {
        _focusedIndex++;

        if (_focusedIndex >= _items.Count)
        {
            _focusedIndex = 0;
            _scrollOffset = 0;
        }
        else if (_focusedIndex >= _scrollOffset + _visibleCount)
        {
            _scrollOffset++;
        }

        RefreshView();
    }

    private void RefreshView()
    {
        int windowEnd = _scrollOffset + _visibleCount;

        for (int i = 0; i < _items.Count; i++)
        {
            bool inWindow = i >= _scrollOffset && i < windowEnd;
            _items[i].SetVisible(inWindow);
            _items[i].OnFocus(inWindow && i == _focusedIndex, !_suppressFirstFocusSound);
        }
    }

    private void ClearFocus()
    {
        foreach (var item in _items)
        {
            item.OnFocus(false);
        }
    }

    public void On_ActionInputPerformed(InputAction.CallbackContext ctx)
    {
        if (!_isActive) return;

        _items[_focusedIndex].OnSubmit();
        OnItemSubmitted?.Invoke(_focusedIndex);
    }
}