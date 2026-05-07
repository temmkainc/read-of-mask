using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LevelDoor : MonoBehaviour
{
    private Animator _animator;
    private static readonly int OpenTriggerHash = Animator.StringToHash("Open");
    private static readonly int CloseTriggerHash = Animator.StringToHash("Close");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Open()
    {
        _animator.SetTrigger(OpenTriggerHash);
        AudioManager.Instance.PlaySFX(SfxClips.DoorOpen, volumeScale: 0.6f);
    }

    public void Close()
    {
        _animator.SetTrigger(CloseTriggerHash);
        AudioManager.Instance.PlaySFX(SfxClips.DoorClose, volumeScale: 0.6f);
    }
}