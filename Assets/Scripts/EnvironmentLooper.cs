using UnityEngine;

public class EnvironmentLooper : MonoBehaviour
{
    public Transform player;

    // Subway Surfers environment length
    public float environmentLength = 32f;

    private float triggerZ;

    void Start()
    {
        // Calculate 80% trigger for loop
        triggerZ = transform.position.z + (environmentLength * 0.7f);
    }

    void Update()
    {
        if (player.position.z > triggerZ)
        {
            LoopEnvironment();
        }
    }

    void LoopEnvironment()
    {
        // Move the world forward by one full length
        transform.position += new Vector3(0, 0, environmentLength);

        // Recalculate new trigger position
        triggerZ = transform.position.z + (environmentLength * 0.7f);
    }
}
