using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleData data;

    void Awake()
    {
        // Ensure physics uses triggers so OnTriggerEnter fires reliably.
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Allow collider on child; look on parent chain.
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null) HandleHit(player);
    }

    public void HandleHit(PlayerController player)
    {
        if (player == null) return;
        player.PlaySfx(data != null ? data.hitSfx : null);
        player.Die();
    }
}
