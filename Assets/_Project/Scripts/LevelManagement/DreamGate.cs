using System;
using UnityEngine;
using Zenject;

/// <summary>
/// "Gate between dreams": a doorway filled with a tunnel of white light. Walking into it plays the
/// dream transition (fast spin, blur, white-out, whoosh) while drawing the player through to the
/// other side - not a teleport, just a doorway you pass through.
///
/// One-way: only enters from the front, and its back is closed - a dull, still gray slab you
/// can't walk through. Once used, the front closes too. Two-way: open from both sides, as often
/// as you like. Either way it's closed while Open During's level isn't being played (if set).
///
/// Setup: pivot on the floor in the middle of the doorway, blue (forward) axis pointing the way
/// the player walks through a one-way gate. Set Size to the opening. Put a
/// DreamGateEnteredCondition on the level that should end here.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DreamGate : MonoBehaviour
{
    /// <summary>The player stepped in. The screen is about to go white, so the next level can switch on now.</summary>
    public event Action OnEntered;
    /// <summary>The player is through.</summary>
    public event Action OnPassed;

    [Header("Shape")]
    [Tooltip("Width and height of the opening, in metres.")]
    [SerializeField] private Vector2 _size = new(3f, 3.6f);
    [Tooltip("How far the light surface reaches into the walls, floor and lintel around the opening, so it fills the doorway with no gaps.")]
    [SerializeField] private float _fillOverlap = 0.05f;
    [Tooltip("How thick the wall around the opening is - the glow on the wall sits on its face.")]
    [SerializeField] private float _wallDepth = 0.8f;
    [Tooltip("How far out from the opening the player is caught by the gate.")]
    [SerializeField] private float _entryDepth = 1f;
    [Tooltip("How far the light spills out of the doorway onto the floor.")]
    [SerializeField] private float _lightSpillLength = 3.5f;

    [Header("Which Way")]
    [Tooltip("On: walk through from either side, any number of times. Off: one-way - only from the front (blue axis points the way through), the back is closed, and it closes for good once used.")]
    [SerializeField] private bool _twoWay;
    [Tooltip("The gate is only open while this level is being played; before and after, both sides are closed. Leave empty to ignore levels.")]
    [SerializeField] private LevelBase _openDuring;

    [Header("Passing Through")]
    [Tooltip("How far past the opening the player ends up. Keep it longer than Entry Depth + the player's radius, so a two-way gate doesn't catch them again.")]
    [SerializeField] private float _exitDistance = 1.7f;
    [Tooltip("Seconds the screen takes to rush into white.")]
    [SerializeField] private float _rushIn = 0.35f;
    [Tooltip("Seconds the screen stays fully white.")]
    [SerializeField] private float _hold = 0.15f;
    [Tooltip("Seconds the picture takes to come back after the white.")]
    [SerializeField] private float _rushOut = 0.9f;
    [Tooltip("How many full turns the picture spins over the whole transition.")]
    [SerializeField] private int _turns = 2;
    [Tooltip("Seconds a side takes to close or open.")]
    [SerializeField] private float _closeDuration = 0.8f;

    [Header("Sound")]
    [SerializeField] private AudioClip _transitionSound;
    [SerializeField][Range(0f, 2f)] private float _transitionVolume = 1f;
    [Tooltip("Music level during the transition, relative to its normal level.")]
    [SerializeField][Range(0f, 1f)] private float _musicDuck = 0.3f;

    [Header("Parts")]
    [SerializeField] private Renderer _tunnel;
    [SerializeField] private Renderer _frontHalo;
    [SerializeField] private Renderer _backHalo;
    [SerializeField] private float _haloMargin = 1.2f;
    [SerializeField] private Renderer _frontSpill;
    [SerializeField] private Renderer _backSpill;
    [SerializeField] private BoxCollider _seal;
    [SerializeField] private Light _light;
    [SerializeField] private ParticleSystem _motes;
    [SerializeField] private float _lightIntensity = 4f;
    [SerializeField][Range(0f, 1f)] private float _lightFlicker = 0.15f;

    private static readonly int HoleId = Shader.PropertyToID("_Hole");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int ClosedFrontId = Shader.PropertyToID("_ClosedFront");
    private static readonly int ClosedBackId = Shader.PropertyToID("_ClosedBack");

    [Inject(Optional = true)] private PlayerFirstPersonHandsController _handsController;

    private BoxCollider _entry;
    private MaterialPropertyBlock _block;
    private PlayerController _player;
    private bool _used;
    private bool _transitioning;
    private bool _waitForPlayerToLeave;
    private bool _bumpedClosedSide;
    private float _startTime;
    private Vector3 _pullFrom, _pullTo;
    private bool _pullDone;
    private float _closedFront, _closedBack;
    private float _motesRate;

    public bool IsTwoWay => _twoWay;
    public bool IsFrontOpen => IsLevelOpen && !(_used && !_twoWay);
    public bool IsBackOpen => IsLevelOpen && _twoWay;
    private bool IsLevelOpen => _openDuring == null || _openDuring.IsRunning;

    private void Awake()
    {
        _entry = GetComponent<BoxCollider>();
        _entry.isTrigger = true;
        if (_motes != null)
            _motesRate = _motes.emission.rateOverTimeMultiplier;
        ApplyShape();
    }

    private void Start()
    {
        // start in the right state, without animating into it
        _closedFront = IsFrontOpen ? 0f : 1f;
        _closedBack = IsBackOpen ? 0f : 1f;
        ApplyLook();
    }

    private void OnValidate()
    {
        // in the editor, show each side as it'll start: open, or closed if it's one-way's back
        if (!Application.isPlaying)
        {
            _closedFront = 0f;
            _closedBack = _twoWay ? 0f : 1f;
        }
        ApplyShape();
    }

    private void OnDisable()
    {
        if (_transitioning)
            FinishTransition();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_transitioning || _waitForPlayerToLeave || !other.TryGetComponent(out PlayerController player))
            return;

        bool fromFront = transform.InverseTransformPoint(player.transform.position).z < 0f;
        if (fromFront ? !IsFrontOpen : !IsBackOpen)
            return;

        StartTransition(player, fromFront);
    }

    private void StartTransition(PlayerController player, bool fromFront)
    {
        _transitioning = true;
        _pullDone = false;
        _startTime = Time.time;
        _player = player;

        // draw the player straight through, keeping where along the doorway they walked in
        Vector3 local = transform.InverseTransformPoint(player.transform.position);
        float halfWidth = Mathf.Max(0f, _size.x * 0.5f - 0.6f);
        _pullFrom = player.transform.position;
        _pullTo = transform.TransformPoint(new Vector3(Mathf.Clamp(local.x, -halfWidth, halfWidth), 0f, fromFront ? _exitDistance : -_exitDistance));
        _pullTo.y = _pullFrom.y;

        if (AudioManager.Instance != null)
        {
            if (_transitionSound != null)
                AudioManager.Instance.PlaySFX(_transitionSound, _transitionVolume);
            AudioManager.Instance.DuckMusic(_musicDuck, _rushIn, _hold, _rushOut);
        }

        _used = true;
        OnEntered?.Invoke();
    }

    private void Update()
    {
        // a side that's closed is solid - but never while drawing the player through
        if (_seal != null)
            _seal.enabled = !_transitioning && !(IsFrontOpen && IsBackOpen);

        if (_transitioning)
            UpdateTransition();
        else
            WatchPlayer();

        float step = Time.deltaTime / Mathf.Max(_closeDuration, 0.01f);
        _closedFront = Mathf.MoveTowards(_closedFront, IsFrontOpen ? 0f : 1f, step);
        _closedBack = Mathf.MoveTowards(_closedBack, IsBackOpen ? 0f : 1f, step);
        ApplyLook();
    }

    private void LateUpdate()
    {
        if (!_transitioning || _pullDone || _player == null)
            return;

        // drawn in through the rush, through the opening by the time the screen is white
        float pullTime = _rushIn + _hold * 0.5f;
        float k = Mathf.Clamp01((Time.time - _startTime) / pullTime);
        float eased = k * k * (3f - 2f * k);
        _player.Teleport(Vector3.Lerp(_pullFrom, _pullTo, eased), _player.transform.rotation);
        if (k >= 1f)
            _pullDone = true;
    }

    // Re-arms a two-way gate once the player has stepped away, and shakes the hands "no" when
    // they walk up to a closed side.
    private void WatchPlayer()
    {
        if (_player == null)
            _player = FindAnyObjectByType<PlayerController>();
        if (_player == null)
            return;

        Vector3 local = transform.InverseTransformPoint(_player.transform.position);
        float distance = Mathf.Abs(local.z);
        if (_waitForPlayerToLeave && distance > _entryDepth + 0.7f)
            _waitForPlayerToLeave = false;

        bool closedSide = local.z < 0f ? !IsFrontOpen : !IsBackOpen;
        bool atDoorway = Mathf.Abs(local.x) < _size.x * 0.5f + 0.3f && local.y < _size.y;
        if (closedSide && atDoorway && distance < 0.95f && !_bumpedClosedSide)
        {
            _bumpedClosedSide = true;
            _handsController?.PlayNotAllowedAnimation();
        }
        else if (distance > 1.6f)
        {
            _bumpedClosedSide = false;
        }
    }

    private void UpdateTransition()
    {
        float t = Time.time - _startTime;
        float peak = _rushIn + _hold;
        float total = peak + _rushOut;

        float amount, white;
        if (t < _rushIn)
        {
            float x = t / _rushIn;
            amount = x * x;
            white = x * x * x;
        }
        else if (t < peak)
        {
            amount = 1f;
            white = 1f;
        }
        else
        {
            float y = Mathf.Clamp01((t - peak) / _rushOut);
            white = 1f - Mathf.SmoothStep(0f, 1f, y / 0.6f);
            amount = (1f - y) * (1f - y) * (1f - y);
        }

        // whole turns, fastest while the screen is white, so the picture lands back upright
        float u = Mathf.Clamp01(t / total);
        float p = Mathf.Clamp(peak / total, 0.05f, 0.95f);
        float turned = u < p ? u * u / p : 1f - (1f - u) * (1f - u) / (1f - p);
        DreamTransition.Set(amount, white, turned * _turns * Mathf.PI * 2f);

        if (t >= total)
            FinishTransition();
    }

    private void FinishTransition()
    {
        if (_player != null && !_pullDone)
            _player.Teleport(_pullTo, _player.transform.rotation);

        _transitioning = false;
        _pullDone = true;
        _waitForPlayerToLeave = true;
        DreamTransition.Clear();
        OnPassed?.Invoke();
    }

    /// <summary>Fades each side's light, glow and particles with how closed that side is.</summary>
    private void ApplyLook()
    {
        _block ??= new MaterialPropertyBlock();

        if (_tunnel != null)
        {
            _tunnel.GetPropertyBlock(_block);
            _block.SetFloat(ClosedFrontId, _closedFront);
            _block.SetFloat(ClosedBackId, _closedBack);
            _tunnel.SetPropertyBlock(_block);
        }

        SetGlow(_frontHalo, 1f - _closedFront, true);
        SetGlow(_backHalo, 1f - _closedBack, true);
        SetGlow(_frontSpill, 1f - _closedFront, false);
        SetGlow(_backSpill, 1f - _closedBack, false);

        float open = Mathf.Max(1f - _closedFront, 1f - _closedBack);
        if (_light != null)
        {
            float flicker = 1f + _lightFlicker * (Mathf.PerlinNoise(Time.time * 3f, 0.37f) * 2f - 1f);
            _light.intensity = _lightIntensity * flicker * Mathf.Lerp(0.08f, 1f, open);
        }

        if (_motes != null && Application.isPlaying)
        {
            var emission = _motes.emission;
            emission.rateOverTimeMultiplier = _motesRate * (1f - _closedFront);
        }
    }

    private void SetGlow(Renderer glow, float strength, bool isHalo)
    {
        if (glow == null || glow.sharedMaterial == null)
            return;

        glow.GetPropertyBlock(_block);
        _block.SetFloat(IntensityId, glow.sharedMaterial.GetFloat(IntensityId) * strength);
        if (isHalo)
        {
            float w = Mathf.Max(0.1f, _size.x), h = Mathf.Max(0.1f, _size.y);
            _block.SetVector(HoleId, new Vector4(w / (w + _haloMargin * 2f), h / (h + _haloMargin * 2f), 0f, 0f));
        }
        glow.SetPropertyBlock(_block);
    }

    /// <summary>Fits the trigger, the seal and the visuals to Size and to one-/two-way.</summary>
    private void ApplyShape()
    {
        float w = Mathf.Max(0.1f, _size.x), h = Mathf.Max(0.1f, _size.y);

        var entry = _entry != null ? _entry : GetComponent<BoxCollider>();
        if (entry != null)
        {
            entry.isTrigger = true;
            // one-way: only the front catches the player; two-way: both sides do
            entry.center = _twoWay ? new Vector3(0f, h * 0.5f, 0f) : new Vector3(0f, h * 0.5f, -_entryDepth * 0.5f - 0.05f);
            entry.size = new Vector3(w, h, _twoWay ? _entryDepth * 2f + 0.1f : _entryDepth);
        }

        if (_seal != null)
        {
            _seal.center = new Vector3(0f, h * 0.5f, 0f);
            _seal.size = new Vector3(w, h, 0.3f);
        }

        // a little bigger than the opening, so its edges tuck into the walls and no gap shows
        if (_tunnel != null)
            Place(_tunnel.transform, new Vector3(0f, h * 0.5f, 0f), Quaternion.identity, new Vector3(w + _fillOverlap * 2f, h + _fillOverlap * 2f, 1f));

        var haloScale = new Vector3(w + _haloMargin * 2f, h + _haloMargin * 2f, 1f);
        if (_frontHalo != null)
            Place(_frontHalo.transform, new Vector3(0f, h * 0.5f, -_wallDepth * 0.5f - 0.02f), Quaternion.identity, haloScale);
        if (_backHalo != null)
        {
            Place(_backHalo.transform, new Vector3(0f, h * 0.5f, _wallDepth * 0.5f + 0.02f), Quaternion.identity, haloScale);
            _backHalo.gameObject.SetActive(_twoWay);
        }

        // the spill mesh is 1x1x1 (doorway-sized at its start, floor at y=0), pointing down -Z
        var spillScale = new Vector3(w, h, _lightSpillLength);
        if (_frontSpill != null)
            Place(_frontSpill.transform, new Vector3(0f, 0.02f, -_wallDepth * 0.5f), Quaternion.identity, spillScale);
        if (_backSpill != null)
        {
            Place(_backSpill.transform, new Vector3(0f, 0.02f, _wallDepth * 0.5f), Quaternion.Euler(0f, 180f, 0f), spillScale);
            _backSpill.gameObject.SetActive(_twoWay);
        }

        if (_light != null)
            _light.transform.localPosition = new Vector3(0f, h * 0.5f, -Mathf.Max(0.6f, _wallDepth));

        if (_motes != null)
        {
            _motes.transform.localPosition = new Vector3(0f, h * 0.5f, -_wallDepth * 0.5f - 0.1f);
            var shape = _motes.shape;
            shape.scale = new Vector3(w * 1.6f, h * 1.2f, 1.4f);
        }

        ApplyLook();
    }

    private static void Place(Transform part, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        part.localPosition = position;
        part.localRotation = rotation;
        part.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Application.isPlaying && !IsFrontOpen ? new Color(1f, 0.3f, 0.3f, 0.8f) : new Color(1f, 1f, 1f, 0.8f);
        Gizmos.DrawWireCube(new Vector3(0f, _size.y * 0.5f, 0f), new Vector3(_size.x, _size.y, 0.05f));
        // arrow: the way through (both ways for a two-way gate)
        Gizmos.DrawLine(new Vector3(0f, 0.05f, _twoWay ? -_exitDistance : -_entryDepth), new Vector3(0f, 0.05f, _exitDistance));
        Gizmos.DrawWireSphere(new Vector3(0f, 0.05f, _exitDistance), 0.15f);
        if (_twoWay)
            Gizmos.DrawWireSphere(new Vector3(0f, 0.05f, -_exitDistance), 0.15f);
    }
}
