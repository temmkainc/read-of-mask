using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldHintLabel : MonoBehaviour
{
    [SerializeField] private DeviceAdaptiveHint _deviceAdaptiveHint;

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

        Vector3 dir = transform.position - _camera.transform.position;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
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