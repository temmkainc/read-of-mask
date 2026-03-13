using System;
using UnityEngine;

public class BreakoutLoseZone : MonoBehaviour
{
    public event Action OnLose;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BreakoutBall>())
            OnLose?.Invoke();
    }
}
