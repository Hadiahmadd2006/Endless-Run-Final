using UnityEngine;

public class EnvironmentLooper : MonoBehaviour
{
    public Transform player;
    public Transform segmentA;
    public Transform segmentB;
    public float scrollSpeed = 15f;
    public float segmentLength = 100f;
    public float triggerOffset = 10f;
    public float recycleBuffer = 420f;

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
        var gm = GameManager.Instance;
        if (gm != null && !gm.IsGameplayActive) return;
        if (player == null) return;

        float dz = scrollSpeed * Time.deltaTime;
        segmentA.position += new Vector3(0f, 0f, dz);
        segmentB.position += new Vector3(0f, 0f, dz);

        furthestZ = Mathf.Min(segmentA.position.z, segmentB.position.z);

        HandleSegment(segmentA);
        HandleSegment(segmentB);
    }

    public void ResetLoop()
    {
        if (segmentA != null) segmentA.position = initialPosA;
        if (segmentB != null) segmentB.position = initialPosB;

        if (segmentA != null && segmentB != null)
        {
            furthestZ = Mathf.Min(segmentA.position.z, segmentB.position.z);
        }
    }

    void HandleSegment(Transform seg)
    {
        float trailingEdgeZ = seg.position.z - segmentLength;

        if (trailingEdgeZ >= player.position.z - recycleBuffer)
        {
            float newZ = furthestZ - segmentLength;
            seg.position = new Vector3(seg.position.x, seg.position.y, newZ);
            furthestZ = newZ;
        }
    }
}
