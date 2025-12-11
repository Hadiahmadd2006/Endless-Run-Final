using UnityEngine;

[CreateAssetMenu(fileName = "NewTrainData", menuName = "EndlessRunner/Train Data")]
public class TrainData : ScriptableObject
{
    public GameObject trainPrefab;
    public float speed = 25f;
    public float spawnInterval = 7f;
}
