using Unity.Cinemachine;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _explosionSource;

    public void ShakeExplosion() => _explosionSource.GenerateImpulse();
}