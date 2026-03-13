using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour, IGrabbable, IHighlightable
{
    [SerializeField] private float _throwForceMultiplier = 1f;
    [SerializeField] private LayerMask _grabbedLayer;
    [SerializeField] private float _wallOffset = 0.2f;
    [SerializeField] private float _minCameraDistance = 0.8f; 
    [SerializeField] private float _throwForce = 10f;

    private Rigidbody _rb;

    private int _originalLayer;
    public bool IsGrabbed { get; private set; }

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;
    public float WallOffset => _wallOffset;
    public float MinCameraDistance => _minCameraDistance;
    public float ThrowForce => _throwForce;


    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public virtual void Grab(Player player, Transform holdPoint)
    {
        IsGrabbed = true;
        _rb.isKinematic = true;

        _originalLayer = gameObject.layer;
        gameObject.layer = LayerMaskToLayer(_grabbedLayer);

        Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Player"), true);
    }

    public void Release(Vector3 throwForce)
    {
        IsGrabbed = false;
        transform.SetParent(null);

        Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Player"), false);
        gameObject.layer = _originalLayer;

        _rb.isKinematic = false;
        _rb.linearVelocity = throwForce * _throwForceMultiplier;
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        return Mathf.RoundToInt(Mathf.Log(mask.value, 2));
    }
}