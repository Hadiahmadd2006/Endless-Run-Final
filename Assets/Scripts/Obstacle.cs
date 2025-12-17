using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleData data;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
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
