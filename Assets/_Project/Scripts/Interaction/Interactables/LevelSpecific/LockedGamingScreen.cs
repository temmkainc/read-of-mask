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
    [SerializeField] private float _cubeThrowForce = 5f;
    private bool _isUnlocked = false;
    private Vector3 _cubeOriginalScale;
    private MinigameType? _pendingMinigame = null;
    private const float VOLUME_SCALE = 0.4f;

    protected override void Awake()
    {
        base.Awake();
        if (_cubeToGrow != null)
        {
            _cubeOriginalScale = _cubeToGrow.localScale;
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

        _cubeToGrow.GetComponent<Rigidbody>().isKinematic = true;
        _cubeToGrow.GetComponent<Collider>().enabled = false;

        var targetColor = new Color(0xBC / 255f, 0xBC / 255f, 0xBC / 255f);
        _lockedScreen.GetComponent<Image>().DOColor(targetColor, _fadeDuration)
            .OnComplete(() =>
            {
                var rb = _cubeToGrow.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                _cubeToGrow.GetComponent<Collider>().enabled = true;

                _cubeToGrow
                    .DOScale(_cubeOriginalScale, _cubeGrowDuration)
                    .SetEase(_cubeGrowEase);

                rb.AddForce(
                    _cubeGrowPoint.forward * _cubeThrowForce,
                    ForceMode.Impulse
                );

                _lockedScreen.gameObject.SetActive(false);
                _turnedOffScreen.gameObject.SetActive(true);
                AudioManager.Instance.PlaySFX(SfxClips.MinigameScore, volumeScale: VOLUME_SCALE);
            });
    }
}