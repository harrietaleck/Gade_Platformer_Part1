using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //Enemy Spawner Declaration
    public List<GameObject> enemyPrefabs;
    public List<Transform> spawnPoints;

    void Start()
    {
        SpawnEnemies();
    }
    void SpawnEnemies()
    {
        //Copy spawn point list
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        //Shuffle the spawn list
        ShuffleList(availableSpawnPoints);

        //Spawn enemies at random spawn points
        int spawnCount = Mathf.Min(enemyPrefabs.Count, availableSpawnPoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[i];
            Transform spawnPoint = availableSpawnPoints[i];
            Instantiate(
                enemyPrefabs[i],                // enemy order stays the same
                availableSpawnPoints[i].position,    // positions are randomized
                Quaternion.identity);
        }
    }
    void ShuffleList(List<Transform>list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Transform temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
