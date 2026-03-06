using UnityEngine;

public interface IGrabbable
{
    bool IsGrabbed { get; }
    void Grab(Player player, Transform holdPoint);
    void Release(Vector3 throwForce);
    float WallOffset { get; }
    float MinCameraDistance { get; }
    float ThrowForce { get; }
}