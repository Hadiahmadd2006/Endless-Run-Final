using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "EndlessRunner/Obstacle")]
public class ObstacleData : ScriptableObject
{
    public GameObject prefab;
    public AudioClip hitSfx;
    public bool killsPlayer = true;
    public int scorePenalty = 0;
}
