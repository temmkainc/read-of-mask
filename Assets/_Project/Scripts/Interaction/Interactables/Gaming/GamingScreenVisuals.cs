using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GamingScreenVisuals
{
    private readonly Image _loadingBarImage;
    private readonly float _totalDuration;

    public GamingScreenVisuals(Image loadingBarImage, float totalDuration = 1f)
    {
        _loadingBarImage = loadingBarImage;
        _totalDuration = totalDuration;
        Reset();
    }

    /// <summary>
    /// Simulates the loading bar animation with random freezes.
    /// </summary>
    public async UniTask SimulateLoadingAsync(CancellationToken token = default)
    {
        _loadingBarImage.fillAmount = 0f;

        float freezePoint1 = UnityEngine.Random.Range(0.25f, 0.45f);
        float freezePoint2 = UnityEngine.Random.Range(0.65f, 0.9f);

        float elapsed = 0f;

        try
        {
            while (elapsed < _totalDuration)
            {
                token.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / _totalDuration);
                _loadingBarImage.fillAmount = progress;

                if (Mathf.Abs(progress - freezePoint1) < 0.01f)
                    await UniTask.Delay(UnityEngine.Random.Range(200, 400), cancellationToken: token);

                if (Mathf.Abs(progress - freezePoint2) < 0.01f)
                    await UniTask.Delay(UnityEngine.Random.Range(200, 400), cancellationToken: token);

                await UniTask.Yield(token);
            }
        }
        catch (OperationCanceledException)
        {
            Reset();
            throw;
        }

        _loadingBarImage.fillAmount = 1f;
    }

    public void Reset()
    {
        _loadingBarImage.fillAmount = 0f;
    }
}