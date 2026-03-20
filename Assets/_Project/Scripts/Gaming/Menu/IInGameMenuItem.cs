using UnityEngine;

public interface IInGameMenuItem
{
    void OnFocus(bool focused);
    void OnLeft();
    void OnRight();
    void OnSubmit();
}
