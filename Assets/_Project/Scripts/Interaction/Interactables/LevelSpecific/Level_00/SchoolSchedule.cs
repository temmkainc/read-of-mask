using Cysharp.Threading.Tasks;
using UnityEngine;

public class SchoolSchedule : LookCloserInteractableBase
{
    private bool _hasBeenInteractedWith;
    [SerializeField] private AudioSource _audioSource;

    public override void Interact(Player player = null)
    {
        base.Interact(player);
        if (_hasBeenInteractedWith)
            return;
        
        AudioManager.Instance.PlayVoicelineAsync(Voicelines.SchoolScheduleInteraction, _audioSource).Forget();
        _hasBeenInteractedWith = true;
    }
}
