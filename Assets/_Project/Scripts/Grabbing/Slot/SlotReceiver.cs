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

        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;
        float elapsed = 0f;

        while (elapsed < _snapDuration)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / _snapDuration);
            t.position = Vector3.Lerp(startPos, transform.position, progress);
            t.rotation = Quaternion.Slerp(startRot, transform.rotation, progress);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        t.SetParent(transform);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
    }

    public override void Clear()
    {
        CurrentObject = null;
        OnCleared?.Invoke();
    }
}