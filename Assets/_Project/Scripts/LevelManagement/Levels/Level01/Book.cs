using UnityEngine;
using Cysharp.Threading.Tasks;

public class Book : GrabbableWithPredefinedSlotsObject
{
    [SerializeField] private AudioSource _audioSource;
    private bool _hasBeenGrabbedOnce = false;
    private bool _hasBeenReleasedOnce = false;
    public override void Grab(Player player, Transform holdPoint)
    {
        if (!_hasBeenGrabbedOnce)
        {
            AudioManager.Instance.PlayVoicelineAsync(Voicelines.BookGrabReaction, source: _audioSource).Forget();
            _hasBeenGrabbedOnce = true;
        }
        base.Grab(player, holdPoint);
    }

    public override void Release(Vector3 throwForce, bool isExternal)
    {
        if (!_hasBeenReleasedOnce && !isExternal)
        {
            AudioManager.Instance.PlayVoicelineAsync(Voicelines.BookOnFloorReaction, source: _audioSource).Forget();
            _hasBeenReleasedOnce = true;
        }
        base.Release(throwForce, isExternal);
    }
}
