using UnityEngine;

public abstract class InGameMenuItemBase : MonoBehaviour, IInGameMenuItem
{
    protected const float SOUND_VOLUME_SCALE = 0.5f;
    public virtual void OnFocus(bool focused, bool playSound = true)
    {
        if (this == null) return;
        if (!focused || playSound == false)
            return;

        AudioManager.Instance.PlaySFX(SfxClips.MenuHover, SOUND_VOLUME_SCALE);
    }
    public virtual void OnLeft()
    {
        if (this == null) return;
        AudioManager.Instance.PlaySFX(SfxClips.MenuHover, SOUND_VOLUME_SCALE);
    }
    public virtual void OnRight()
    {
        if (this == null) return;
        AudioManager.Instance.PlaySFX(SfxClips.MenuHover, SOUND_VOLUME_SCALE);
    }
    public virtual void OnSubmit(bool playSound = true)
    {
        if (this == null) return;
        if(!playSound)
            return;
        AudioManager.Instance.PlaySFX(SfxClips.MenuConfirm, SOUND_VOLUME_SCALE);
    }
    
    public void SetVisible(bool visible)
    {
        if (this == null) return;
        gameObject.SetActive(visible);
    }
}
