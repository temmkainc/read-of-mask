using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour, IGrabbable
{
    [SerializeField] private float _throwForceMultiplier = 1f;
    [SerializeField] private LayerMask _grabbedLayer;

    private Rigidbody _rb;

    private int _originalLayer;
    public bool IsGrabbed { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Grab(Player player, Transform holdPoint)
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