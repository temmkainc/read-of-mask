using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Runtime subtitle display manager. Tracks AudioSource.time every frame
/// and swaps subtitle text when the playhead crosses cue boundaries.
/// Attach to a persistent GameObject alongside AudioManager.
/// </summary>
public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInTime = 0.12f;
    [SerializeField] private float fadeOutTime = 0.18f;

    private Coroutine _activeRoutine;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _canvasGroup = subtitlePanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = subtitlePanel.AddComponent<CanvasGroup>();

        subtitlePanel.SetActive(false);
    }

    /// <summary>Begin displaying timed subtitle cues that follow the given AudioSource.</summary>
    public void ShowSubtitles(VoicelineData data, AudioSource source)
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);

        if (data == null || data.Subtitles == null || data.Subtitles.Count == 0)
        {
            subtitlePanel.SetActive(false);
            return;
        }

        _activeRoutine = StartCoroutine(SubtitleRoutine(data, source));
    }

    /// <summary>Immediately hide subtitles (called on voiceline interrupt).</summary>
    public void ClearSubtitle()
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = null;
        subtitlePanel.SetActive(false);
        _canvasGroup.alpha = 0f;
    }

    // ── Core routine ─────────────────────────────────────────────────────────

    private IEnumerator SubtitleRoutine(VoicelineData data, AudioSource source)
    {
        subtitlePanel.SetActive(true);
        _canvasGroup.alpha = 0f;

        SubtitleCue lastCue = null;

        while (source != null && source.isPlaying)
        {
            SubtitleCue current = data.GetCueAt(source.time);

            if (current != lastCue)
            {
                lastCue = current;

                if (current == null)
                {
                    // Entered a gap — fade out
                    yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));
                }
                else
                {
                    // New cue — update text, fade in (instant-cross if was visible)
                    float fromAlpha = _canvasGroup.alpha;
                    subtitleText.text = current.text;
                    yield return StartCoroutine(Fade(fromAlpha, 1f, fadeInTime));
                }
            }

            yield return null;
        }

        // Clip ended — fade out
        yield return StartCoroutine(Fade(_canvasGroup.alpha, 0f, fadeOutTime));
        subtitlePanel.SetActive(false);
        _activeRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f) { _canvasGroup.alpha = to; yield break; }

        float elapsed = 0f;
        _canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }
}