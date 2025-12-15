using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleData", menuName = "EndlessRunner/Collectible")]
public class CollectibleData : ScriptableObject
{
    public GameObject prefab;
    public int points = 1;
    [Range(0f, 1f)] public float bonusChance = 0.15f;
    public int bonusPoints = 5;
    public float bonusDuration = 10f;
    public AudioClip pickSfx;
}
