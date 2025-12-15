using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleData", menuName = "EndlessRunner/Collectible")]
public class CollectibleData : ScriptableObject
{
    public GameObject prefab;
    public int points = 1;
    public AudioClip pickSfx;
}
