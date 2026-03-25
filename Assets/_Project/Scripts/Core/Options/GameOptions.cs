using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class GameOptions
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "config.json");

    private OptionsData _data;


    public GameOptions()
    {
        Load();
        Apply();
    }

    public float Sensitivity
    {
        get => _data.Sensitivity;
        set { _data.Sensitivity = value; Save(); }
    }

    public float MasterVolume
    {
        get => _data.MasterVolume;
        set { _data.MasterVolume = value; Save(); }
    }

    public float MusicVolume
    {
        get => _data.MusicVolume;
        set { _data.MusicVolume = value; Save(); }
    }

    public bool Fullscreen
    {
        get => _data.Fullscreen;
        set { _data.Fullscreen = value; Save(); Screen.fullScreen = value; }
    }

    public string Resolution
    {
        get => _data.Resolution;
        set { _data.Resolution = value; Save(); ApplyResolution(value); }
    }

    public void Apply()
    {
        Screen.fullScreen = _data.Fullscreen;
        ApplyResolution(_data.Resolution);
    }

    private void ApplyResolution(string resolution)
    {
        var parts = resolution.Split('x');
        Screen.SetResolution(int.Parse(parts[0]), int.Parse(parts[1]), Screen.fullScreen);
    }

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

    private class OptionsData
    {
        public float Sensitivity { get; set; } = 1f;
        public float MasterVolume { get; set; } = 10f;
        public float MusicVolume { get; set; } = 10f;
        public bool Fullscreen { get; set; } = true;
        public string Resolution { get; set; } = "1920x1080";
    }
}