using Unity.Cinemachine;
using UnityEngine;

public class InteractionCinemachineCamera : MonoBehaviour
{
    public CinemachineCamera CinemachineCamera {  get; private set; }

    private void Awake()
    {
        CinemachineCamera = GetComponent<CinemachineCamera>();
    }
}
