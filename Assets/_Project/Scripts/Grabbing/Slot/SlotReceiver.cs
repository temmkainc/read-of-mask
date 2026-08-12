using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class SlotReceiver : MonoBehaviour, IInteractable, IHighlightable
{
    public abstract string HintLabel { get; }

    public abstract float HintYOffset { get; }
    public abstract float HintXOffset { get; }
    public abstract float HintZOffset { get; }

    public virtual bool CanHighlight(PlayerGrabbing grabbing) => true;
    public abstract void Interact(Player player);
    public abstract void Show(bool active);
    public abstract void Clear();
}

public abstract class SlotReceiver<T> : SlotReceiver
    where T : GrabbableObject
{
    [SerializeField] private float _snapDuration = 0.25f;

    protected T CurrentObject { get; private set; }
    public bool IsOccupied => CurrentObject != null;

    public event Action<T> OnInserted;
    public event Action OnCleared;


    public override bool CanHighlight(PlayerGrabbing grabbing)
        => grabbing.TryGetHeld<T>(out _);

    public override string HintLabel => IsOccupied ? "Take out" : "Insert";
    public override float HintYOffset => 0.5f;
    public override float HintXOffset => 0f;
    public override float HintZOffset => -0.1f;

    public override void Interact(Player player)
    {
        var grabbing = player.Grabbing;

        if (!grabbing.TryGetHeld<T>(out var obj))
            return;

        if (!CanAccept(obj))
            return;

        grabbing.ReleaseHeldObject(isExternal: true);

        CurrentObject = obj;
        var rb = CurrentObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        SnapIntoPlace(CurrentObject).Forget();
        OnObjectInserted(CurrentObject);
    }

    public void ForceInsert(T obj)
    {
        if (CurrentObject != null)
            Clear();

        CurrentObject = obj;
        var rb = CurrentObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        SnapIntoPlace(CurrentObject).Forget();
        OnObjectInserted(CurrentObject);
    }
    

    public override void Show(bool active) { }

    protected virtual bool CanAccept(T obj) => !IsOccupied;

    protected virtual void OnObjectInserted(T obj)
    {
        OnInserted?.Invoke(obj);
    }

    private async UniTask SnapIntoPlace(T obj)
    {
        Transform t = obj.transform;

        // Reparent immediately (not after the tween) so the cube is safely owned by this slot
        // from the very start. If a level transition interrupts the animation below, the cube
        // will still be correctly destroyed/kept along with the slot instead of being left
        // behind as an orphan at whatever position the tween happened to freeze on.
        Vector3 worldStartPos = t.position;
        Quaternion worldStartRot = t.rotation;

        t.SetParent(transform);
        t.position = worldStartPos;
        t.rotation = worldStartRot;

        Vector3 startLocalPos = t.localPosition;
        Quaternion startLocalRot = t.localRotation;
        float elapsed = 0f;

        while (elapsed < _snapDuration)
        {
            if (t == null) return; // object was destroyed mid-tween (e.g. level transition) - nothing left to animate

            float progress = Mathf.SmoothStep(0f, 1f, elapsed / _snapDuration);
            t.localPosition = Vector3.Lerp(startLocalPos, Vector3.zero, progress);
            t.localRotation = Quaternion.Slerp(startLocalRot, Quaternion.identity, progress);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        if (t == null) return;

        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
    }

    public override void Clear()
    {
        CurrentObject = null;
        OnCleared?.Invoke();
    }
}
