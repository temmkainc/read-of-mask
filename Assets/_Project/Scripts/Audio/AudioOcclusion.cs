using UnityEngine;

/// <summary>
/// Muffles a 3D AudioSource when walls or floors are between it and the listener: a few times a
/// second it casts a ray from the AudioListener to the source, and each solid collider the ray
/// passes through lowers the low-pass cutoff and the level, smoothly - so music from another room
/// or floor sounds like it comes through the wall instead of straight through it.
/// The level change is applied in the audio stream itself, so it never fights AudioManager's
/// volume settings on the source.
/// </summary>
[RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
public class AudioOcclusion : MonoBehaviour
{
    [Tooltip("What counts as walls and floors. Triggers are always ignored, and so are colliders belonging to the source's own character and to the listener's.")]
    [SerializeField] private LayerMask _occluders = ~0;
    [Tooltip("Colliders under this object never count as walls (the source's own body). Defaults to the nearest Animator above the source.")]
    [SerializeField] private Transform _ignoreCollidersUnder;
    [Tooltip("Low-pass cutoff (Hz) with nothing in the way.")]
    [SerializeField] private float _clearCutoff = 22000f;
    [Tooltip("Low-pass cutoff (Hz) behind one wall; each further wall lowers it further.")]
    [SerializeField] private float _occludedCutoff = 900f;
    [Tooltip("Level behind one wall; each further wall multiplies it again.")]
    [SerializeField][Range(0f, 1f)] private float _occludedVolume = 0.35f;
    [Tooltip("How many walls are counted at most.")]
    [SerializeField] private int _maxWalls = 3;
    [Tooltip("Seconds between occlusion checks.")]
    [SerializeField] private float _checkInterval = 0.1f;
    [Tooltip("How quickly (seconds) the sound eases between clear and muffled.")]
    [SerializeField] private float _smoothing = 0.25f;

    private readonly RaycastHit[] _hits = new RaycastHit[16];
    private AudioLowPassFilter _lowPass;
    private AudioListener _listener;
    private Transform _ownRoot;
    private float _nextCheck;
    private float _targetCutoff, _targetGain = 1f;
    private float _cutoff, _gain = 1f;
    private volatile float _audioGain = 1f;   // read on the audio thread

    public int WallsBetween { get; private set; }

    private void Awake()
    {
        _lowPass = GetComponent<AudioLowPassFilter>();
        _ownRoot = _ignoreCollidersUnder;
        if (_ownRoot == null)
        {
            var animator = GetComponentInParent<Animator>();
            _ownRoot = animator != null ? animator.transform : transform;
        }
        _cutoff = _targetCutoff = _clearCutoff;
        _lowPass.cutoffFrequency = _cutoff;
    }

    private void Update()
    {
        if (Time.time >= _nextCheck)
        {
            _nextCheck = Time.time + _checkInterval;
            if (_listener == null || !_listener.isActiveAndEnabled)
                _listener = FindFirstObjectByType<AudioListener>();
            if (_listener != null)
            {
                WallsBetween = CountWalls(_listener.transform.position);
                _targetCutoff = WallsBetween == 0 ? _clearCutoff : _occludedCutoff / Mathf.Pow(1.6f, WallsBetween - 1);
                _targetGain = Mathf.Pow(_occludedVolume, WallsBetween);
            }
        }

        float k = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(_smoothing, 0.01f));
        // ease the cutoff on a log scale so it sounds even
        _cutoff = Mathf.Exp(Mathf.Lerp(Mathf.Log(_cutoff), Mathf.Log(_targetCutoff), k));
        _gain = Mathf.Lerp(_gain, _targetGain, k);
        _lowPass.cutoffFrequency = _cutoff;
        _audioGain = _gain;
    }

    /// <summary>How many walls/floors are between this source and a listener at the given point.</summary>
    public int CountWalls(Vector3 listenerPosition)
    {
        Vector3 toSource = transform.position - listenerPosition;
        float distance = toSource.magnitude;
        if (distance < 0.01f) return 0;

        // the listener's own body (the player) never counts as a wall
        Transform listenerRoot = null;
        if (_listener != null)
        {
            var body = _listener.GetComponentInParent<CharacterController>();
            listenerRoot = body != null ? body.transform : _listener.transform;
        }
        int count = Physics.RaycastNonAlloc(listenerPosition, toSource / distance, _hits, distance, _occluders, QueryTriggerInteraction.Ignore);
        int walls = 0;
        for (int i = 0; i < count; i++)
        {
            Transform hit = _hits[i].collider.transform;
            if (hit.IsChildOf(_ownRoot) || (listenerRoot != null && hit.IsChildOf(listenerRoot)))
                continue;
            walls++;
        }
        return Mathf.Min(walls, _maxWalls);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        float g = _audioGain;
        if (g >= 0.999f) return;
        for (int i = 0; i < data.Length; i++)
            data[i] *= g;
    }
}
