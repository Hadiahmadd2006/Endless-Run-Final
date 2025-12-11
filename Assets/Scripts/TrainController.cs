using UnityEngine;

public class TrainController : MonoBehaviour
{
    public TrainData data;
    float speed;

    void Start()
    {
        speed = data.speed;
    }

    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}