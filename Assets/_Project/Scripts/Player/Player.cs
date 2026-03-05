using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerGrabbing Grabbing { get; private set; }
    void Start()
    {
        Grabbing = GetComponent<PlayerGrabbing>();
    }
}
