using UnityEngine;

/// <summary>
/// Keeps a door locked: trying it plays the "not allowed" hand gesture and it stays shut.
/// Call Unlock() (e.g. from a level script) to let it open again.
/// </summary>
[RequireComponent(typeof(DoorInteractable))]
public class LockedDoor : MonoBehaviour
{
    [SerializeField] private bool _lockedOnStart = true;

    private DoorInteractable _door;

    public bool IsLocked { get; private set; }

    private void Start()
    {
        SetLocked(_lockedOnStart);
    }

    public void Lock() => SetLocked(true);

    public void Unlock() => SetLocked(false);

    private void SetLocked(bool locked)
    {
        if (_door == null)
            _door = GetComponent<DoorInteractable>();

        IsLocked = locked;
        _door.SetDisabled(locked);
    }
}
