using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Patrol : MonoBehaviour
{
    public List<Transform> patrolPoints;

    private LinkedList<Transform> patrolPath = new LinkedList<Transform>();
    private LinkedListNode<Transform> currentPatrolNode;
    private int currentPointIndex = 0;

    private NavMeshAgent agent;
    public bool isAttacking = false;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        //Create a for loop to convert the list into a linked list
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            patrolPath.AddLast(patrolPoints[i]);
        }

        //Start at the first point of the node
        currentPatrolNode = patrolPath.First;
        if (currentPatrolNode != null)
        {
            agent.SetDestination(currentPatrolNode.Value.position);
        }
    }
    private void Update()
    {
        if (isAttacking || currentPatrolNode == null)
        {
            return;
        }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
         {
             //Move to the next node
             currentPatrolNode = currentPatrolNode.Next;

             //Loop back to the first node
             if (currentPatrolNode == null)
             {
                 currentPatrolNode = patrolPath.First;
             }

             agent.SetDestination(currentPatrolNode.Value.position);
         }
    }
}