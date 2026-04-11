using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drawer : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] private Vector3 _openDirection = new Vector3(0, 0, 1);
    [SerializeField] private float _duration = 0.4f;

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;

    private Rigidbody _rb;
    private bool _isOpen;
    private Vector3 _closedPosition;
    private Vector3 _targetPosition;
    private float _speed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _closedPosition = transform.localPosition;
        _targetPosition = _closedPosition;
        _speed = _openDirection.magnitude / _duration;
    }

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding)
            return;
        _isOpen = !_isOpen;
        _targetPosition = _isOpen ? _closedPosition + _openDirection : _closedPosition;
    }

    private void FixedUpdate()
    {
        Vector3 worldTarget = transform.parent != null
            ? transform.parent.TransformPoint(_targetPosition)
            : _targetPosition;

        if (Vector3.Distance(_rb.position, worldTarget) < 0.001f)
        {
            _rb.MovePosition(worldTarget);
            return;
        }

        Vector3 newPos = Vector3.MoveTowards(_rb.position, worldTarget, _speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);
    }
}