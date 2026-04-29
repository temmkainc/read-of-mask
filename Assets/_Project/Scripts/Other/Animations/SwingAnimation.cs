using DG.Tweening;
using UnityEngine;

public class SwingAnimation : MonoBehaviour
{
    public enum SwingAxis { X, Y, Z }

    [SerializeField] private SwingAxis _axis = SwingAxis.Z;
    [SerializeField] private float _border = 45f;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private Ease _ease = Ease.InOutSine;

    private void Start()
    {
        Swing();
    }

    private void Swing()
    {
        Vector3 from = AxisVector(-_border);
        Vector3 to = AxisVector(_border);

        transform.localRotation = Quaternion.Euler(from);

        transform.DOLocalRotate(to, _duration)
            .SetEase(_ease)
            .OnComplete(() =>
                transform.DOLocalRotate(from, _duration)
                    .SetEase(_ease)
                    .OnComplete(Swing));
    }

    private Vector3 AxisVector(float angle) => _axis switch
    {
        SwingAxis.X => new Vector3(angle, 0, 0),
        SwingAxis.Y => new Vector3(0, angle, 0),
        SwingAxis.Z => new Vector3(0, 0, angle),
        _ => Vector3.zero
    };
}