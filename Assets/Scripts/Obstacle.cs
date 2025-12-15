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
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.PlaySfx(data != null ? data.hitSfx : null);
            player.Die();
        }
    }
}
