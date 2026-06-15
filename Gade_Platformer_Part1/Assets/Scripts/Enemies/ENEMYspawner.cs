using System.Collections.Generic;
using UnityEngine;
using static FactoryEnemy;

public class EnemySpawner : MonoBehaviour
{
    //Enemy Spawner Declaration
    public AIEnemyFactory enemyFactory;
    public List<SpawnGroup> spawnGroups;
    void Start()
    {
        SpawnEnemies();
    }
    void SpawnEnemies()
    {
        //Copy spawn point list
        List<SpawnGroup> availableSpawnPoints = new List<SpawnGroup>(spawnGroups);

        //Shuffle the spawn list
        ShuffleList(availableSpawnPoints);

        EnemyType[] enemyTypes =
        {
            EnemyType.Fast,
            EnemyType.Heavy,
            EnemyType.Normal
        };

        //Spawn enemies at random spawn points
        int spawnCount = Mathf.Min(enemyTypes.Length, availableSpawnPoints.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            //spawn the enemy at the spawn point
            var group = availableSpawnPoints[i];
            Enemy enemyObj = enemyFactory.CreateEnemy(enemyTypes[i],group.spawnPoint.position);
            AiControllerA ai = enemyObj.GetComponent<AiControllerA>();
            //code fix : remove slashes when ai controllera is fixed
           // ai.SetPatrolPoints(group.patrolPoints);
           // ai.InitEnemy();
        }
    }
    void ShuffleList(List<SpawnGroup> list)
    { //Fisher-Yates Shuffle Algorithm
        for (int i = 0; i < list.Count; i++)
        {
            SpawnGroup temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    /*void SpawnEnemies()
    {
        //Copy spawn point list
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        //Shuffle the spawn list
        ShuffleList(availableSpawnPoints);

        EnemyType[] enemyTypes =
        {
        EnemyType.Fast,
        EnemyType.Heavy,
        EnemyType.Normal
    };

        //Spawn enemies at random spawn points
        int spawnCount = Mathf.Min(enemyTypes.Length, availableSpawnPoints.Count);
        for (int i = 0; i < spawnCount; i++)
        {
               //spawn the enemy at the spawn point
               enemyFactory.CreateEnemy(enemyTypes[i],availableSpawnPoints[i].position);
        }
    }*/

}




