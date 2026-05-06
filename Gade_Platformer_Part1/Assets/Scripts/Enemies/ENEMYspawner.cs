using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    /*public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    private NavMeshAgent agent;
    public bool isAttacking = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);// Set the destionation to the first patrol point
        }

    }
    private void Update()
    {
        if (isAttacking)
        {
            return;
        }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }
}*/
}
        // Spawn Declaration
        /*public List<GameObject> animalPrefabs;
        public List<Transform> spawnPoints;

        void Start()
        {
            SpawnAnimals();
        }

        void SpawnAnimals()
        {
            //Generate a random index to select a random enemy prefab from the list
            //int randomIndex = Random.Range(0, animalPrefabs.Count);

            /*int count = Mathf.Min(animalPrefabs.Count, spawnPoints.Count);

            for (int i = 0; i < count; i++)
            {
                //Place the enemy at the spawn point 
                animalPrefabs[i].position = spawnPoints[i].position;
            }*/

        // copy spawn points so we can remove used ones
        /*List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < animalPrefabs.Count; i++)
        {
            if (availableSpawns.Count == 0) break;

            int randIndex = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[randIndex];

            animalPrefabs[i] = chosenSpawn.position;

            availableSpawns.RemoveAt(randIndex); // prevents duplicates
        }

    }*/