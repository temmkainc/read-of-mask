using UnityEngine;

public class SwitchInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;
    private bool _isOn = false;
    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding) 
            return;

        _isOn = !_isOn;
        AudioManager.Instance.PlaySFX(_isOn ? SfxClips.SwitchOn : SfxClips.SwitchOff);
    }
}
