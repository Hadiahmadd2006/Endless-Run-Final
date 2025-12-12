using UnityEngine;

// Simple follow cam like Subway Surfers: stays behind/above the player with a smooth damp.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(-10f, 6f, 0f); // behind (x-), above (y+), centered on z
    public float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
