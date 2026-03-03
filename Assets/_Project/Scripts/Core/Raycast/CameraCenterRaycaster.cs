using UnityEngine;

public class CameraCenterRaycaster : IRaycaster
{
    private readonly Camera _camera;
    private readonly float _distance;
    private readonly LayerMask _mask;

    public CameraCenterRaycaster(Camera camera, float distance, LayerMask mask)
    {
        _camera = camera;
        _distance = distance;
        _mask = mask;
    }

    public bool TryHit<T>(out T component) where T : class
    {
        return TryHit(_distance, _mask, out component);
    }

    public bool TryHit<T>(float distance, LayerMask mask, out T component) where T : class
    {
        component = default;

        var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (!Physics.Raycast(ray, out var hit, distance, mask))
            return false;

        return hit.collider.TryGetComponent(out component);
    }
}