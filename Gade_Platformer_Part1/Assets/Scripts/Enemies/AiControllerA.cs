using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
[RequireComponent(typeof(NavMeshAgent))]
public class AiControllerA : MonoBehaviour
{
    //State declaration
    /*public State currentState;
    private Dictionary<State, Action> stateActions;

    //Collider Checkers
    public bool playerInChase;
    public bool playerInAttack;

    //Patrol Declaration
    private List<Transform> patrolPoints;

    private PatrolPath patrolPath;
    private Patrollnode currentPatrolNode;

    //NavMeshAgent Declaration
    private NavMeshAgent agent;
    public bool isAttacking = false;

    //Attacking Declaration
    public float coolDownPoint = 2f;
    private float lastAttackTime;
    private float bounceForce = 5f;
    private float bouncecoolDownPoint = 2f;
    private int hitCounter = 0;
    private int attackDamageThreshold;

    //Player position Declaration
    public Transform player;
    PlayerCheckpointDatat playerData;

    //Enemy declaration
    public float chaseRange = 5f;
    public float attackRange = 2f;
    private float distanceToPlayer;
    private Enemy enemyData;
    public PatrolRoute patrolRoute;


    //State declaration
    public enum State
    {
        Patrolling,
        Chasing,
        Attacking,
        Return
    }
    private void Start()
    {*/

        /*agent = GetComponent<NavMeshAgent>();
         playerData = player.GetComponent<PlayerCheckpointDatat>();
         enemyData = GetComponent<Enemy>();

         enemyData.Initialize(); // ensures stats are applied

         agent.speed = enemyData.MoveSpeed;
         if (enemyData == null)
         {
             Debug.LogError("Enemy component missing!");
             return;
         }
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

         //Set the enemy to patrolling at the start of the game
         currentState = State.Patrolling;

         //Set the threshold damage for the player
         attackDamageThreshold = Random.Range(3, 6);

         //Set up the state actions
         stateActions = new Dictionary<State, Action>
         {
             {
                 State.Patrolling, PatrolState
             },
             {
                 State.Chasing, ChaseState
             },
             {
                 State.Attacking, AttackState
             },
             {
                 State.Return, ReturnState
             }
         };*/

        /*agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            Debug.LogError("Player not assigned!");
            return;
        }

        playerData = player.GetComponent<PlayerCheckpointDatat>();*/

        //Code issue
        //enemyData = GetComponent<Enemy>();

        /*if (enemyData == null)
        {
            Debug.LogError("Enemy component missing!");
            return;
        }*/

