using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInGameMenuItem : InGameMenuItemBase
{
    [SerializeField] private Button _button;
    [SerializeField] private float _focusScale = 1.1f;
    [Tooltip("Optional - the button's text label. Assign this if you want to change it at runtime (e.g. Play/Continue) via SetLabel().")]
    [SerializeField] private TMP_Text _label;

    public override void OnFocus(bool focused, bool playSound = true)
    {
        base.OnFocus(focused, playSound);
        transform.localScale = focused ? Vector3.one * _focusScale : Vector3.one;
    }

    public override void OnLeft() { }
    public override void OnRight() { }

    public override void OnSubmit(bool playSound = true)
    {
        base.OnSubmit(playSound);
        _button.onClick.Invoke();
    }

    /// <summary>Changes this button's visible text, if a label is assigned. Safe no-op otherwise.</summary>
    public void SetLabel(string text)
    {
        if (_label != null)
            _label.text = text;
    }
}
