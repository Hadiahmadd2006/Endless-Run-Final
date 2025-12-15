using UnityEngine;

public class SimpleObstacle : MonoBehaviour
{
    public AudioClip hitSfx;

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.PlaySfx(hitSfx);
            player.Die();
        }
    }
}
