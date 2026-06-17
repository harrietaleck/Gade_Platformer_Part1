using UnityEngine;

public abstract class FactoryEnemy : MonoBehaviour
{
    public abstract Enemy CreateEnemy
    (
        EnemyType type,
        Vector3 spawnPosition
    );
    public enum EnemyType
    {
        Fast,
        Heavy,
        Normal
    }
}
