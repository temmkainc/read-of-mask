using Cysharp.Threading.Tasks;
using UnityEngine;

public class RadioInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private VoicelineData[] _voicelines; 

    private bool _isPlaying = false;
    
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding)
            return;

        if (_isPlaying)        {
            _audioSource.Stop();
        }
        else
        {
            AudioManager.Instance.PlayVoicelineAsync(_voicelines[0].Id, _audioSource).Forget();
        }
        _isPlaying = !_isPlaying;
    }
}
