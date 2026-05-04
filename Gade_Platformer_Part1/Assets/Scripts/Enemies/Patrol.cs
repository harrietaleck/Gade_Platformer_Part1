using UnityEngine;
using UnityEngine.AI;

public class Patrol : MonoBehaviour
{
    public Transform[] patrolPoints;
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
   /* public Transform[] points;
    public float speed = 3f;

    private int index = 0;
    public bool isChasing = false;

    void Start()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogError("No patrol points assigned!");
            enabled = false;
        }
    }

    void Update()
    {
        if (!enabled || isChasing) return;

        Transform target = points[index];

        MoveTowards(target.position);
        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            index = (index + 1) % points.Length;
        }
    }

    void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );
    }*/

}
