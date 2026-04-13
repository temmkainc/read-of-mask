using UnityEngine;

public class RhythmPressChallenge
{
    public float PointerT { get; private set; }  
    public float WindowStart { get; private set; }
    public float WindowEnd { get; private set; }
    public bool IsActive { get; private set; }

    private float _pointerSpeed;
    private int _direction = 1;

    public void Activate(float pointerSpeed, float minWindowSize, float maxWindowSize)
    {
        _pointerSpeed = pointerSpeed;
        PointerT = 0f;
        _direction = 1;
        IsActive = true;
        RandomizeWindow(minWindowSize, maxWindowSize);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        PointerT += _direction * _pointerSpeed * deltaTime;

        if (PointerT >= 1f)
        {
            PointerT = 1f;
            _direction = -1;
        }
        else if (PointerT <= 0f)
        {
            PointerT = 0f;
            _direction = 1;
        }
    }

    public bool TryPress()
    {
        return PointerT >= WindowStart && PointerT <= WindowEnd;
    }

    public void RandomizeWindow(float minSize, float maxSize)
    {
        float windowSize = Random.Range(minSize, maxSize);
        WindowStart = Random.Range(0.05f, 1f - windowSize - 0.05f);
        WindowEnd = WindowStart + windowSize;
    }

}