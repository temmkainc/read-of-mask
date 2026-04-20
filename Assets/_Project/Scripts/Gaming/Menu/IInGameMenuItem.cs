using UnityEngine;

public interface IInGameMenuItem
{
    void OnFocus(bool focused, bool playSound = true);
    void OnLeft();
    void OnRight();
    void OnSubmit(bool playSound = true);
    void SetVisible(bool visible);
}
