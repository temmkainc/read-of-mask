using UnityEngine;

public interface IInGameMenuItem
{
    void OnFocus(bool focused, bool playSound = true);
    void OnLeft();
    void OnRight();
    void OnSubmit();
    void SetVisible(bool visible);
}
