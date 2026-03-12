using UnityEngine;
using System;

public class PongGoal : MonoBehaviour
{
    public event Action OnGoal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PongBall>())
            OnGoal?.Invoke();
    }
}