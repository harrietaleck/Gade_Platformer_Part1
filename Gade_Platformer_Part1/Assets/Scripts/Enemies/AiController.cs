using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AiController : MonoBehaviour
{//State declaration
    public State currentState;
    private Dictionary<State, Action> stateActions;

    //Collider Checkers
    public bool playerInChase;
    public bool playerInAttack;

    //Patrol Declaration
    public List<Transform> patrolPoints;

    private LinkedList<Transform> patrolPath = new LinkedList<Transform>();
    private LinkedListNode<Transform> currentPatrolNode;

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
    public float chaseRange = 10f;
    public float attackRange = 2f;
    private float distanceToPlayer;


    //State declaration
    public enum State
    {
        Patrolling,
        Chasing,
        Attacking,
        Return
    }
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerData = player.GetComponent<PlayerCheckpointDatat>();

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
        };
    }
    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        //Check if the player is in chase range or attcak range
        playerInAttack = distanceToPlayer <= attackRange;
        playerInChase = distanceToPlayer <= chaseRange;
        stateActions[currentState].Invoke();
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
            currentPatrolNode = currentPatrolNode.Next;

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
                    playerData.LoseLife();
                //Reset the hit counter
                hitCounter = 0;
                //Reset the attack damage threshold
                attackDamageThreshold = Random.Range(3, 6);
                //Perform attack logic here(health decrease must be called)
                Debug.Log("Attacking the player!");
            }
        }
    }
    // Change the access modifier of the BounceOffPLY method to public to fix the CS0122 error.
    public void BounceOffPLY(Collision collision)
    {
        //Move the ememy away after attacking the player
        Vector3 direction = (transform.position - collision.transform.position).normalized;

        transform.position += direction * bounceForce;
        agent.isStopped = false;

        /*if (Time.time - lastAttackTime >= coolDownPoint)
        {
            lastAttackTime = Time.time;
            Vector3 bounceDirection = (transform.position - collision.transform.position).normalized;
            agent.Move(bounceDirection * bounceForce);
        }
        //Return to chase state if the player is in range
        if (playerInChase)
        {
            currentState = State.Chasing;
        }
        else if (!playerInChase)
        {
            currentState = State.Return;
        }*/
    }
    void ChaseState()
    {
        //Call the chase function
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
}