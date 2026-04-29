using UnityEngine;

/// <summary>
/// Represents a single button on the domophone numpad.
/// ButtonType drives behaviour: Digit (0-9), Backspace, or Enter.
/// </summary>
public class DomoPhoneMenuItem : InGameMenuItemBase
{
    public enum ButtonType { Digit, Backspace, Enter }

    [SerializeField] private ButtonType _buttonType = ButtonType.Digit;
    [SerializeField] private int _digit;

    public DomoPhoneMenuController Controller { get; set; }

    public override void OnFocus(bool focused, bool playSound = true)
    {
        base.OnFocus(focused, false);
        if (playSound && focused)
        {
            AudioManager.Instance.PlaySFX(SfxClips.KeypadFocus, volumeScale: 0.2f);
        }
        if (focused){
            transform.localScale = Vector3.one * 1.1f; 
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public override void OnSubmit(bool playSound = true)
    {
        base.OnSubmit(false); 
        if(playSound){
            AudioManager.Instance.PlaySFX(SfxClips.KeypadEnter);
        }
        switch (_buttonType)
        {
            case ButtonType.Digit:
                Controller.InputDigit(_digit);
                break;
            case ButtonType.Backspace:
                Controller.InputBackspace();
                break;
            case ButtonType.Enter:
                Controller.InputEnter();
                break;
        }
    }
}