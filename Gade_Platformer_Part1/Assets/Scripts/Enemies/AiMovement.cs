using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AiMovement : MonoBehaviour
{
    public Transform player;

    public int enemyHP = 100;
    private NavMeshAgent agent;
    private Patrol patrolScript;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolScript = GetComponent<Patrol>();

        if (player == null)
        {
            Debug.LogError("Player not attatched");
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