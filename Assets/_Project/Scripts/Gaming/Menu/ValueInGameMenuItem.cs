using UnityEngine;
using UnityEngine.UI;
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

    public float CurrentValue => _current;
    public float MinValue => _min;
    public float MaxValue => _max;
    public float Step => _step;
    public Action<float> OnValueChanged { get; set; }

    public override void OnFocus(bool focused)
    {
        transform.localScale = focused ? Vector3.one * 1.1f : Vector3.one;
    }

    public override void OnLeft()
    {
        SetValue(_current - _step);
    }

    public override void OnRight()
    {
        SetValue(_current + _step);
    }

    public override void OnSubmit()
    {
    }

    private void SetValue(float value)
    {
        _current = Mathf.Clamp(value, _min, _max);
        _valueText.text = _current.ToString("0.00");
        OnValueChanged?.Invoke(_current);
    }

    public void Initialize(float startValue, Action<float> callback)
    {
        _current = Mathf.Clamp(startValue, _min, _max);
        _valueText.text = _current.ToString("0.00");
        OnValueChanged = callback;
    }
}