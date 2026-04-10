using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

/// <summary>
/// Central audio manager. Singleton, persists across scenes.
/// Manages background music (with crossfade), SFX (one-shot), and voicelines (with subtitles + interrupt).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioDatabase _db;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _defaultVoiceSource;

    [Header("Settings")]
    [SerializeField][Range(0f, 1f)] private float _masterVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float _musicVolume = 0.8f;
    [SerializeField][Range(0f, 1f)] private float _sfxVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float _voiceVolume = 1f;

    private Coroutine _musicFadeRoutine;
    
    private CancellationTokenSource _voiceCts;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyVolumes();
    }

    // ── Volume ───────────────────────────────────────────────────────────────

    public void SetMasterVolume(float v) { _masterVolume = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetMusicVolume(float v) { _musicVolume = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetSFXVolume(float v) { _sfxVolume = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetVoiceVolume(float v) { _voiceVolume = Mathf.Clamp01(v); ApplyVolumes(); }

    private void ApplyVolumes()
    {
        if (_musicSource) _musicSource.volume = _masterVolume * _musicVolume;
        if (_sfxSource) _sfxSource.volume = _masterVolume * _sfxVolume;
        if (_defaultVoiceSource) _defaultVoiceSource.volume = _masterVolume * _voiceVolume;
    }

    // ── Music ────────────────────────────────────────────────────────────────

    public void PlayMusic(string id, float fadeDuration = 1.5f)
    {
        var data = _db.GetMusic(id);
        if (data == null || data.AudioClip == null) return;

        if (_musicFadeRoutine != null)
            StopCoroutine(_musicFadeRoutine);

        if (_musicSource.isPlaying)
            _musicFadeRoutine = StartCoroutine(CrossfadeMusic(data.AudioClip, fadeDuration));
        else
            StartInstantMusic(data.AudioClip, fadeDuration);
    }
    private void StartInstantMusic(AudioClip clip, float fadeDuration)
    {
        _musicSource.clip = clip;
        _musicSource.volume = 0f;
        _musicSource.Play();

        StartCoroutine(FadeInMusic(fadeDuration));
    }
    private IEnumerator FadeInMusic(float duration)
    {
        float targetVol = _masterVolume * _musicVolume;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, targetVol, elapsed / duration);
            yield return null;
        }

        _musicSource.volume = targetVol;
    }

    public void StopMusic(float fadeDuration = 1.5f)
    {
        if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
        _musicFadeRoutine = StartCoroutine(CrossfadeMusic(null, fadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        float targetVol = _masterVolume * _musicVolume;
        float startVol = _musicSource.volume;

        float elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (duration * 0.5f));
            yield return null;
        }

        _musicSource.Stop();
        if (newClip != null)
        {
            _musicSource.clip = newClip;
            _musicSource.Play();

            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, targetVol, elapsed / (duration * 0.5f));
                yield return null;
            }
        }

        _musicSource.volume = newClip != null ? targetVol : 0f;
    }

    // ── SFX ──────────────────────────────────────────────────────────────────

    public void PlaySFX(string id, float volumeScale = 1f)
    {
        var data = _db.GetSfx(id);
        if (data == null || data.AudioClip == null) return;
        _sfxSource.PlayOneShot(data.AudioClip, volumeScale);
    }




    public async UniTask PlayVoicelineAsync(string id, AudioSource source = null)
    {
        var data = _db.GetVoiceline(id);
        if (data == null || data.AudioClip == null)
            return;

        var sourceToPlay = source ?? _defaultVoiceSource;

        _voiceCts?.Cancel();
        _voiceCts?.Dispose();
        _voiceCts = new CancellationTokenSource();
   
        var token = _voiceCts.Token;

        sourceToPlay.clip = data.AudioClip;
        sourceToPlay.Play();

        SubtitleManager.Instance.ShowSubtitles(data, sourceToPlay);

        try
        {
            await UniTask.WaitUntil(
                () => !sourceToPlay.isPlaying,
                cancellationToken: token
            );
        }
        catch (OperationCanceledException)
        {
            // expected on interrupt
        }
    }
}