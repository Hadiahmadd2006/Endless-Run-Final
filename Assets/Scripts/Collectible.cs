using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleData data;
    public AudioClip collectSfx; // optional override
    public float rotationSpeed = 180f; // degrees per second, coins spin by default

    void Awake()
    {
        // Ensure physics uses triggers so OnTriggerEnter fires reliably.
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        if (Mathf.Abs(rotationSpeed) > 0.01f)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Allow collider on child; look on parent chain.
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;
        HandleCollect(player);
    }

    public void HandleCollect(PlayerController player)
    {
        if (player == null) return;
        int pts = data != null ? data.points : 1;
        GameManager.Instance?.AddScore(pts);

        AudioClip clipToPlay = collectSfx != null ? collectSfx : (data != null ? data.pickSfx : null);
        player.PlaySfx(clipToPlay);
        Destroy(gameObject);
    }
}
