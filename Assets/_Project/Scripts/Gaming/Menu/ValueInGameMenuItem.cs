using UnityEngine;
using System;
using TMPro;

public class ValueInGameMenuItem : InGameMenuItemBase, IValueInGameMenuItem
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private float _min = 0.1f;
    [SerializeField] private float _max = 3f;
    [SerializeField] private float _step = 0.1f;
    [SerializeField] private float _current = 1f;

    private float _confirmed;
    private bool _isEditing;

    public float CurrentValue => _confirmed;
    public float MinValue => _min;
    public float MaxValue => _max;
    public float Step => _step;
    public Action<float> OnValueChanged { get; set; }

    public override void OnFocus(bool focused, bool playSound = true)
    {
        base.OnFocus(focused, playSound);
        transform.localScale = focused ? Vector3.one * 1.1f : Vector3.one;

        if (!focused)
        {
            _isEditing = false;
            _current = _confirmed;
        }

        UpdateDisplay();
    }

    public override void OnLeft()
    {
        if (!_isEditing) return;

        base.OnLeft();
        _current = Mathf.Clamp(_current - _step, _min, _max);
        UpdateDisplay();
    }

    public override void OnRight()
    {
        if (!_isEditing) return;

        base.OnRight();
        _current = Mathf.Clamp(_current + _step, _min, _max);
        UpdateDisplay();
    }

    public override void OnSubmit(bool playSound = true)
    {
        base.OnSubmit(playSound);
        if (_isEditing)
        {
            _isEditing = false;
            _confirmed = _current;
            OnValueChanged?.Invoke(_confirmed);
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
            ? $"<{_current:0.0}>"
            : _current.ToString("0.0");
        _valueText.color = new Color(
            _valueText.color.r,
            _valueText.color.g,
            _valueText.color.b,
            _isEditing ? 0.5f : 1f
        );
    }

    public void Initialize(float startValue, Action<float> callback)
    {
        _confirmed = Mathf.Clamp(startValue, _min, _max);
        _current = _confirmed;
        _valueText.text = _current.ToString("0.0");
        OnValueChanged = callback;
    }
}