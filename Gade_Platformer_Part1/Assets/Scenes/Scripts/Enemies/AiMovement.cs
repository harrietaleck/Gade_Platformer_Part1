using UnityEngine;
using UnityEngine.AI;

public class AiMovement : MonoBehaviour
{
    public Transform player;

    public int enemyHP = 100;
    private NavMeshAgent agent;
    private Patrol patrolScript;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolScript = GetComponent<Patrol>();
    }

    private void Start()
    {
        // Avoid runtime spam when no NavMesh is baked yet.
        if (agent != null && !agent.isOnNavMesh)
        {
            agent.enabled = false;
            Debug.LogWarning($"NavMeshAgent disabled on {name}: no valid NavMesh under this enemy.");
        }

        if (player == null)
        {
            Debug.LogWarning("Player reference not attached on AiMovement.");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //MoveTowardsPlayer();
        }
    }
}