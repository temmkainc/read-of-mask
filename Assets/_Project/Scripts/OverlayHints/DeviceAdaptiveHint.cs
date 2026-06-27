using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DeviceAdaptiveHint : MonoBehaviour
{
    [SerializeField] private TMP_Text _hintText;
    [SerializeField] private string _keyboardKey;
    [SerializeField] private string _xboxSpriteName;
    [SerializeField] private string _playstationSpriteName;
    [SerializeField] private TMP_SpriteAsset _xboxSpriteAsset;
    [SerializeField] private TMP_SpriteAsset _playstationSpriteAsset;

    public string Label { get; set; }

    private void Awake()
    {
        Label = _hintText.text;
    }

    private void OnEnable()
    {
        InputDeviceTracker.OnDeviceChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        InputDeviceTracker.OnDeviceChanged -= Refresh;
    }

    public void Refresh()
    {
        string hint = DetectDeviceType() switch
        {
            DeviceType.Xbox        => $"<sprite=\"{_xboxSpriteAsset.name}\" name=\"{_xboxSpriteName}\"> {Label}",
            DeviceType.PlayStation => $"<sprite=\"{_playstationSpriteAsset.name}\" name=\"{_playstationSpriteName}\"> {Label}",
            _                      => $"[<b>{_keyboardKey}</b>] {Label}"
        };
        _hintText.text = hint;
    }

    private DeviceType DetectDeviceType()
    {
        var device = InputDeviceTracker.LastUsedDevice;
        if (device == null || device is Keyboard || device is Mouse)
            return DeviceType.Keyboard;
        if (device is Gamepad gamepad)
        {
            string name   = gamepad.name.ToLower();
            string layout = gamepad.layout?.ToLower() ?? "";
            if (name.Contains("dualsense") || name.Contains("dualshock") || name.Contains("playstation")
                || layout.Contains("dualsense") || layout.Contains("dualshock"))
                return DeviceType.PlayStation;
            return DeviceType.Xbox;
        }
        return DeviceType.Keyboard;
    }

    private enum DeviceType { Keyboard, Xbox, PlayStation }
}