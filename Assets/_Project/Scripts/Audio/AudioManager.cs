using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// Central audio manager. Singleton, persists across scenes.
/// Manages background music (with crossfade), SFX (one-shot), and voicelines (with subtitles + interrupt).
/// External AudioSources passed into Play methods are automatically tracked and updated when volume changes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioDatabase _db;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _defaultMusicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _defaultVoiceSource;

    [Header("Settings")]
    [SerializeField][Range(0f, 1f)] private float _masterVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float _musicVolume = 0.8f;
    [SerializeField][Range(0f, 1f)] private float _sfxVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float _voiceVolume = 1f;

    public enum AudioSourceType { Music, SFX, Voice }

    // Tracks external sources: source -> (volumeScale, type)
    private readonly Dictionary<AudioSource, (float scale, AudioSourceType type)> _trackedSources = new();

    private Coroutine _musicFadeRoutine;
    private CancellationTokenSource _voiceCts;
    private AudioSource _currentMusicSource;
    private AudioSource _currentVoiceSource;
    private bool _voicelineInProgress;

    public AudioSource CurrentMinigamesSource { get; set; }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ApplyVolumes();
    }

    // -------------------------------------------------------------------------
    // Volume
    // -------------------------------------------------------------------------

    public float MasterVolume => _masterVolume;
    public float MusicVolume  => _musicVolume;
    public float SFXVolume    => _sfxVolume;
    public float VoiceVolume  => _voiceVolume;

    public void SetMasterVolume(float v) { _masterVolume = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetMusicVolume(float v)  { _musicVolume  = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetSFXVolume(float v)    { _sfxVolume    = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetVoiceVolume(float v)  { _voiceVolume  = Mathf.Clamp01(v); ApplyVolumes(); }

    private void ApplyVolumes()
    {
        // Default sources
        if (_defaultMusicSource) _defaultMusicSource.volume = _masterVolume * _musicVolume;
        if (_sfxSource)          _sfxSource.volume          = _masterVolume * _sfxVolume;
        if (_defaultVoiceSource) _defaultVoiceSource.volume = _masterVolume * _voiceVolume;

        // All tracked external sources
        var deadSources = new List<AudioSource>();
        foreach (var kvp in _trackedSources)
        {
            var source = kvp.Key;
            var (scale, type) = kvp.Value;

            if (source == null) { deadSources.Add(source); continue; }

            source.volume = type switch
            {
                AudioSourceType.Music => _masterVolume * _musicVolume * scale,
                AudioSourceType.SFX   => _masterVolume * _sfxVolume   * scale,
                AudioSourceType.Voice => _masterVolume * _voiceVolume  * scale,
                _                     => source.volume
            };
        }

        foreach (var dead in deadSources)
            _trackedSources.Remove(dead);
    }

    private void TrackSource(AudioSource source, AudioSourceType type, float volumeScale = 1f)
    {
        if (source == null) return;
        // Don't track the default sources — they're handled directly
        if (source == _defaultMusicSource || source == _sfxSource || source == _defaultVoiceSource) return;
        _trackedSources[source] = (volumeScale, type);
    }

    private void UntrackSource(AudioSource source)
    {
        if (source != null) _trackedSources.Remove(source);
    }

    // -------------------------------------------------------------------------
    // Music
    // -------------------------------------------------------------------------

    public void PlayMusic(string id, float fadeDuration = 0, AudioSource source = null, bool loop = true)
    {
        var data = _db.GetMusic(id);
        if (data == null || data.AudioClip == null) return;

        var sourceToPlay = source ?? _defaultMusicSource;
        TrackSource(sourceToPlay, AudioSourceType.Music);

        if (_musicFadeRoutine != null)
            StopCoroutine(_musicFadeRoutine);

        if (_currentMusicSource != null && _currentMusicSource.isPlaying)
        {
            if (fadeDuration <= 0)
            {
                if (_currentMusicSource != sourceToPlay)
                    _currentMusicSource.Stop();
                StartInstantMusic(data.AudioClip, 0, sourceToPlay, loop);
            }
            else
            {
                _musicFadeRoutine = StartCoroutine(CrossfadeMusic(_currentMusicSource, sourceToPlay, data.AudioClip, fadeDuration));
            }
        }
        else
        {
            StartInstantMusic(data.AudioClip, fadeDuration, sourceToPlay, loop);
        }

        _currentMusicSource = sourceToPlay;
    }

    public void PlayMusicForSource(string id, AudioSource source, bool loop = true, float volumeScale = 1f)
    {
        if (source == null) return;
        var data = _db.GetMusic(id);
        if (data == null || data.AudioClip == null) return;

        TrackSource(source, AudioSourceType.Music, volumeScale);

        source.clip   = data.AudioClip;
        source.loop   = loop;
        source.volume = _masterVolume * _musicVolume * volumeScale;
        source.Play();
    }

    public void StopMusic(float fadeDuration = 1.5f)
    {
        if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
        if (_currentMusicSource != null)
            _musicFadeRoutine = StartCoroutine(CrossfadeMusic(_currentMusicSource, null, null, fadeDuration));
    }

    public void StopMusicForSource(AudioSource source)
    {
        if (source == null) return;
        UntrackSource(source);
        source.Stop();
        source.clip = null;
    }

    private void StartInstantMusic(AudioClip clip, float fadeDuration, AudioSource source, bool loop)
    {
        source.clip = clip;
        source.loop = loop;

        if (fadeDuration <= 0)
        {
            source.volume     = _masterVolume * _musicVolume;
            source.timeSamples = 0;
            source.Play();
            return;
        }

        source.volume = 0f;
        source.Play();
        StartCoroutine(FadeInMusic(fadeDuration, source));
    }

    private IEnumerator FadeInMusic(float duration, AudioSource source)
    {
        float targetVol = _masterVolume * _musicVolume;
        float elapsed   = 0f;
        while (elapsed < duration)
        {
            elapsed      += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVol, elapsed / duration);
            yield return null;
        }
        source.volume = targetVol;
    }

    private IEnumerator CrossfadeMusic(AudioSource oldSource, AudioSource newSource, AudioClip newClip, float duration)
    {
        float targetVol = _masterVolume * _musicVolume;
        float startVol  = oldSource.volume;
        float elapsed   = 0f;

        while (elapsed < duration * 0.5f)
        {
            elapsed      += Time.deltaTime;
            oldSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (duration * 0.5f));
            yield return null;
        }
        oldSource.Stop();
        oldSource.volume = 0f;

        if (newSource != null && newClip != null)
        {
            newSource.clip   = newClip;
            newSource.volume = 0f;
            newSource.Play();

            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed      += Time.deltaTime;
                newSource.volume = Mathf.Lerp(0f, targetVol, elapsed / (duration * 0.5f));
                yield return null;
            }
            newSource.volume = targetVol;
        }
    }

    // -------------------------------------------------------------------------
    // SFX
    // -------------------------------------------------------------------------

    public void PlaySFX(string id, float volumeScale = 1f, AudioSource source = null, bool isLooping = false)
    {
        var data = _db.GetSfx(id);
        if (data == null || data.AudioClip == null) return;

        var sourceToPlay = source ?? _sfxSource;

        if (isLooping)
        {
            TrackSource(sourceToPlay, AudioSourceType.SFX, volumeScale);
            sourceToPlay.clip   = data.AudioClip;
            sourceToPlay.loop   = true;
            sourceToPlay.volume = _masterVolume * _sfxVolume * volumeScale;
            sourceToPlay.Play();
            return;
        }

        // One-shot: no tracking needed (fire and forget)
        sourceToPlay.PlayOneShot(data.AudioClip, _masterVolume * _sfxVolume * volumeScale);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, AudioSource source = null)
    {
        var sourceToPlay = source ?? _sfxSource;
        sourceToPlay.PlayOneShot(clip, _masterVolume * _sfxVolume * volumeScale);
    }

    public void StopSFX(AudioSource source)
    {
        if (source == null) return;
        UntrackSource(source);
        source.Stop();
    }

    public async UniTask PlaySFXAsync(string id, float volumeScale = 1f, AudioSource source = null, CancellationToken cancellationToken = default)
    {
        var data = _db.GetSfx(id);
        if (data == null || data.AudioClip == null) return;

        var sourceToPlay = source ?? _sfxSource;
        sourceToPlay.PlayOneShot(data.AudioClip, _masterVolume * _sfxVolume * volumeScale);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(data.AudioClip.length), cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) { }
    }

    // -------------------------------------------------------------------------
    // Voicelines
    // -------------------------------------------------------------------------

    public async UniTask PlayVoicelineAsync(string id, AudioSource source = null, CancellationToken externalToken = default)
    {
        var data = _db.GetVoiceline(id);
        if (data == null || data.AudioClip == null) return;

        var sourceToPlay = source ?? _defaultVoiceSource;
        if (sourceToPlay == null || !sourceToPlay.gameObject.activeInHierarchy) return;

        TrackSource(sourceToPlay, AudioSourceType.Voice);

        bool isInterrupting = _voicelineInProgress;

        if (_currentVoiceSource != null && _currentVoiceSource.isPlaying)
            _currentVoiceSource.Stop();

        _voiceCts?.Cancel();
        _voiceCts?.Dispose();
        _voiceCts = new CancellationTokenSource();

        var linkedCts = externalToken != default
            ? CancellationTokenSource.CreateLinkedTokenSource(_voiceCts.Token, externalToken)
            : _voiceCts;

        _currentVoiceSource  = sourceToPlay;
        _voicelineInProgress = true;

        if (isInterrupting)
            PlaySFX(SfxClips.VoicelineInterruptionSound);

        sourceToPlay.clip   = data.AudioClip;
        sourceToPlay.volume = _masterVolume * _voiceVolume;
        sourceToPlay.Play();

        SubtitleManager.Instance.ShowSubtitles(data, sourceToPlay);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(data.AudioClip.length), cancellationToken: linkedCts.Token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (linkedCts != _voiceCts) linkedCts.Dispose();
            _voicelineInProgress = false;
            UntrackSource(sourceToPlay);
        }
    }
}