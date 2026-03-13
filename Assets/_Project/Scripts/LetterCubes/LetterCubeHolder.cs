using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class LetterCubeHolder : MonoBehaviour, IInteractable, IHighlightable
{
    public event Action<LetterCubeHolder> StateChanged;

    [SerializeField] private float _snapDuration = 0.25f;
    [SerializeField] private char _currentLetter;

    private LetterCube _currentLetterCube;

    private char _correctLetter;

    public bool CanHighlight(PlayerGrabbing grabbing) => !grabbing.IsHolding || grabbing.TryGetHeld<LetterCube>(out _);

    public bool IsCorrect() => _correctLetter == _currentLetter;

    public void Interact(Player player)
    {
        var grabbing = player.Grabbing;

        if (!grabbing.TryGetHeld<LetterCube>(out var cube))
            return;

        grabbing.ReleaseHeldObject();

        _currentLetterCube = cube;
        var rb = _currentLetterCube.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        _currentLetterCube.SetCurrentHolder(this);

        LerpCubeIntoPlace(_currentLetterCube).Forget();

        SetCurrentLetter(_currentLetterCube.Letter);
    }

    public void SetCurrentLetter(char letter = char.MinValue)
    {
        _currentLetter = letter;
        StateChanged?.Invoke(this);
    }

    private async UniTask LerpCubeIntoPlace(LetterCube cube)
    {
        Transform cubeTransform = cube.transform;

        Vector3 startPos = cubeTransform.position;
        Quaternion startRot = cubeTransform.rotation;

        Vector3 targetPos = transform.position;
        Quaternion targetRot = transform.rotation;

        float time = 0f;

        while (time < _snapDuration)
        {
            float t = time / _snapDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            cubeTransform.position = Vector3.Lerp(startPos, targetPos, t);
            cubeTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            time += Time.deltaTime;
            await UniTask.Yield();
        }

        cubeTransform.SetParent(transform);
        cubeTransform.localPosition = Vector3.zero;
        cubeTransform.localRotation = Quaternion.identity;
    }
}
