using UnityEngine;
using static FactoryEnemy;

public class AIEnemyFactory : FactoryEnemy
{
    public GameObject fastEnemyPrefab;
     public GameObject heavyEnemyPrefab;
     public GameObject normalEnemyPrefab;

     public override Enemy CreateEnemy
     (
         EnemyType type,
         Vector3 spawnPosition
     )
     {
         GameObject prefab = null;

         switch (type)
         {
             case EnemyType.Fast:
                 prefab = fastEnemyPrefab;
                 break;

             case EnemyType.Heavy:
                 prefab = heavyEnemyPrefab;
                 break;

             case EnemyType.Normal:
                 prefab = normalEnemyPrefab;
                 break;
         }

         GameObject enemyObject =
             Instantiate(prefab, spawnPosition, Quaternion.identity);

         Enemy enemy =
             enemyObject.GetComponent<Enemy>();
         if (enemy == null)
         {
             Debug.LogError("Enemy script missing on prefab!");
             return null;
         }
         if (prefab == null)
         {
             Debug.LogError($"No prefab assigned for {type}");
             return null;
         }
         return enemy;
     }
}
