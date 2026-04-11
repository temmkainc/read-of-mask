using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [Header("Audio Collections")]
    [SerializeField] private List<VoicelineData> _voicelines;
    [SerializeField] private List<MusicData> _music;
    [SerializeField] private List<SFXData> _sfx;

    private Dictionary<string, VoicelineData> _voiceCache;
    private Dictionary<string, MusicData> _musicCache;
    private Dictionary<string, SFXData> _sfxCache;

    private void OnEnable()
    {
        BuildCaches();
    }

    private void BuildCaches()
    {
        _voiceCache = new Dictionary<string, VoicelineData>();
        _musicCache = new Dictionary<string, MusicData>();
        _sfxCache = new Dictionary<string, SFXData>();

        foreach (var v in _voicelines) _voiceCache[v.Id] = v;
        foreach (var m in _music) _musicCache[m.Id] = m;
        foreach (var s in _sfx) _sfxCache[s.Id] = s;
    }

    public VoicelineData GetVoiceline(string id)
        => _voiceCache.TryGetValue(id, out var v) ? v : null;

    public MusicData GetMusic(string id)
        => _musicCache.TryGetValue(id, out var m) ? m : null;

    public SFXData GetSfx(string id)
        => _sfxCache.TryGetValue(id, out var s) ? s : null;
}

public static class Voicelines
{
    public const string HeadmistressIntroSpeech = "headmistress_intro_speech";
    public const string HeadmistressTransitionToTheOffice = "headmistress_transition_to_the_office";
    public const string HeadmistressYouAreDear = "headmistress_you_are_dear";
}
public static class MusicTracks
{
    public const string Level00Intro = "level00_intro_music";
    public const string Level01Music = "level01_music";
}
public static class SfxTracks
{
    public const string VoicelineInterruptionSound = "voiceline_interrupt";
}