using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleData data;
    public AudioClip collectSfx;
    public float rotationSpeed = 180f;

    void Awake()
    {
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
