using UnityEngine;

public class Gramophone : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private string _musicId;
    private bool _hasInteracted;
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding && !_hasInteracted;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding || _hasInteracted)
            return;

        _hasInteracted = true;
        AudioManager.Instance.PlayMusic(_musicId, 0.5f, _audioSource);
    }
}
