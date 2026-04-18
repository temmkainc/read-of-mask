using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LevelDoor : MonoBehaviour
{
    private Animator _animator;
    private static readonly int OpenTriggerHash = Animator.StringToHash("Open");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }


    public void Open()
    {
        _animator.SetTrigger(OpenTriggerHash);
    }
}
