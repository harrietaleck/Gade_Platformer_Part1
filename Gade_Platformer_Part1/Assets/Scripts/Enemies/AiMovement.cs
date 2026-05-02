using UnityEngine;
using UnityEngine.AI;

public class Waypoints : MonoBehaviour
{
    public Transform[] deliveryPoints;

    private NavMeshAgent agent;
    private int currentPoint = 1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Fixed ERROR 1: Check that deliveryPoints contains items
        if (deliveryPoints == null || deliveryPoints.Length == 0)
        {
            Debug.LogError("No delivery points assigned!");
            enabled = false;
            return;
        }

        // Fixed ERROR 2: Start at the correct point index (0)
        agent.SetDestination(deliveryPoints[currentPoint].position);
    }

    void Update()
    {
        // Fixed ERROR 3: Correct condition for arrival
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Fixed ERROR 4: Move to the next point (not skipping)
            currentPoint = (currentPoint + 1) % deliveryPoints.Length;

            // Fixed ERROR 5: Prevent out-of-range crash
            agent.SetDestination(deliveryPoints[currentPoint].position);
        }
    }
}
