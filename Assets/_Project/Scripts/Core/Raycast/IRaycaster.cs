using UnityEngine;

public interface IRaycaster 
{
    bool TryHit<T>(out T component) where T : class;
    bool TryHit<T>(float distance, LayerMask mask, out T component) where T : class;
}
