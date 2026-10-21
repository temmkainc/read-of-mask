using UnityEngine;

/// <summary>
/// A seat - a chair (one place), a bench or a row of seats (several): sits the player in the free
/// place nearest to them, by moving the look point's pivot there before the usual Look Closer.
/// Places someone else already sits in are marked taken when the level is built; with every place
/// taken it can't be used.
///
/// Setup: CameraSnapPoint is an InteractionLookPoint at the local origin of <see cref="_place"/>, a
/// child of this object; each place is the sitter's eyes (local to this object) and the way they look.
/// </summary>
public class BenchInteractable : LookCloserInteractableBase
{
    [Tooltip("The pivot the look point sits on - moved and turned to the chosen place.")]
    [SerializeField] private Transform _place;
    [Tooltip("The sitter's eyes in each place, local to this object.")]
    [SerializeField] private Vector3[] _places = new Vector3[0];
    [Tooltip("Which way the sitter looks in each place: degrees round Y from this object's forward.")]
    [SerializeField] private float[] _yaws = new float[0];
    [Tooltip("Places someone already sits in.")]
    [SerializeField] private bool[] _taken = new bool[0];

    public override bool CanHighlight(PlayerGrabbing grabbing)
        => HasFreePlace() && base.CanHighlight(grabbing);

    public override void Interact(Player player = null)
    {
        if (player == null || !HasFreePlace())
            return;

        int nearest = -1;
        float best = float.MaxValue;
        for (int i = 0; i < _places.Length; i++)
        {
            if (IsTaken(i))
                continue;
            float distance = (transform.TransformPoint(_places[i]) - player.transform.position).sqrMagnitude;
            if (distance < best)
            {
                best = distance;
                nearest = i;
            }
        }

        _place.localPosition = _places[nearest];
        _place.localRotation = Quaternion.Euler(0f, _yaws[nearest], 0f);
        base.Interact(player);
    }

    private bool IsTaken(int i) => i < _taken.Length && _taken[i];

    private bool HasFreePlace()
    {
        for (int i = 0; i < _places.Length; i++)
            if (!IsTaken(i))
                return true;
        return false;
    }
}
