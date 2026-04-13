using UnityEngine;

public class HorizontalBarSway : MonoBehaviour
{
    [SerializeField] private float _swayAmplitude = 2f;   
    [SerializeField] private float _swayFrequency = 0.8f; 

    private bool _isActive;
    private Quaternion _baseRotation;

    private void Start()
    {
        _baseRotation = transform.localRotation;
    }

    public void SetActive(bool active)
    {
        _isActive = active;

        if (!active)
            transform.localRotation = _baseRotation;
    }

    private void Update()
    {
        if (!_isActive) return;

        float sway = Mathf.Sin(Time.time * _swayFrequency * Mathf.PI * 2f) * _swayAmplitude;
        transform.localRotation = _baseRotation * Quaternion.Euler(0f,0f, sway);
    }
}