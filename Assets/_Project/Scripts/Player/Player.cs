using UnityEngine;
using Zenject;

public class Player : MonoBehaviour
{
    public PlayerGrabbing Grabbing { get; private set; }

    [Inject] private PlayerLookTarget _lookTarget;

    void Start()
    {
        Grabbing = GetComponent<PlayerGrabbing>();
    }
    private void Update()
    {
        _lookTarget.Tick();
    }
}
