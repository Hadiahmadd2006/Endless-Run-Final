using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public PlayerController playerController;

    [Header("Spawn Data")]
    public ObstacleData[] obstacles;
    public CollectibleData[] collectibles;

    [Header("Settings")]
    public float spawnZ = 0f;
    public float spawnInterval = 1f;
    public float laneOffset = 2f;
    public float yPos = 0.5f;
    public float collectibleY = 1f;
    public float obstacleY = 0.2f;
    [Range(0f, 1f)] public float perLaneCollectibleChance = 0.4f;
    [Range(0f, 1f)] public float perLaneObstacleChance = 0.7f;
    [Header("Grounding")]
    public bool useGrounding = false;
    public LayerMask groundMask = ~0;
    public float raycastHeight = 5f;
    public float surfaceOffset = 0.1f;
    public bool parentToGround = false;
    [Range(0f, 1f)]
    public float minGroundNormalY = 0.5f;
    public bool alignToGroundNormal = true;
    public bool keepWorldScaleWhenParented = true;
    [Header("Motion")]
    public float scrollSpeed = 15f;
    public Vector3 moveDirection = Vector3.forward;
    public float despawnBehindPlayer = 20f;
    public float despawnAtZ = 80f;

    private float timer = 0f;
    private readonly List<Transform> spawned = new List<Transform>();

    public void ResetSpawns()
    {
        timer = 0f;
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null)
            {
                Destroy(spawned[i].gameObject);
            }
        }
        spawned.Clear();
    }

    void Reset()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null) player = playerController.transform;
    }

    void Update()
    {
        if (player == null) return;
        var gm = GameManager.Instance;
        if (gm != null && !gm.IsGameplayActive) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnOne();
        }

        MoveAndCull();
    }

    void SpawnOne()
    {
        SpawnRowAtZ(spawnZ);
    }

    void SpawnRowAtZ(float targetZ)
    {
        int[] lanes = { -1, 0, 1 };
        HashSet<int> collectibleLanes = new HashSet<int>();
        for (int i = 0; i < lanes.Length; i++)
        {
            int laneIndex = lanes[i];
            float laneX = laneIndex * laneOffset;
            Transform groundParent = null;
            bool hitFound = false;
            RaycastHit hit = default;

            Vector3 basePos = new Vector3(laneX, yPos, targetZ);

            if (useGrounding)
            {
                Vector3 rayOrigin = new Vector3(laneX, raycastHeight, targetZ);
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundMask) && hit.normal.y >= minGroundNormalY)
                {
                    basePos = hit.point + Vector3.up * surfaceOffset;
                    groundParent = parentToGround ? hit.collider.transform : null;
                    hitFound = true;
                }
            }

            bool tryCollectible = collectibles != null && collectibles.Length > 0 && Random.value < perLaneCollectibleChance;
            bool tryObstacle = obstacles != null && obstacles.Length > 0 && Random.value < perLaneObstacleChance;

            if (tryCollectible && (!tryObstacle || Random.value < 0.5f))
            {
                if (collectibleLanes.Contains(laneIndex - 1) || collectibleLanes.Contains(laneIndex + 1))
                {
                    tryCollectible = false;
                }
            }

            if (tryCollectible && (!tryObstacle || Random.value < 0.5f))
            {
                var data = collectibles[Random.Range(0, collectibles.Length)];
                if (data != null && data.prefab != null)
                {
                    Vector3 pos = basePos;
                    if (!useGrounding) pos.y = collectibleY;
                    Quaternion spawnRot = GetSpawnRotation(data.prefab.transform.rotation, hit, hitFound);
                    var go = Instantiate(data.prefab, pos, spawnRot);
                    if (groundParent != null) ParentWithOptionalScalePreserve(go.transform, groundParent);
                    var c = go.GetComponent<Collectible>();
                    if (c != null) c.data = data;
                    spawned.Add(go.transform);
                    collectibleLanes.Add(laneIndex);
                    continue;
                }
            }

            if (tryObstacle)
            {
                var data = obstacles[Random.Range(0, obstacles.Length)];
                if (data != null && data.prefab != null)
                {
                    Vector3 pos = basePos;
                    if (!useGrounding) pos.y = obstacleY;
                    Quaternion spawnRot = GetSpawnRotation(data.prefab.transform.rotation, hit, hitFound);
                    var go = Instantiate(data.prefab, pos, spawnRot);
                    if (groundParent != null) ParentWithOptionalScalePreserve(go.transform, groundParent);
                    var o = go.GetComponent<Obstacle>();
                    if (o != null) o.data = data;
                    spawned.Add(go.transform);
                }
            }
        }
    }

    Quaternion GetSpawnRotation(Quaternion prefabRotation, RaycastHit hit, bool hasHit)
    {
        if (alignToGroundNormal && hasHit)
        {
            return Quaternion.FromToRotation(Vector3.up, hit.normal) * prefabRotation;
        }
        return prefabRotation;
    }

    void ParentWithOptionalScalePreserve(Transform child, Transform parent)
    {
        if (!keepWorldScaleWhenParented)
        {
            child.SetParent(parent, worldPositionStays: true);
            return;
        }

        Vector3 worldScale = child.lossyScale;
        child.SetParent(parent, worldPositionStays: true);
        Vector3 parentScale = parent.lossyScale;
        if (parentScale.x != 0 && parentScale.y != 0 && parentScale.z != 0)
        {
            child.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z
            );
        }
    }

    void MoveAndCull()
    {
        if (spawned.Count == 0) return;

        Vector3 dir = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector3.forward;
        float speed = scrollSpeed;
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            Transform t = spawned[i];
            if (t == null)
            {
                spawned.RemoveAt(i);
                continue;
            }

            t.position += dir * speed * Time.deltaTime;

            if (t.position.z >= despawnAtZ)
            {
                Destroy(t.gameObject);
                spawned.RemoveAt(i);
                continue;
            }

            if (player != null)
            {
                float rel = Vector3.Dot(t.position - player.position, dir);
                if (rel < -despawnBehindPlayer)
                {
                    Destroy(t.gameObject);
                    spawned.RemoveAt(i);
                }
            }
        }
    }
}
