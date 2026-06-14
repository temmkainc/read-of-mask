using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using Newtonsoft.Json;
using Zenject;

public class GameOptions : IInitializable
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "config.json");

    private OptionsData _data;

    [InjectOptional] private AudioManager _audioManager;
    [InjectOptional] private Volume _globalVolume;

    private MotionBlur _motionBlur;

    public GameOptions()
    {
        Load();
    }

    void IInitializable.Initialize()
    {
        if (_globalVolume != null && _globalVolume.profile.TryGet(out _motionBlur))
        {
            // motionBlur found
        }
        else
        {
            Debug.LogWarning("[GameOptions] MotionBlur override not found on the global Volume profile.");
        }
        Apply();
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public float Sensitivity
    {
        get => _data.Sensitivity;
        set { _data.Sensitivity = value; Save(); }
    }

    public float MasterVolume
    {
        get => _data.MasterVolume;
        set { _data.MasterVolume = value; _audioManager.SetMasterVolume(value); Save(); }
    }

    public float MusicVolume
    {
        get => _data.MusicVolume;
        set { _data.MusicVolume = value; _audioManager.SetMusicVolume(value); Save(); }
    }

    public bool Fullscreen
    {
        get => _data.Fullscreen;
        set { _data.Fullscreen = value; Screen.fullScreen = value; Save(); }
    }

    public string Resolution
    {
        get => _data.Resolution;
        set { _data.Resolution = value; ApplyResolution(value); Save(); }
    }

    public bool MotionBlur
    {
        get => _data.MotionBlur;
        set { _data.MotionBlur = value; ApplyMotionBlur(value); Save(); }
    }

    // -------------------------------------------------------------------------
    // Apply
    // -------------------------------------------------------------------------


    public void Apply()
    {
        Screen.fullScreen = _data.Fullscreen;
        ApplyResolution(_data.Resolution);
        _audioManager?.SetMasterVolume(_data.MasterVolume);
        _audioManager?.SetMusicVolume(_data.MusicVolume);
        ApplyMotionBlur(_data.MotionBlur);
    }
    private void ApplyMotionBlur(bool enabled)
    {
        if (_motionBlur == null) return;
        _motionBlur.active = enabled;
    }

    private void ApplyResolution(string resolution)
    {
        var parts = resolution.Split('x');
        if (parts.Length == 2
            && int.TryParse(parts[0], out int w)
            && int.TryParse(parts[1], out int h))
        {
            Screen.SetResolution(w, h, Screen.fullScreen);
        }
    }

    // -------------------------------------------------------------------------
    // Persistence
    // -------------------------------------------------------------------------

    private void Load()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            _data = JsonConvert.DeserializeObject<OptionsData>(json);
        }
        else
        {
            _data = new OptionsData();
        }
    }

    private void Save()
    {
        var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    // -------------------------------------------------------------------------
    // Data
    // -------------------------------------------------------------------------

    private class OptionsData
    {
        public float Sensitivity { get; set; } = 1f;
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 0.3f;
        public bool Fullscreen { get; set; } = true;
        public string Resolution { get; set; } = "1920x1080";
        public bool MotionBlur { get; set; } = true;
    }
}