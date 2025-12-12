using UnityEngine;

// Simple follow cam like Subway Surfers: stays behind/above the player with a smooth damp.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;

    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (target != null)
        {
            // Use the starting relative position so the camera stays where you placed it.
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
