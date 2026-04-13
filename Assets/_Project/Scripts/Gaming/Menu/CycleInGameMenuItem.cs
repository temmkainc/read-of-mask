using UnityEngine;
using TMPro;
using System;

public class CycleInGameMenuItem : InGameMenuItemBase, IInGameMenuItem
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_Text _valueText;

    private string[] _options;
    private int _currentIndex;
    private int _confirmedIndex;
    private bool _isEditing;

    public string CurrentValue => _options[_confirmedIndex];
    public Action<string> OnValueChanged { get; set; }

    public override void OnFocus(bool focused, bool playSound = true)
    {
        base.OnFocus(focused, playSound);
        transform.localScale = focused ? Vector3.one * 1.1f : Vector3.one;

        if (!focused)
        {
            _isEditing = false;
            _currentIndex = _confirmedIndex;
        }

        UpdateDisplay();
    }

    public override void OnLeft()
    {
        if (!_isEditing) return;
        
        base.OnLeft();
        _currentIndex--;
        if (_currentIndex < 0) _currentIndex = _options.Length - 1;
        UpdateDisplay();
    }

    public override void OnRight()
    {
        if (!_isEditing) return;

        base.OnRight();
        _currentIndex++;
        if (_currentIndex >= _options.Length) _currentIndex = 0;
        UpdateDisplay();
    }

    public override void OnSubmit()
    {
        base.OnSubmit();
        if (_isEditing)
        {
            _isEditing = false;
            _confirmedIndex = _currentIndex;
            OnValueChanged?.Invoke(CurrentValue);
        }
        else
        {
            _isEditing = true;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        _valueText.text = _isEditing
            ? $"<{_options[_currentIndex]}>"
            : _options[_currentIndex];
        _valueText.color = new Color(
            _valueText.color.r,
            _valueText.color.g,
            _valueText.color.b,
            _isEditing ? 0.5f : 1f
        );
    }

    public void Initialize(string[] options, string startValue, Action<string> callback)
    {
        _options = options;
        _confirmedIndex = Mathf.Max(0, Array.IndexOf(options, startValue));
        _currentIndex = _confirmedIndex;
        _valueText.text = _options[_currentIndex];
        OnValueChanged = callback;
    }
}