using System;
using UnityEngine;

public class BreakoutBrick : MonoBehaviour
{
    public event Action OnDestroyed;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<BreakoutBall>())
        {
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}