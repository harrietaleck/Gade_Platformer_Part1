using UnityEngine;

// ============================================================
// AiAttack.cs  —  Chase + attack the player on approach
//
// When the player enters chaseRange the wolf stops patrolling,
// runs toward the player, and deals damage once inside hitRadius.
// Each hit costs one life (via PlayerCheckpointDatat.LoseLife).
// When lives reach 0, LoseLife shows the Game Over screen.
// ============================================================
public class AiAttack : MonoBehaviour
{
    [Header("Ranges")]
    [Tooltip("Distance at which the wolf notices the player and starts chasing.")]
    [SerializeField] private float chaseRange = 4f;

    [Tooltip("Distance at which the wolf deals damage (bite range).")]
    [SerializeField] private float hitRadius = 1.4f;

    [Header("Combat")]
    [Tooltip("Seconds between consecutive hits.")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Tooltip("Move speed while chasing the player.")]
    [SerializeField] private float chaseSpeed = 6.5f;

    [Tooltip("How quickly the wolf turns to face the player.")]
    [SerializeField] private float turnSpeed = 12f;

    Patrol _patrol;
    Transform _player;
    PlayerCheckpointDatat _checkpointData;
    Player _playerScript;
    float _lastAttackTime = -999f;
    float _visualYawOffset;

    private void Start()
    {
        _patrol = GetComponent<Patrol>();

        Transform visual = transform.Find("WolfVisual");
        _visualYawOffset = visual != null ? visual.localEulerAngles.y : 0f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning($"AiAttack on '{name}': No GameObject tagged 'Player' found.");
            enabled = false;
            return;
        }

        _player = playerObj.transform;
        _checkpointData = playerObj.GetComponent<PlayerCheckpointDatat>();
        if (_checkpointData == null)
            _checkpointData = Object.FindObjectOfType<PlayerCheckpointDatat>();

        _playerScript = playerObj.GetComponent<Player>();

        if (_checkpointData == null)
            Debug.LogWarning($"AiAttack on '{name}': PlayerCheckpointDatat not found — attacks won't reduce lives.");
    }

    private void Update()
    {
        if (_player == null || Time.timeScale <= 0f) return;

        // Already game over — stop chasing / attacking.
        if (_checkpointData != null && _checkpointData.lives <= 0)
        {
            if (_patrol != null) _patrol.isChasing = false;
            return;
        }

        float distance = HorizontalDistance(transform.position, _player.position);

        if (distance <= chaseRange)
        {
            if (_patrol != null)
                _patrol.isChasing = true;

            // Close in until inside bite range, then hold and face the player.
            if (distance > hitRadius * 0.85f)
                MoveToward(_player.position);
            else
                FaceToward(_player.position);

            if (distance <= hitRadius && Time.time - _lastAttackTime >= attackCooldown)
                PerformAttack();
        }
        else
        {
            if (_patrol != null)
                _patrol.isChasing = false;
        }
    }

    void PerformAttack()
    {
        _lastAttackTime = Time.time;

        if (_checkpointData != null)
        {
            _checkpointData.LoseLife();

            // Respawn at last checkpoint only while lives remain.
            // LoseLife already opens Game Over when lives hit 0.
            if (_checkpointData.lives > 0)
            {
                _checkpointData.Death();
                BiteNoticeScreen.ShowBiteNotice();
            }
            else if (_patrol != null)
            {
                _patrol.isChasing = false;
            }
        }

        _playerScript?.TriggerHurt();
        SFXManager.Instance?.PlaySound("hit");
    }

    void MoveToward(Vector3 target)
    {
        Vector3 targetXZ = new Vector3(target.x, transform.position.y, target.z);
        FaceToward(targetXZ);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetXZ,
            chaseSpeed * Time.deltaTime
        );
    }

    void FaceToward(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion faceMove = Quaternion.LookRotation(direction.normalized);
        Quaternion targetRot = faceMove * Quaternion.Euler(0f, -_visualYawOffset, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.45f);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
#endif
}
