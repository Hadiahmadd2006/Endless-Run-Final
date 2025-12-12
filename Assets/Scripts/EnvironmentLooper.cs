using UnityEngine;

/// <summary>
/// Pools two copies of the subway_surfers root and leapfrogs them along +X.
/// Skips the final big segment (by name) in the moving chunks and re-positions
/// a separate copy of that final segment ahead of the player. Also drops a
/// ground plane under each chunk to avoid falling.
/// </summary>
public class EnvironmentLooper : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Transform environmentRoot; // the "subway_surfers" root in the scene
    public string lastSegmentName = "node_7e53937d_1ffc_44a1_b6cb_918bd170f38e_mesh0";

    [Header("Bounds (world X)")]
    public float startX = 233.6f;
    public float endX = 1789.8f;
    public float lastSegmentStartX = 1270.7f;
    public float loopStartX = 1700f;

    [Header("Tuning")]
    public float triggerRatio = 0.7f; // trigger chunk move after this fraction
    public float overlap = 1f;        // slight overlap to avoid seams
    public float groundY = -0.1f;
    public float groundWidthZ = 30f;

    private float environmentLength;
    private float lastOffset;          // local offset of last segment from root
    private Transform chunkA;
    private Transform chunkB;
    private Transform lastPrefab;
    private Transform lastInstance;
    private GameObject groundA;
    private GameObject groundB;
    private static float furthestChunkX = float.MinValue;

    void Start()
    {
        if (player == null || environmentRoot == null)
        {
            enabled = false;
            return;
        }

        environmentLength = (endX > startX) ? (endX - startX) : 1556.2f;

        // Find the last segment inside the root to use as a prefab.
        Transform found = FindChildRecursive(environmentRoot, lastSegmentName);
        if (found == null)
        {
            Debug.LogWarning($"EnvironmentLooper: last segment '{lastSegmentName}' not found.");
        }
        lastPrefab = found;
        if (lastPrefab != null)
        {
            lastOffset = lastPrefab.position.x - environmentRoot.position.x;
            lastPrefab.gameObject.SetActive(false); // hide in pooled chunks
        }

        // First chunk is the existing root.
        chunkA = environmentRoot;
        // Create second chunk as a clone.
        chunkB = Instantiate(environmentRoot.gameObject, environmentRoot.position + new Vector3(environmentLength, 0f, 0f), environmentRoot.rotation).transform;

        // Hide the last segment in the cloned chunk too.
        Transform cloneLast = FindChildRecursive(chunkB, lastSegmentName);
        if (cloneLast != null) cloneLast.gameObject.SetActive(false);

        furthestChunkX = Mathf.Max(chunkA.position.x, chunkB.position.x);

        // Spawn a separate instance of the last segment to place ahead.
        if (lastPrefab != null)
        {
            lastInstance = Instantiate(lastPrefab.gameObject).transform;
            PositionLastSegmentAhead(furthestChunkX);
        }

        // Ground planes
        CreateOrMoveGround(chunkA, ref groundA);
        CreateOrMoveGround(chunkB, ref groundB);
    }

    void Update()
    {
        if (player == null) return;
        if (player.position.x < loopStartX) return;

        HandleChunk(chunkA, ref groundA);
        HandleChunk(chunkB, ref groundB);
    }

    void HandleChunk(Transform chunk, ref GameObject ground)
    {
        float chunkStart = chunk.position.x;
        float triggerX = chunkStart + environmentLength * triggerRatio;

        if (player.position.x > triggerX)
        {
            MoveChunkToFront(chunk, ref ground);
        }
    }

    void MoveChunkToFront(Transform chunk, ref GameObject ground)
    {
        float newX = furthestChunkX + environmentLength - overlap;
        chunk.position = new Vector3(newX, chunk.position.y, chunk.position.z);
        furthestChunkX = newX;

        // Move ground with chunk
        CreateOrMoveGround(chunk, ref ground);

        // Move the separate last segment to line up with this chunk
        if (lastInstance != null)
        {
            PositionLastSegmentAhead(furthestChunkX);
        }
    }

    void PositionLastSegmentAhead(float chunkStartX)
    {
        if (lastInstance == null) return;
        float targetX = chunkStartX + lastOffset;
        lastInstance.position = new Vector3(targetX, lastInstance.position.y, lastInstance.position.z);
        lastInstance.gameObject.SetActive(true);
    }

    void CreateOrMoveGround(Transform chunk, ref GameObject ground)
    {
        float centerX = chunk.position.x + environmentLength * 0.5f;
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Auto";
            ground.transform.SetParent(chunk);
        }
        ground.transform.position = new Vector3(centerX, groundY, chunk.position.z);
        // Plane default size is 10x10; scale to fit length and width
        ground.transform.localScale = new Vector3(environmentLength / 10f, 1f, groundWidthZ / 10f);
    }

    Transform FindChildRecursive(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform child in root)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
