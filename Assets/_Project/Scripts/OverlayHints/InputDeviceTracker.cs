using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceTracker : MonoBehaviour
{
    public static InputDeviceTracker Instance { get; private set; }
    public static event System.Action OnDeviceChanged;

    public static InputDevice LastUsedDevice { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() => InputSystem.onActionChange += OnActionChange;
    private void OnDisable() => InputSystem.onActionChange -= OnActionChange;

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        var action = obj as InputAction;
        var device = action?.activeControl?.device;

        if (device == null || device == LastUsedDevice) return;

        // Treat mouse and keyboard as the same "desktop" device
        if (device is Mouse && LastUsedDevice is Keyboard) return;
        if (device is Keyboard && LastUsedDevice is Mouse) return;

        LastUsedDevice = device;
        OnDeviceChanged?.Invoke();
    }
}