using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossAIagent : MonoBehaviour
{
    //Declare variables
    private NavMeshAgent agent;
    public string startNode = "PathA";
    public float speed;
    private bool isJumping;
    [SerializeField]
    private Node currentNode;
    private Node previousNode;
    private Node targetNode;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 5f;
        agent.autoTraverseOffMeshLink = true;

        currentNode = GraphSetup.Graphs.GetNode(startNode);
        transform.position = currentNode.Waypoint.position;
        StartCoroutine(Patrol());
        ChooseNextNode();
    }

    private IEnumerator Patrol()
    {
        //Check if the current node is null
        while (true)
        {
            Move();
            yield return null;
        }
    }
    
    private void Move()
    {
        if (targetNode == null)
        {
            return;
        }
        //Set the direction of the boss to move towards the target node
        //Vector3 direction = (targetNode.Waypoint.position - transform.position).normalized;
        //Calculate the distance between the boss and the target node
        //transform.position += direction * speed * Time.deltaTime;
        if (isJumping)
            return;

        if (targetNode == null)
            return;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            previousNode = currentNode;
            currentNode = targetNode;

            ChooseNextNode();
        }
    }

    private void ChooseNextNode()
    {
         if (currentNode.PathCount == 0)
         {
             Debug.LogError("No paths available from current node: " + currentNode.ID);
             return;
         }
        //Declare variables
        /*Node[] validNodes = new Node[currentNode.PathCount];
        int count = 0;

        //Create a for loop to loop the paths
        for (int i = 0; i < currentNode.PathCount; i++)
        {
            Node paths = currentNode.GetPaths(i);

            //check if the paths are not equal to the previous node or if the current node has only one path
            if (paths != previousNode || currentNode.PathCount == 1)
            {
                validNodes[count] = paths;
                count++;
            }
        }*/
        Node chosen = null;
        int safety = 0;

        while (chosen == null && safety < 30)
        {
            Node candidate =
                currentNode.GetPaths(Random.Range(0, currentNode.PathCount));

            //Prevent the boss from going back and forth between two nodes, unless there is no other option
            if (candidate == previousNode && currentNode.PathCount > 1)
            {
                safety++;
                continue;
            }

            if (CanReach(candidate))
            {
                chosen = candidate;
            }
            else
            {
                //If the candidate node is not reachable, try to jump to it if it's within a certain distance
                StartCoroutine(JumpToNode(candidate));
                return;
            }

            safety++;
        }

        if (chosen == null)
        {
            Debug.LogWarning("No valid node found, fallback used.");
            return;
        }

        //Set the target node to a random valid node
        targetNode = chosen;
        agent.SetDestination(targetNode.Waypoint.position);
    }
    //Check if the boss can reach the target node
    private bool CanReach(Node target)
    {
        NavMeshPath path = new NavMeshPath();

        agent.CalculatePath(target.Waypoint.position, path);

        return path.status == NavMeshPathStatus.PathComplete;
    }
    private IEnumerator JumpToNode(Node node)
    {
        //Declare variables
        isJumping = true;
        agent.isStopped = true;
        agent.updatePosition = false;

        Vector3 start = transform.position;
        Vector3 end = node.Waypoint.position;

        float targetTime = 0f;
        float jumpDuration = 3f;
        //Check if the boss can reach the target node
        while (targetTime < 1f)
        {
            targetTime += Time.deltaTime / jumpDuration;

            Vector3 pos = Vector3.Lerp(start, end, targetTime);
            pos.y += Mathf.Sin(targetTime * Mathf.PI) * 2f; // jump arc

            transform.position = pos;
            //Look in the direction of the jump
            Vector3 lookDir = (end - start).normalized;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
            yield return null;
        }

        transform.position = end;

        agent.Warp(end);//Instantly move the agent to the new position
        agent.updatePosition = true;
        agent.isStopped = false;

        currentNode = node;//Update the current node to the new node
        isJumping = false;

        ChooseNextNode();
    }
}
