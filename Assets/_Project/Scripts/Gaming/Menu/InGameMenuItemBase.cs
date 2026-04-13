using UnityEngine;

public abstract class InGameMenuItemBase : MonoBehaviour, IInGameMenuItem
{
    private const float SOUND_VOLUME_SCALE = 0.5f;
    public virtual void OnFocus(bool focused, bool playSound = true)
    {
        if (!focused || playSound == false)
            return;

        AudioManager.Instance.PlaySFX(SfxTracks.MenuHover, SOUND_VOLUME_SCALE);
    }
    public virtual void OnLeft()
    {
        AudioManager.Instance.PlaySFX(SfxTracks.MenuHover, SOUND_VOLUME_SCALE);
    }
    public virtual void OnRight()
    {
        AudioManager.Instance.PlaySFX(SfxTracks.MenuHover, SOUND_VOLUME_SCALE);
    }
    public virtual void OnSubmit()
    {
        AudioManager.Instance.PlaySFX(SfxTracks.MenuConfirm, SOUND_VOLUME_SCALE);
    }
    
    public void SetVisible(bool visible) => gameObject.SetActive(visible);
}