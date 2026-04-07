using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class FlowerWithCube : GrabbableObject
{
    [field: SerializeField] public LetterCube CubeToThrow { get; private set; }
    [SerializeField] private Vector3 _firstTimeGrabRotationOffset = Vector3.zero;
    [SerializeField] private float _growDuration = 2f;

    private bool _hasBeenGrabbed = false;
    private Quaternion _activeRotationOffset;
    private Vector3 _cubeOriginalScale;

    public override Quaternion GrabRotationOffset => _activeRotationOffset;

    protected override void Awake()
    {
        base.Awake();
        _activeRotationOffset = Quaternion.Euler(_firstTimeGrabRotationOffset);

        _cubeOriginalScale = CubeToThrow.transform.localScale;
        CubeToThrow.transform.localScale = Vector3.zero;
        CubeToThrow.transform.SetParent(transform);
        CubeToThrow.transform.localPosition = Vector3.zero;
        CubeToThrow.GetComponent<Rigidbody>().isKinematic = true;
        CubeToThrow.GetComponent<Collider>().enabled = false;
    }

    public override void Grab(Player player, Transform holdPoint)
    {
        if (!_hasBeenGrabbed)
        {
            _activeRotationOffset = Quaternion.Euler(_firstTimeGrabRotationOffset);
            _hasBeenGrabbed = true;
            GrowCube().Forget();
        }
        else
        {
            _activeRotationOffset = base.GrabRotationOffset;
        }

        base.Grab(player, holdPoint);
    }

    private async UniTask GrowCube()
    {
        CubeToThrow.GetComponent<Rigidbody>().isKinematic = false;
        CubeToThrow.GetComponent<Collider>().enabled = true;
        await UniTask.WaitForSeconds(0.3f);
        CubeToThrow.transform.SetParent(null);
        CubeToThrow.transform.localScale = Vector3.zero;
        CubeToThrow.transform
            .DOScale(_cubeOriginalScale, _growDuration);
    }
}