using UnityEngine;

public class AIController : MonoBehaviour
{
    /*[Header("Required")]
    public AIEnemyFactory EnemyFactory;
    public Transform Hero;
    public PatrolRoute PatrolRoute;

    [Header("Optional")]
    public EnemySpawner Spawner;

    void Awake()
    {
        if (EnemyFactory == null)
            Debug.LogError("AIController: assign Enemy Factory.");

        if (Hero == null)
            Debug.LogWarning("AIController: assign Hero — enemies cannot chase.");

        if (EnemyRegistry.Instance == null)
            Debug.LogError("AIController: add EnemyRegistry to the scene.");

        if (Spawner != null)
        {
            if (Spawner.Factory == null)
                Spawner.Factory = EnemyFactory;

            Spawner.SceneController = this;
        }

        if (EnemyFactory != null)
            EnemyFactory.SceneController = this;
    }

    void Update()
    {
        if (Hero == null || EnemyRegistry.Instance == null)
            return;

        // Only stationary enemies (Heavy) are driven from here.
        // Fast/Normal use AiControllerA on the prefab for patrol/chase/attack.
        foreach (var enemy in EnemyRegistry.Instance.StationaryEnemies)
        {
            if (enemy != null)
                enemy.TickStationaryBehavior(Hero);
        }
    }

    /// <summary>Wire player + patrol path into a freshly spawned enemy.</summary>
    public void SetupEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        var ai = enemy.GetComponent<AiControllerA>();
        if (ai != null)
            ai.Configure(Hero, PatrolRoute, enemy);
    }

    public Enemy SpawnEnemy(FactoryEnemy.EnemyType type, Vector3 position)
    {
        if (EnemyFactory == null)
        {
            Debug.LogError("AIController: EnemyFactory not assigned.");
            return null;
        }

        return EnemyFactory.CreateEnemy(type, position);
    }*/
}
