using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drawer : MonoBehaviour, IInteractable, IHighlightable, IDynamicHintLabel
{
    [SerializeField] private Vector3 _openDirection = new Vector3(0, 0, 1);
    [SerializeField] private float _duration = 0.4f;
    [SerializeField] private AudioSource _audioSource;
    
    public string HintLabel => _isOpen ? "Close drawer" : "Open drawer";


    [Header("Hint")]
    [SerializeField] private float _hintXOffset = 0f;
    [SerializeField] private float _hintYOffset = 0.5f;
    [SerializeField] private float _hintZOffset = -0.1f;

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding;
    public float HintYOffset => _hintYOffset;
    public float HintXOffset => _hintXOffset;
    public float HintZOffset => _hintZOffset;


    private Rigidbody _rb;
    private bool _isOpen;
    private Vector3 _closedPosition;
    private Vector3 _targetPosition;
    private float _speed;

    public event Action OnHintChanged;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if(_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
        _rb.isKinematic = true;
        _closedPosition = transform.localPosition;
        _targetPosition = _closedPosition;
        _speed = _openDirection.magnitude / _duration;
    }

    public void Interact(Player player)
    {
        if(player.Grabbing.IsHolding)
            return;

        AudioManager.Instance.PlaySFX(_isOpen ? SfxClips.DrawerClose : SfxClips.DrawerOpen, volumeScale: 1f, source: _audioSource);
        _isOpen = !_isOpen;
        OnHintChanged?.Invoke();
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