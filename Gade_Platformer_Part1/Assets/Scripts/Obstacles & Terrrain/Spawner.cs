 using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] spawnObjects;
    public Collider spawnArea;

    public int spawnCount = 5; 
    void Start()
    {
        Spawnner();
    }
    void Spawnner()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            if (gameObject.name == "Platforms")
            {
                if (spawnObjects.Length == 0) return;

                //Pick a random oject from the array
                GameObject random = spawnObjects[Random.Range(0, spawnObjects.Length)];

                Bounds bounds = spawnArea.bounds;
                //Get radom positions on the object
                float x = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
                float z = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);

                Vector3 spawnPos = new Vector3(x, bounds.max.y, z);

                //Then spawn it
                Instantiate(random, spawnPos, Quaternion.identity);
            }
        }
    }
}
