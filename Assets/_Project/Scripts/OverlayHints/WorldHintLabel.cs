using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldHintLabel : MonoBehaviour
{
    [SerializeField] private DeviceAdaptiveHint _deviceAdaptiveHint;

    // how far in front of a surface between it and the camera the label is brought
    private const float FRONT_MARGIN = 0.3f;

    private Transform _target;
    private Vector3   _offset;
    private Camera    _camera;

    private void Awake()
    {
        _camera = Camera.main;

        if (_deviceAdaptiveHint == null)
            _deviceAdaptiveHint = GetComponentInChildren<DeviceAdaptiveHint>();

        Hide();
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        transform.position = _target.position + _offset;
        KeepInFrontOfSurfaces();

        Vector3 dir = transform.position - _camera.transform.position;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    // The label is drawn in the world, so anything between it and the camera hides it - the door it
    // sits in the middle of, the cabinet its target's pivot is inside. Bring it forward to just in
    // front of the nearest such surface; it barely changes how big it looks.
    private void KeepInFrontOfSurfaces()
    {
        Vector3 eye = _camera.transform.position;
        Vector3 toLabel = transform.position - eye;
        float distance = toLabel.magnitude;
        if (distance < 0.01f)
            return;

        Vector3 direction = toLabel / distance;
        if (Physics.Raycast(eye, direction, out var hit, distance, ~0, QueryTriggerInteraction.Ignore))
            transform.position = eye + direction * Mathf.Max(hit.distance - FRONT_MARGIN, 0.1f);
    }

    public void Show(Transform target, string hintLabel, float xOffset, float yOffset, float zOffset)
    {
        _target  = target;
        _offset  = new Vector3(xOffset, yOffset, zOffset);
        _deviceAdaptiveHint.Label = hintLabel;
        _deviceAdaptiveHint.Refresh();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _target = null;
        gameObject.SetActive(false);
    }
}