using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public enum RockPaperScissorsChoice { Rock, Paper, Scissors }

public class RockPaperScissorsMenuController : MonoBehaviour
{
    [SerializeField] private RockPaperScissorsMenuItem[] _menuItems;
    [SerializeField] private Canvas _canvas;

    [Inject] private InputManager _inputManager;

    public event Action<RockPaperScissorsChoice> OnPlayerSubmitted;

    private int _focusedIndex = 0;
    private bool _isLocked = false;

    private void Awake()
    {
        for (int i = 0; i < _menuItems.Length; i++)
            _menuItems[i].Controller = this;
    }

    public void Activate()
    {
        _canvas.gameObject.SetActive(true);
        _isLocked = false;
        _focusedIndex = 0;
        UpdateFocus();
        _inputManager.LookCloserDirectionAction.performed += OnDirection;
        _inputManager.LookCloserActionAction.performed += OnAction;
    }

    public void Deactivate()
    {    
        _canvas.gameObject.SetActive(false);
        _inputManager.LookCloserDirectionAction.performed -= OnDirection;
        _inputManager.LookCloserActionAction.performed -= OnAction;
    }

    public void Lock() => _isLocked = true;
    public void Unlock() => _isLocked = false;

    private void OnDirection(InputAction.CallbackContext ctx)
    {
        if (_isLocked) return;

        Vector2 dir = ctx.ReadValue<Vector2>();

        if (dir.x > 0.5f || dir.y < -0.5f)
            SetFocus((_focusedIndex + 1) % _menuItems.Length);
        else if (dir.x < -0.5f || dir.y > 0.5f)
            SetFocus((_focusedIndex - 1 + _menuItems.Length) % _menuItems.Length);
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

    public void OnPlayerChose(RockPaperScissorsChoice choice)
    {
        _isLocked = true;
        OnPlayerSubmitted?.Invoke(choice);
    }
}

