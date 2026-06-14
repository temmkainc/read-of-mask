using UnityEngine;

public class Gramophone : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private string _musicId;
    private bool _isPlaying;
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding)
            return;

        if (!_isPlaying)
        {
            AudioManager.Instance.PlayMusicForSource(_musicId, _audioSource, volumeScale: 0.5f);
        } else
        {
            AudioManager.Instance.StopMusicForSource(_audioSource);
        }

        _isPlaying = !_isPlaying;

    }
}
