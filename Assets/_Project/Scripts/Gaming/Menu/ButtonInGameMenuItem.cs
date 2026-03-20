using UnityEngine;
using UnityEngine.UI;

public class ButtonInGameMenuItem : InGameMenuItemBase
{
    [SerializeField] private Button _button;

    public override void OnFocus(bool focused)
    {
        transform.localScale = focused ? Vector3.one * 1.1f : Vector3.one;
    }

    public override void OnLeft() { }
    public override void OnRight() { }

    public override void OnSubmit()
    {
        _button.onClick.Invoke();
    }
}