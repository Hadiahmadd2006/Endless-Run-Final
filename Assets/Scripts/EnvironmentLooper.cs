using UnityEngine;

// Simple two-segment looper along -Z. Assign two chunks and a player.
public class EnvironmentLooper : MonoBehaviour
{
    public Transform player;
    public Transform segmentA;
    public Transform segmentB;
    public float scrollSpeed = 15f;       // how fast the world moves toward -Z
    public float segmentLength = 100f;
    public float triggerOffset = 10f; // move a segment when player is this far past it
    public float recycleBuffer = 420f;    // how far past the player the trailing edge must travel before recycling

    private float furthestZ;
    private Vector3 initialPosA;
    private Vector3 initialPosB;

    void Awake()
    {
        if (segmentA != null) initialPosA = segmentA.position;
        if (segmentB != null) initialPosB = segmentB.position;
    }

    void Start()
    {
        if (segmentA == null || segmentB == null || player == null)
        {
            enabled = false;
            return;
        }

        ResetLoop();
    }

    void Update()
    {
        if (player == null) return;

        // Move both segments toward +Z to simulate the player moving toward -Z.
        float dz = scrollSpeed * Time.deltaTime;
        segmentA.position += new Vector3(0f, 0f, dz);
        segmentB.position += new Vector3(0f, 0f, dz);

        // Track the back-most Z after movement (most negative)
        furthestZ = Mathf.Min(segmentA.position.z, segmentB.position.z);

        HandleSegment(segmentA);
        HandleSegment(segmentB);
    }

    public void ResetLoop()
    {
        if (segmentA != null) segmentA.position = initialPosA;
        if (segmentB != null) segmentB.position = initialPosB;

        // Initialize furthestZ as the min (back-most) Z among the two
        if (segmentA != null && segmentB != null)
        {
            furthestZ = Mathf.Min(segmentA.position.z, segmentB.position.z);
        }
    }

    void HandleSegment(Transform seg)
    {
        // Compute trailing edge assuming pivot is at the front of the mesh on +Z; adjust if your pivot differs.
        float trailingEdgeZ = seg.position.z - segmentLength;

        // Wait until the trailing edge is well past the player (e.g., reaches -recycleBuffer) before recycling.
        if (trailingEdgeZ >= player.position.z - recycleBuffer)
        {
            float newZ = furthestZ - segmentLength;
            seg.position = new Vector3(seg.position.x, seg.position.y, newZ);
            furthestZ = newZ;
        }
    }
}
