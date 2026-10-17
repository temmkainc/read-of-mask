using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LockedGamingScreen : GamingScreen
{
    [Header("Unlock")]
    [SerializeField] private Transform _cubeToGrow;
    [SerializeField] private float _cubeGrowDuration = 0.5f;
    [SerializeField] private float _fadeDuration = 3f;
    [SerializeField] private Ease _cubeGrowEase = Ease.OutBack;
    [SerializeField] private Transform _cubeGrowPoint;
    [SerializeField] private CanvasGroup _lockedScreen;
    [SerializeField] private CanvasGroup _turnedOffScreen;
    [Tooltip("Impulse that pushes the grown cube out of the screen, towards the room.")]
    [SerializeField] private float _cubeThrowForce = 1.5f;
    [Tooltip("Gap (m) kept between the fully grown cube and the screen before physics takes over, so it never starts wedged in the TV.")]
    [SerializeField] private float _cubeScreenClearance = 0.05f;
    private bool _isUnlocked = false;
    private bool _cubePoppedOut = false;
    private Vector3 _cubeOriginalScale;
    private RigidbodyInterpolation _cubeInterpolation;
    private Vector3 _cubeOutward;
    private Vector3 _cubeReleasePosition;
    private MinigameType? _pendingMinigame = null;
    private const float VOLUME_SCALE = 0.4f;
    // Ease.OutBack briefly grows the cube ~10% past its real size.
    private const float GROW_OVERSHOOT = 1.1f;

    protected override void Awake()
    {
        base.Awake();
        if (_cubeToGrow != null)
        {
            _cubeOriginalScale = _cubeToGrow.localScale;
            _cubeInterpolation = _cubeToGrow.GetComponent<Rigidbody>().interpolation;
            _cubeToGrow.localScale = Vector3.zero;
            _cubeToGrow.GetComponent<Rigidbody>().isKinematic = true;
            _cubeToGrow.GetComponent<Collider>().enabled = false;
        }
    }

    protected override void On_CartridgeInserted(MinigameType minigameType)
    {
        if (!_isUnlocked)
        {
            _pendingMinigame = minigameType;
            return;
        }
        base.On_CartridgeInserted(minigameType);
    }

    protected override void On_CartridgeEjected()
    {
        if (!_isUnlocked)
        {
            _pendingMinigame = null;
            return;
        }
        base.On_CartridgeEjected();
    }

    public override void Interact(Player player)
    {
        if (player.Grabbing.IsHolding) return;

        if (!_isUnlocked)
        {
            _isUnlocked = true;
            GrowCube();

            if (_pendingMinigame.HasValue)
                base.On_CartridgeInserted(_pendingMinigame.Value);

            return;
        }

        base.Interact(player);
    }

    private void GrowCube()
    {
        if (_cubeToGrow == null) return;
        AudioManager.Instance.PlaySFX(SfxClips.ConsoleLoading, volumeScale: VOLUME_SCALE);
        _cubeToGrow.SetParent(null);
        _cubeToGrow.position = _cubeGrowPoint != null ? _cubeGrowPoint.position : transform.position;

        var rb = _cubeToGrow.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        // The grow below is tween-driven - interpolation could write a stale physics pose back over it.
        rb.interpolation = RigidbodyInterpolation.None;
        _cubeToGrow.GetComponent<Collider>().enabled = false;

        // Work out now, while the whole TV is still around, which way is "out of the screen" and
        // where the grown cube is fully clear of it.
        _cubeOutward = GetOutOfScreenDirection(_cubeToGrow.position);
        _cubeReleasePosition = GetClearPosition(_cubeToGrow.position, _cubeOutward);

        var targetColor = new Color(0xBC / 255f, 0xBC / 255f, 0xBC / 255f);
        _lockedScreen.GetComponent<Image>().DOColor(targetColor, _fadeDuration)
            .SetLink(_lockedScreen.gameObject)
            // OnKill, not OnComplete: it also runs if the fade gets cut short, so the cube can't be
            // left invisible inside the TV.
            .OnKill(PopCubeOut);
    }

    private void PopCubeOut()
    {
        if (_cubePoppedOut || this == null || _cubeToGrow == null) return;
        _cubePoppedOut = true;

        // Grow while still kinematic with the collider off, sliding out to a spot clear of the
        // screen - physics only takes over once the cube is full size and touches nothing.
        DOTween.Sequence()
            .Join(_cubeToGrow.DOScale(_cubeOriginalScale, _cubeGrowDuration).SetEase(_cubeGrowEase))
            .Join(_cubeToGrow.DOMove(_cubeReleasePosition, _cubeGrowDuration).SetEase(Ease.OutQuad))
            .SetLink(_cubeToGrow.gameObject)
            .OnKill(DropCube);

        if (_lockedScreen != null) _lockedScreen.gameObject.SetActive(false);
        if (_turnedOffScreen != null) _turnedOffScreen.gameObject.SetActive(true);
        AudioManager.Instance.PlaySFX(SfxClips.MinigameScore, volumeScale: VOLUME_SCALE);
    }

    // Hands the full-size cube over to physics and nudges it out of the screen.
    private void DropCube()
    {
        if (_cubeToGrow == null) return;

        _cubeToGrow.localScale = _cubeOriginalScale;
        _cubeToGrow.position = _cubeReleasePosition;
        Physics.SyncTransforms();

        var rb = _cubeToGrow.GetComponent<Rigidbody>();
        _cubeToGrow.GetComponent<Collider>().enabled = true;
        rb.isKinematic = false;
        rb.interpolation = _cubeInterpolation;
        rb.AddForce(_cubeOutward * _cubeThrowForce, ForceMode.Impulse);
    }

    // Flat direction from the screen towards the room.
    private Vector3 GetOutOfScreenDirection(Vector3 from)
    {
        var screen = GetComponent<Collider>();
        if (screen == null) return -transform.forward;

        Vector3 dir = from - screen.ClosestPoint(from);
        if (dir.sqrMagnitude < 0.0001f) dir = from - screen.bounds.center; // spawn point is inside the screen
        dir.y = 0f;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : -transform.forward;
    }

    // Spot along `outward` where the fully grown cube (overshoot included) no longer touches the screen.
    private Vector3 GetClearPosition(Vector3 from, Vector3 outward)
    {
        var screen = GetComponent<Collider>();
        var box = _cubeToGrow.GetComponent<BoxCollider>();
        if (screen == null || box == null) return from;

        Vector3 half = Vector3.Scale(box.size, _cubeOriginalScale) * (0.5f * GROW_OVERSHOOT);
        Quaternion rot = _cubeToGrow.rotation;
        float reach = Mathf.Abs(Vector3.Dot(rot * Vector3.right, outward)) * half.x
                    + Mathf.Abs(Vector3.Dot(rot * Vector3.up, outward)) * half.y
                    + Mathf.Abs(Vector3.Dot(rot * Vector3.forward, outward)) * half.z;

        float gap = Vector3.Distance(screen.ClosestPoint(from), from);
        float push = reach + _cubeScreenClearance - gap;
        return push > 0f ? from + outward * push : from;
    }
}