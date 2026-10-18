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
    [Tooltip("Keep the playing animation's loop aligned with the track (both loops are the same length), so strums land on the beat.")]
    [SerializeField] private bool _syncAnimationToTrack = true;
    [Tooltip("How far (in seconds) the animation may drift from the track before it is snapped back.")]
    [SerializeField] private float _maxDrift = 0.08f;

    private const int BaseLayer = 0;
    private bool _started;

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
    }

    private void Update()
    {
        if (!_syncAnimationToTrack || _animator == null || _audioSource == null || !_audioSource.isPlaying)
            return;

        var state = _animator.GetCurrentAnimatorStateInfo(BaseLayer);
        if (_animator.IsInTransition(BaseLayer) || state.length <= 0f)
            return;

        float trackPhase = _audioSource.time / _audioSource.clip.length;
        float animPhase = Mathf.Repeat(state.normalizedTime, 1f);
        float drift = Mathf.Abs(Mathf.DeltaAngle(trackPhase * 360f, animPhase * 360f)) / 360f * state.length;
        if (drift > _maxDrift)
            _animator.Play(state.fullPathHash, BaseLayer, trackPhase);
    }

    private void Play()
    {
        if (AudioManager.Instance == null || _track == null || _audioSource == null)
            return;

        AudioManager.Instance.PlayMusicForSource(_track, _audioSource, loop: true, volumeScale: _volumeScale);
    }
}
