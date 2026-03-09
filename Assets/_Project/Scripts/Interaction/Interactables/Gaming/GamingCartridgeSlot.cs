using System;
using UnityEngine;
using Zenject;

public class GamingCartridgeSlot : MonoBehaviour, IInteractable, IHighlightable
{
    public GamingCartridgeData CurrentCartridge { get; private set; }

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding || grabbing.TryGetHeld<GamingCartridgeItem>(out _);

    [SerializeField] private Transform _ejectPoint;

    private GamingCartridgeItem _currentCartridgeItem; 

    public event Action OnCartridgeInserted;
    public event Action OnCartridgeEjected;

    public void Interact(Player player)
    {
        if(_currentCartridgeItem != null)
        {
            Eject();
            return;
        }

        if (!player.Grabbing.TryGetHeld<GamingCartridgeItem>(out var cartridge))
            return;
       
        player.Grabbing.ReleaseHeldObject();

        _currentCartridgeItem = cartridge;
        cartridge.gameObject.SetActive(false);

        Insert(cartridge);
    }

    public bool Insert(GamingCartridgeItem item)
    {
        if (CurrentCartridge != null)
            return false;

        CurrentCartridge = item.CartridgeData;
        OnCartridgeInserted?.Invoke();

        return true;
    }

    private void Eject()
    {
        var cartridge = CurrentCartridge;

        CurrentCartridge = null;
        OnCartridgeEjected?.Invoke();

        ThrowCartridgeOut();
    }

    private void ThrowCartridgeOut()
    {
        if (_currentCartridgeItem == null)
            return;

        if (_ejectPoint == null)
        {
            Debug.LogWarning("[CartridgeSlot] EjectPoint not assigned, using slot position.", this);
            _ejectPoint = transform;
        }

        var cartridgeTransform = _currentCartridgeItem.transform;
        cartridgeTransform.SetParent(null);
        cartridgeTransform.position = _ejectPoint.position;
        cartridgeTransform.rotation = _ejectPoint.rotation;

        _currentCartridgeItem.gameObject.SetActive(true);

        Rigidbody rb = _currentCartridgeItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce((_ejectPoint.forward + Vector3.up * 0.3f) * 2f, ForceMode.Impulse);
        }

        _currentCartridgeItem = null;
    }
}