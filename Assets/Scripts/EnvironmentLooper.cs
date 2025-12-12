using UnityEngine;

// Simple two-segment looper along -Z. Assign two chunks and a player.
public class EnvironmentLooper : MonoBehaviour
{
    public Transform player;
    public Transform segmentA;
    public Transform segmentB;
    public float segmentLength = 100f;
    public float triggerOffset = 10f; // move a segment when player is this far past it

    private float furthestZ;

    void Start()
    {
        if (segmentA == null || segmentB == null || player == null)
        {
            enabled = false;
            return;
        }

        // Initialize furthestZ as the min (more negative) Z among the two
        furthestZ = Mathf.Min(segmentA.position.z, segmentB.position.z);
    }

    void Update()
    {
        if (player == null) return;

        HandleSegment(segmentA);
        HandleSegment(segmentB);
    }

    void HandleSegment(Transform seg)
    {
        // Player runs toward negative Z. When player is triggerOffset past this segment, move it ahead of the furthest one.
        if (player.position.z < seg.position.z - triggerOffset)
        {
            float newZ = furthestZ - segmentLength;
            seg.position = new Vector3(seg.position.x, seg.position.y, newZ);
            furthestZ = newZ;
        }
    }
}
