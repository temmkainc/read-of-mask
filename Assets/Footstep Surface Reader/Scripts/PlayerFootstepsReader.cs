using UnityEngine;
using FSR;

public class PlayerFootstepsReader : MonoBehaviour
{
    [SerializeField] private FSR_Data _data;
    [SerializeField] private float _raycastSize = 2.0f;
    [SerializeField] private Transform _foot;
    [SerializeField] private AudioSource _audioSource;

    [Header("Step Timing")]
    [SerializeField] private float _minStepInterval = 0.6f; 
    [SerializeField] private float _maxStepInterval = 0.3f; 

    private float _stepTimer;
    private int _lastIndex = -1;

    private const float VOLUME_SCALE = 0.15f;

    public void UpdateFootsteps(float speedNormalized)
    {
        if (speedNormalized <= 0.05f)
        {
            _stepTimer = 0f;
            return;
        }

        float stepInterval = Mathf.Lerp(_minStepInterval, _maxStepInterval, speedNormalized);

        _stepTimer += Time.deltaTime;

        if (_stepTimer >= stepInterval)
        {
            _stepTimer = 0f;
            Step();
        }
    }

    private void Step()
    {
        if (!Physics.Raycast(_foot.position, -_foot.up, out RaycastHit hit, _raycastSize))
            return;

        var simple = hit.transform.GetComponent<FSR_SimpleSurface>();
        if (simple != null)
        {
            TryPlay(simple.GetSurface());
            return;
        }

        var tagged = hit.transform.GetComponent<FSR_TagedSurface>();
        if (tagged != null)
        {
            TryPlay(tagged.GetSurface());
            return;
        }

        var terrain = hit.transform.GetComponent<FSR_TerrainSurface>();
        if (terrain != null)
        {
            TryPlay(terrain.GetSurface(transform.position));
            return;
        }

        TryPlay("GENERIC");
    }

    private void TryPlay(string surfaceName)
    {
        foreach (var surface in _data.surfaces)
        {
            if (surface.name.Equals(surfaceName))
            {
                PlaySound(surface);
                return;
            }
        }
    }

    private void PlaySound(FSR_Data.SurfaceType surfaceType)
    {
        var clips = surfaceType.soundEffects;
        if (clips == null || clips.Length == 0) return;

        int index;
        do
        {
            index = Random.Range(0, clips.Length);
        }
        while (index == _lastIndex && clips.Length > 1);

        _lastIndex = index;

        AudioManager.Instance.PlaySFX(clips[index], volumeScale: VOLUME_SCALE, source: _audioSource);
    }
}