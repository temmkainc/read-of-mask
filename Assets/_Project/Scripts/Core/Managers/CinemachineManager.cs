using Unity.Cinemachine;
using UnityEngine;

public class CinemachineManager { 
    public CinemachineCamera CurrentCamera { get; private set; }

    public void SwitchCamera(CinemachineCamera camera)
    {
        DeactivatePreviousCamera();
        CurrentCamera = camera;
        CurrentCamera.Priority = 10;
    }

    private void DeactivatePreviousCamera()
    {
        if (CurrentCamera == null)
            return;

        CurrentCamera.Priority = -1;
    }
}
