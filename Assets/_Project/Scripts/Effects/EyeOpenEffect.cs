using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EyeOpenEffect : MonoBehaviour
{
    [Header("References")]
    public RawImage eyeOverlay;

    [Header("Settings")]
    public float duration = 1.5f;
    public float startDelay = 0.2f;

    [Header("Realism")]
    public float jitterStrength = 0.015f;
    public float jitterSpeed = 40f;

    private Material _mat;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _mat = eyeOverlay.material = new Material(eyeOverlay.material);
        _mat.SetFloat("_Progress", 0f);
    }
    
    private void OnDisable()
    {
        _cts?.Cancel();
    }

    public async UniTask Play()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _mat.SetFloat("_Progress", 0f);


        var sequence = new (float time, float value)[]
        {
            (0f,    0f),
            (0.12f, 0.2f),
            (0.22f, 0.1f),

            (0.32f, 0.35f),
            (0.42f, 0.2f),

            (0.52f, 0.5f),

            (0.7f,  0.7f),
            (0.85f, 0.9f),
            (1f,    1f)
        };

        await PlaySequence(sequence, _cts.Token);

        eyeOverlay.gameObject.SetActive(false);
    }

    private async UniTask PlaySequence((float time, float value)[] seq, CancellationToken token)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            token.ThrowIfCancellationRequested();

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float value = EvaluateSequence(seq, t);

            float noise = (Mathf.PerlinNoise(Time.time * jitterSpeed, 0f) - 0.5f) * jitterStrength;

            _mat.SetFloat("_Progress", Mathf.Clamp01(value + noise));

            await UniTask.Yield(token);
        }

        _mat.SetFloat("_Progress", 1f);
    }

    private float EvaluateSequence((float time, float value)[] seq, float t)
    {
        for (int i = 0; i < seq.Length - 1; i++)
        {
            if (t >= seq[i].time && t <= seq[i + 1].time)
            {
                float segmentT = Mathf.InverseLerp(seq[i].time, seq[i + 1].time, t);
                segmentT = Mathf.SmoothStep(0f, 1f, segmentT);
                return Mathf.Lerp(seq[i].value, seq[i + 1].value, segmentT);
            }
        }

        return seq[seq.Length - 1].value;
    }
}