using System;
using UnityEngine;

/// <summary>
/// Generic "pick this up" interactable. On Interact(): plays an optional pickup SFX, fires
/// OnPickedUp, then either destroys the GameObject or just deactivates it.
///
/// Not tied to any specific item or system - to gate this behind an ObligatorySequence step
/// (e.g. "take the backpack"), add an ObligatoryStepGate alongside it, wrapping this component
/// with a matching Action Id, same as doors.
/// </summary>
public class PickupInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Hint")]
    [SerializeField] private string _hintLabel = "Pick up";
    [SerializeField] private float _hintXOffset = 0f;
    [SerializeField] private float _hintYOffset = 0.5f;
    [SerializeField] private float _hintZOffset = 0f;

    [Header("Pickup")]
    [Tooltip("If true, destroys the GameObject on pickup. If false, just deactivates it.")]
    [SerializeField] private bool _destroyOnPickup = false;
    [SerializeField] private AudioClip _pickupSfx;
    [Range(0f, 1f)]
    [SerializeField] private float _pickupSfxVolume = 1f;

    private bool _isPickedUp;

    public string HintLabel => _hintLabel;
    public float HintXOffset => _hintXOffset;
    public float HintYOffset => _hintYOffset;
    public float HintZOffset => _hintZOffset;

    /// <summary>Fired the moment this object is picked up, before it's destroyed/deactivated.</summary>
    public event Action OnPickedUp;

    public bool CanHighlight(PlayerGrabbing grabbing) => !_isPickedUp && !grabbing.IsHolding;

    public void Interact(Player player)
    {
        if (_isPickedUp)
            return;

        _isPickedUp = true;

        if (_pickupSfx != null)
            AudioSource.PlayClipAtPoint(_pickupSfx, transform.position, _pickupSfxVolume);

        OnPickedUp?.Invoke();

        if (_destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
