using UnityEngine;

public class RockPaperScissorsMenuItem : InGameMenuItemBase
{
    public RockPaperScissorsChoice Choice;
    public RockPaperScissorsMenuController Controller { get; set; }

    public override void OnFocus(bool isFocused, bool playSound = true)
    {
        base.OnFocus(isFocused, false);
        transform.localScale = isFocused ? Vector3.one * 1.2f : Vector3.one;
    }

    public override void OnSubmit(bool playSound = false)
    {
        base.OnSubmit(playSound);
        Controller.OnPlayerChose(Choice);
    }
}