        // Apply Fast/Normal/Heavy stats
        /*Debug.Log("Enemy found: " + GetComponent<Enemy>());
        Debug.Log("Enemy children count: " + GetComponentsInChildren<Enemy>().Length);
        //enemyData.Initialize();//code issue

        // Apply speed to NavMesh
        //agent.speed = enemyData.MoveSpeed;//code issue
        attackDamageThreshold = Random.Range(3, 6);

        currentState = State.Patrolling;

        stateActions = new Dictionary<State, Action>()
    {
        { State.Patrolling, PatrolState },
        { State.Chasing, ChaseState },
        { State.Attacking, AttackState },
        { State.Return, ReturnState }
    };
    }
    private void Update()
    {
        if (player == null) return;
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        //Check if the player is in chase range or attcak range
        playerInAttack = distanceToPlayer <= attackRange;
        playerInChase = distanceToPlayer > attackRange && distanceToPlayer <= chaseRange;
        if (stateActions != null && stateActions.ContainsKey(currentState))
            stateActions[currentState]();
    }
    public void Patrolling()
    {
        if (isAttacking || currentPatrolNode == null)
        {
            return;
        }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            //Move to the next node
            //currentPatrolNode = currentPatrolNode.Next;
            currentPatrolNode = patrolPath.GetNextNode(currentPatrolNode);
            //Loop back to the first node
            if (currentPatrolNode == null)
            {
                currentPatrolNode = patrolPath.First;
            }
            if (currentPatrolNode != null)
                agent.SetDestination(currentPatrolNode.Value.position);
        }
    }

    public void ChasePLY()
    {
        //Set the destination to the player's position
        agent.SetDestination(player.position);
    }
    public void Attacking()
    {
        //Check if player is in range 
        if (!playerInAttack)
        {
            agent.isStopped = false;
            return;
        }

        agent.isStopped = true;
        //Direct the emeny to look at the player
        transform.LookAt(player);

        if (Time.time - lastAttackTime >= bouncecoolDownPoint)
        {
            //Reset the attack timer
            lastAttackTime = Time.time;

            //Increase the hit counter
            hitCounter++;
            Debug.Log("Hit Counter: " + hitCounter);

            if (hitCounter >= attackDamageThreshold)
            {
                Debug.Log("Player hit damage max!");
                //Call the lose life function to decrease the player's health
                if (playerData != null)
                    playerData.lives--;
                //Reset the hit counter
                hitCounter = 0;
                //Reset the attack damage threshold
                attackDamageThreshold = Random.Range(3, 6);
            }
        }
    }
    // Change the access modifier of the BounceOffPLY method to public to fix the CS0122 error.
    public void BounceOffPLY(Collision collision)
    {
        //Move the ememy away after attacking the player
        Vector3 direction = (transform.position - collision.transform.position).normalized;

        //transform.position += direction * bounceForce;
        agent.Move(direction * bounceForce);
        agent.isStopped = false;

        if (Time.time - lastAttackTime >= coolDownPoint)
        {
            lastAttackTime = Time.time;
            Vector3 bounceDirection = (transform.position - collision.transform.position).normalized;
            //agent.Move(bounceDirection * bounceForce);
        }
        //Return to chase state if the player is in range
        if (playerInChase)
        {
            currentState = State.Chasing;
        }
        else if (!playerInChase)
        {
            currentState = State.Return;
        }
    }
    void ChaseState()
    {
        //Set movemnt as always on
        agent.isStopped = false;

        
        //////Call the chase function
        ChasePLY();
        //Transition to return state if the player is out of chase range
        if (!playerInChase)
        {
            currentState = State.Return;
        }

        //Transition to attack state if the player is in attack range
        if (playerInAttack)
        {
            currentState = State.Attacking;
        }
    }
    void AttackState()
    {
        //Call the attack function
        Attacking();
        //Transition to chase state in chase range if not in attack range
        if (!playerInAttack)
        {
            agent.isStopped = false;
            currentState = State.Chasing;
        }
    }
    void PatrolState()
    {
        //Call the patrol function
        Patrolling();
        //Transition to chase state if the player is in chase range
        if (playerInChase)
        {
            currentState = State.Chasing;
        }
       /* if (!enemyData.IsPatrolling)
        {
            return;
        }*/

        /*Patrolling();

        if (playerInChase)
        {
            currentState = State.Chasing;
        }
        
    }
    void Returning()
    {
        //Check if the enemy is at the patrol point
        if (currentPatrolNode != null)
            agent.SetDestination(currentPatrolNode.Value.position);
        //Return to the patrol path
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Patrolling;
        }
    }
    void ReturnState()
    {
        //Call the return function
         Returning();
         //Transition to chase state if the player is in chase range
         if (playerInChase)
         {
             currentState = State.Chasing;
         }
        
    }
    public void SetPatrolPoints(List<Transform> points)
    {
        /*patrolPoints = points;

        patrolPath = new PatrolPath();

        foreach (Transform point in patrolPoints)
        {
            patrolPath.AddLast(point);
        }

        currentPatrolNode = patrolPath.First;

        if (currentPatrolNode != null)
        {
            agent.SetDestination(currentPatrolNode.Value.position);
        }*/
        /*patrolPoints = points;

        patrolPath = new PatrolPath();

        foreach (Transform point in patrolPoints)
        {
            patrolPath.AddLast(point);
        }

        currentPatrolNode = patrolPath.First;

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (currentPatrolNode != null && agent.isOnNavMesh)
        {
            agent.SetDestination(currentPatrolNode.Value.position);
        }
        currentState = State.Patrolling;
        agent.isStopped = false;
    }
    public void InitEnemy()
    {
        enemyData = GetComponent<Enemy>();

        if (enemyData == null)
        {
            Debug.LogError("Enemy not found on InitEnemy()");
            return;
        }

        enemyData.Initialize();
        agent.speed = enemyData.MoveSpeed;
    }*/
}

 
