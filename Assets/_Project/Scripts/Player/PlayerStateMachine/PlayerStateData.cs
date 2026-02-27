using Unity.Cinemachine;
using UnityEngine;

[System.Serializable]
public class PlayerStateData
{
    [field: SerializeField] public InputsData.ActionMapType ActionMapType { get; private set; }
    [field: SerializeField] public CinemachineCamera Camera { get; private set; }
    [field: SerializeField] public CursorLockMode CursorLockMode { get; private set; }
}
