using DG.Tweening;
using UnityEngine;
public class JarInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private Transform _dropPoint;
    [SerializeField] private GameObject _jarMesh;
    [SerializeField] private GameObject[] _fragmentPrefabs;
    [SerializeField] private float _fallDuration = 0.4f;
    [SerializeField] private float _explosionForce = 3f;
    [SerializeField] private float _explosionRadius = 1f;
    [SerializeField] private Transform _transformToMove;
    [Header("Cube")]
    [SerializeField] private Transform _cubeToGrow;
    [SerializeField] private float _cubeGrowDuration = 0.5f;
    [SerializeField] private Ease _cubeGrowEase = Ease.OutBack;
    private bool _broken = false;
    private Vector3 _cubeOriginalScale;

    [Header("Hint")]
    [SerializeField] private float _hintXOffset = 0.3f;
    [SerializeField] private float _hintYOffset = 1.5f;
    [SerializeField] private float _hintZOffset = 0f;

    public string HintLabel => "Take";

    public float HintYOffset => _hintYOffset;
    public float HintXOffset => _hintXOffset;
    public float HintZOffset => _hintZOffset;

    private void Awake()
    {
        if (_cubeToGrow != null)
        {
            // _cubeOriginalScale = _cubeToGrow.localScale;
            // _cubeToGrow.localScale = Vector3.zero;
            // _cubeToGrow.GetComponent<Rigidbody>().isKinematic = true;
            // _cubeToGrow.GetComponent<Collider>().enabled = false;
            _cubeToGrow.gameObject.SetActive(false);
        }
    }
    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding && !_broken;

    public void Interact(Player player = null)
    {
        if (_broken || player.Grabbing.IsHolding) return;
        _broken = true;
        Vector3 randomTilt = new Vector3(
            Random.Range(80f, 100f),
            Random.Range(-30f, 30f),
            Random.Range(-20f, 20f)
        );
        _transformToMove
            .DOMove(_dropPoint.position, _fallDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(Break);
        _transformToMove
            .DORotate(randomTilt, _fallDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InCubic);
    }

    private void Break()
    {
        AudioManager.Instance.PlaySFX(SfxClips.GlassBreak);
        _jarMesh.SetActive(false);

        foreach (var prefab in _fragmentPrefabs)
        {
            var fragment = Instantiate(prefab, _dropPoint.position, Random.rotation);
            var rb = fragment.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(_explosionForce, _dropPoint.position, _explosionRadius);
        }

        // GrowCube();
        _cubeToGrow.gameObject.SetActive(true);
        Destroy(gameObject);
    }

    // private void GrowCube()
    // {
    //     if (_cubeToGrow == null) return;
    //     _cubeToGrow.SetParent(null);
    //     _cubeToGrow.position = _dropPoint.position;
    //     _cubeToGrow.GetComponent<Rigidbody>().isKinematic = false;
    //     _cubeToGrow.GetComponent<Collider>().enabled = true;
    //     _cubeToGrow
    //         .DOScale(_cubeOriginalScale, _cubeGrowDuration)
    //         .SetEase(_cubeGrowEase);
    // }
}