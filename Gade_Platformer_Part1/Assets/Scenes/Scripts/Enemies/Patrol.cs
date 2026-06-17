using UnityEngine;

// ============================================================
// Patrol.cs  —  Enemy patrol movement using a Linked List ADT
//
// At Start() the public points[] array is loaded into a PatrolPath
// singly-linked list (custom ADT — not a built-in collection).
// Each frame, Update() moves the enemy toward the current node's
// waypoint and advances currentNode via GetNextNode() when the
// enemy arrives, producing a continuously looping patrol route.
//
// WHY LINKED LIST?
//   The PatrolPath linked list gives O(1) node traversal and
//   makes it easy to extend patrol routes at runtime without
//   resizing arrays.  GetNextNode() handles the loop-back
//   to First automatically.
//
// WHY XZ-ONLY DISTANCE?
//   A full 3D distance check freezes the enemy if the waypoint Y
//   differs even slightly from the enemy's current Y — common on
//   uneven surfaces.  Ignoring Y ensures the patrol loop advances
//   as long as the enemy is horizontally close to the waypoint.
// ============================================================

public class Patrol : MonoBehaviour
{
    [Header("Waypoints (assign in Inspector or via EnemySpawner)")]
    public Transform[] points;

    [Header("Movement")]
    public float speed = 5f;

    // Set true by other scripts (e.g. AiControllerA) to pause patrolling
    // while the enemy is chasing or attacking the player.
    public bool isChasing = false;

    // ---- Linked List ADT fields ----
    // patrolLinkedList: the custom PatrolPath linked list built from points[].
    // currentNode:      tracks which waypoint the enemy is heading for.
    private PatrolPath patrolLinkedList;
    private Patrollnode currentNode;

    void Start()
    {
        // Guard: disable Patrol if no waypoints were assigned.
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning($"Patrol on '{name}': no patrol points assigned. Patrol disabled.");
            enabled = false;
            return;
        }

        // ---- Build the Linked List ----
        // Push every waypoint Transform into the custom PatrolPath linked list.
        // AddLast() appends each node so the patrol order matches the array.
        patrolLinkedList = new PatrolPath();
        foreach (Transform pt in points)
        {
            if (pt != null)
                patrolLinkedList.AddLast(pt);
        }

        // Begin at the first node in the list.
        currentNode = patrolLinkedList.First;

        if (currentNode == null)
        {
            Debug.LogWarning($"Patrol on '{name}': all patrol points were null. Patrol disabled.");
            enabled = false;
        }
    }

    void Update()
    {
        if (!enabled || isChasing || currentNode == null) return;

        // Move toward the current waypoint's position (XZ only).
        MoveTowards(currentNode.Value.position);

        // Check arrival using only X and Z (horizontal plane).
        // Ignoring Y prevents the enemy from freezing when the
        // waypoint is at a slightly different height.
        float xzDistance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(currentNode.Value.position.x, currentNode.Value.position.z)
        );

        if (xzDistance <= 0.2f)
        {
            // ---- Advance via Linked List ----
            // GetNextNode() returns current.Next, or First if we are at the
            // last node — creating an infinite looping patrol.
            currentNode = patrolLinkedList.GetNextNode(currentNode);
        }
    }

    void MoveTowards(Vector3 target)
    {
        // Lock Y: only move on the horizontal plane (XZ).
        // Without this, MoveTowards would also pull the enemy up or down
        // toward the waypoint's Y, potentially lifting it off the platform.
        Vector3 targetXZ = new Vector3(target.x, transform.position.y, target.z);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetXZ,
            speed * Time.deltaTime
        );
    }
}
