using UnityEngine;

/// <summary>
/// Plays the faceless guitarist's track on a loop from the guitar's own (3D) AudioSource, through
/// AudioManager so the master/music volume settings apply to it, and keeps the strumming animation
/// on the beat of the track. The music stops while this object is disabled (e.g. its level is
/// switched off) and starts again when it comes back.
/// </summary>
public class GuitarPlayerController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private MusicData _track;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField][Range(0f, 1f)] private float _volumeScale = 0.5f;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [Tooltip("Keep the strumming on the beat of the track.")]
    [SerializeField] private bool _syncAnimationToTrack = true;
    [Tooltip("Optional. The track's real beat times: the strumming then follows a live recording's drifting tempo, one strum per beat. Without it, the animation loop is simply aligned with the track loop (they must be the same length).")]
    [SerializeField] private BeatMap _beatMap;
    [Tooltip("Length of one strum (one beat) in the playing animation, in seconds.")]
    [SerializeField] private float _animationBeatLength = 0.5f;
    [Tooltip("How far (in seconds) the animation may drift from the track before it is snapped back.")]
    [SerializeField] private float _maxDrift = 0.08f;

    private const int BaseLayer = 0;
    private bool _started;
    private int _loopsPlayed;
    private float _lastTrackTime;

    private void Start()
    {
        // AudioManager may not exist yet during the first OnEnable of a freshly loaded scene.
        _started = true;
        Play();
    }

    private void OnEnable()
    {
        if (_started) Play();
    }

    private void OnDisable()
    {
        // Can be null while the scene is being torn down.
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusicForSource(_audioSource);
        if (_animator != null)
            _animator.speed = 1f;
    }

    private void Update()
    {
        if (!_syncAnimationToTrack || _animator == null || _audioSource == null || !_audioSource.isPlaying)
            return;

        var state = _animator.GetCurrentAnimatorStateInfo(BaseLayer);
        if (_animator.IsInTransition(BaseLayer))
            return;
        // The clip's own length: AnimatorStateInfo.length changes with the animator speed set below.
        var clips = _animator.GetCurrentAnimatorClipInfo(BaseLayer);
        if (clips.Length == 0 || clips[0].clip.length <= 0f)
            return;
        float animLength = clips[0].clip.length;

        float trackTime = _audioSource.time;
        float clipLength = _audioSource.clip.length;
        if (trackTime < _lastTrackTime - clipLength * 0.5f) _loopsPlayed++; // the track wrapped around
        _lastTrackTime = trackTime;

        float targetTime;
        if (_beatMap != null && _beatMap.BeatTimes != null && _beatMap.BeatTimes.Length > 1)
        {
            // One strum per beat; count beats across track loops so the body's idle motion never jumps.
            float beat = _beatMap.BeatAt(trackTime, clipLength, _loopsPlayed);
            targetTime = Mathf.Repeat(beat * _animationBeatLength, animLength);
            float beatLength = _beatMap.BeatLengthAt(trackTime, clipLength);
            if (beatLength > 0f) _animator.speed = _animationBeatLength / beatLength;
        }
        else
        {
            targetTime = trackTime / clipLength * animLength;
        }

        float animTime = Mathf.Repeat(state.normalizedTime, 1f) * animLength;
        float drift = Mathf.Abs(Mathf.DeltaAngle(animTime / animLength * 360f, targetTime / animLength * 360f)) / 360f * animLength;
        if (drift > _maxDrift)
            _animator.Play(state.fullPathHash, BaseLayer, targetTime / animLength);
    }

    private void Play()
    {
        if (AudioManager.Instance == null || _track == null || _audioSource == null)
            return;

        _loopsPlayed = 0;
        _lastTrackTime = 0f;
        AudioManager.Instance.PlayMusicForSource(_track, _audioSource, loop: true, volumeScale: _volumeScale);
    }
}
