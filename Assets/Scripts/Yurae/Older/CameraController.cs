// Unity
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Update()
    {
        TraceTarget();
    }

    private void TraceTarget()
    {
        if (target == null) return;

        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }
}
