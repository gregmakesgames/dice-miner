using UnityEngine;

[DisallowMultipleComponent]
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float smoothTime = 0.15f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private bool ignoreZ = true;

    private Vector3 velocity;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public void SnapTo(Transform newTarget)
    {
        target = newTarget;
        if (target == null)
        {
            return;
        }

        velocity = Vector3.zero;
        transform.position = ComputeDesired();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = ComputeDesired();
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    private Vector3 ComputeDesired()
    {
        Vector3 t = target.position;
        float z = ignoreZ ? transform.position.z : t.z;
        return new Vector3(t.x + offset.x, t.y + offset.y, z);
    }
}
