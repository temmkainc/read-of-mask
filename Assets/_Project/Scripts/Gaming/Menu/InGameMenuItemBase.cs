using UnityEngine;

public abstract class InGameMenuItemBase : MonoBehaviour, IInGameMenuItem
{
    public abstract void OnFocus(bool focused);
    public abstract void OnLeft();
    public abstract void OnRight();
    public abstract void OnSubmit();
}