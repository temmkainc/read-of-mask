using UnityEngine;
using UnityEngine.UI;

public class ButtonInGameMenuItem : InGameMenuItemBase
{
    [SerializeField] private Button _button;
    [SerializeField] private float _focusScale = 1.1f;

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
}