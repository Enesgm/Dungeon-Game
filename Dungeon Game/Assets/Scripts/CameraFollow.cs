using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -10);
    public float smoothSpeed = 0.1f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        Vector3 velocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothSpeed);
        transform.LookAt(target);
    }
}
