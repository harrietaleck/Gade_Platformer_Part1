
using UnityEngine;
using UnityEngine.AI;

public class RobotDeliveryRun : MonoBehaviour
{
    public Transform[] deliveryPoints;

    private NavMeshAgent agent;
    private int currentPoint = 0; // Fixed starting index to 0

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // FIX 1:
        // Check if deliveryPoints contains items
        if (deliveryPoints == null || deliveryPoints.Length == 0)
        {
            Debug.LogError("No delivery points assigned!");
            return;
        }

        // FIX 2:
        // Start at the correct point index (0)
        agent.SetDestination(deliveryPoints[currentPoint].position);
    }

    void Update()
    {
        // FIX 3:
        // Corrected the condition to check if the agent has reached the destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // FIX 4:
            // Increment the current point correctly
            currentPoint = (currentPoint + 1) % deliveryPoints.Length;

            // FIX 5:
            // Ensure the index is within range
            agent.SetDestination(deliveryPoints[currentPoint].position);
        }
    }
}