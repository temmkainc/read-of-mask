using System.Collections;
using UnityEngine;

public class DeactivateTriggerLine : MonoBehaviour
{
    [SerializeField] private LevelDoor _door;
    [SerializeField] private DoorInteractable _doorInteractable;
    [SerializeField] private float _lineLength = 5f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private GameObject[] _objectsToDestroy;

    private Vector3 _lastPlayerPosition;
    private Player _player;
    private bool _triggered = false;

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        if (_player != null)
            _lastPlayerPosition = _player.transform.position;
    }

    private void Update()
    {
        if (_triggered || _player == null) return;

        Vector3 playerPos = _player.transform.position;

        Vector3 moveDir = playerPos - _lastPlayerPosition;
        float moveDist = moveDir.magnitude;

        if (moveDist > 0f)
        {
            Vector3 lineStart = transform.position - transform.right * _lineLength * 0.5f;
            Vector3 lineEnd = transform.position + transform.right * _lineLength * 0.5f;

            if (SegmentsCross(_lastPlayerPosition, playerPos, lineStart, lineEnd))
            {
                Trigger();
            }
        }

        _lastPlayerPosition = playerPos;
    }

    private void Trigger()
    {
        _triggered = true;
        StartCoroutine(CloseAndDestroy());
    }

    private IEnumerator CloseAndDestroy()
    {
        if (_door != null)
        {
            _door.Close();
            yield return new WaitForSeconds(2f);
        }

        if (_doorInteractable != null)
        {
            _doorInteractable.CloseAndDisable();
            yield return new WaitForSeconds(2f);
        }

        foreach (var obj in _objectsToDestroy)
        {
            Destroy(obj);
        }
    }

    private bool SegmentsCross(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        Vector2 p1 = new Vector2(a1.x, a1.z);
        Vector2 p2 = new Vector2(a2.x, a2.z);
        Vector2 p3 = new Vector2(b1.x, b1.z);
        Vector2 p4 = new Vector2(b2.x, b2.z);

        float d1 = Cross(p3, p4, p1);
        float d2 = Cross(p3, p4, p2);
        float d3 = Cross(p1, p2, p3);
        float d4 = Cross(p1, p2, p4);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        return false;
    }

    private float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _triggered ? Color.red : Color.cyan;
        Vector3 lineStart = transform.position - transform.right * _lineLength * 0.5f;
        Vector3 lineEnd = transform.position + transform.right * _lineLength * 0.5f;
        Gizmos.DrawLine(lineStart, lineEnd);
        Gizmos.DrawSphere(transform.position, 0.05f);
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawCube(transform.position, new Vector3(_lineLength, 2f, 0.05f));
    }
}