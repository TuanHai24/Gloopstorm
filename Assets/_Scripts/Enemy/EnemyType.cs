using UnityEngine;

[CreateAssetMenu(menuName = "VPSV/EnemyType", fileName = "EnemyType")]
public class EnemyType : ScriptableObject
{
    public GameObject prefab;
    [Min(1)] public int cost = 1;
}
