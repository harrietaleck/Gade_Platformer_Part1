using UnityEngine;
using UnityEngine.AI;

// Drives the wolf model's Animator based on enemy movement speed.
[RequireComponent(typeof(Enemy))]
public class EnemyWolfAnimator : MonoBehaviour
{
    static readonly int HashIdle  = Animator.StringToHash("breathes");
    static readonly int HashWalk  = Animator.StringToHash("walk");
    static readonly int HashRun   = Animator.StringToHash("Wolf_run");

    Animator _animator;
    NavMeshAgent _agent;
    Patrol _patrol;
    Enemy _enemy;
    Vector3 _lastPosition;

    int _currentStateHash;
    const float MoveThreshold = 0.12f;
    const float CrossFadeDuration = 0.15f;

    void Awake()
    {
        var wolfVisual = transform.Find("WolfVisual");
        if (wolfVisual != null)
            _animator = wolfVisual.GetComponentInChildren<Animator>(true);

        _agent  = GetComponent<NavMeshAgent>();
        _patrol = GetComponent<Patrol>();
        _enemy  = GetComponent<Enemy>();
    }

    void Start()
    {
        _lastPosition = transform.position;

        if (_animator == null)
        {
            Debug.LogWarning($"EnemyWolfAnimator on '{name}': no Animator under WolfVisual.");
            enabled = false;
            return;
        }

        // ── Auto-assign Wolf_animation.controller if missing ────────────────
        // The Beginner scene wolves were saved without a controller on their
        // WolfVisual Animator. This block loads the correct controller at
        // runtime so the wolf animations play immediately without manual
        // Inspector work. In a standalone build the controller must be
        // assigned in the Inspector beforehand (the #else branch warns you).
        if (_animator.runtimeAnimatorController == null)
        {
#if UNITY_EDITOR
            const string CtrlPath = "Assets/Wolf_Animated/Model/Wolf_animation.controller";
            var ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CtrlPath);
            if (ctrl != null)
            {
                _animator.runtimeAnimatorController = ctrl;
                Debug.Log($"[EnemyWolfAnimator] '{name}': auto-assigned '{CtrlPath}'.");
            }
            else
            {
                Debug.LogWarning($"[EnemyWolfAnimator] '{name}': controller not found at '{CtrlPath}'. Run Tools ▶ Fix Wolf Animators.");
                enabled = false;
                return;
            }
#else
            Debug.LogWarning($"[EnemyWolfAnimator] '{name}': no AnimatorController assigned. Assign Wolf_animation.controller in Inspector.");
            enabled = false;
            return;
#endif
        }

        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        _animator.applyRootMotion = false;
        CrossFadeTo(HashIdle, 1f);
    }

    void LateUpdate()
    {
        if (_animator == null) return;

        float moveSpeed = GetMoveSpeed();
        bool isMoving = moveSpeed > MoveThreshold;
        bool isFast = isMoving && IsFastMovement();

        if (!isMoving)
        {
            CrossFadeTo(HashIdle, 1f);
            return;
        }

        int targetHash = isFast ? HashRun : HashWalk;
        float refSpeed = isFast ? Mathf.Max(_enemy != null ? _enemy.MoveSpeed : 8f, 6f) : 3f;
        float animSpeed = Mathf.Clamp(moveSpeed / refSpeed, 0.75f, 1.5f);
        CrossFadeTo(targetHash, animSpeed);
    }

    bool IsFastMovement()
    {
        if (_patrol != null && _patrol.isChasing)
            return true;

        if (_patrol != null && _patrol.speed >= 6f)
            return true;

        return _enemy != null && _enemy.MoveSpeed >= 6f;
    }

    float GetMoveSpeed()
    {
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            var velocity = _agent.velocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude > 0.04f)
                return velocity.magnitude;
        }

        float distance = Vector3.Distance(transform.position, _lastPosition);
        _lastPosition = transform.position;
        return distance / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    void CrossFadeTo(int stateHash, float speed)
    {
        if (_currentStateHash != stateHash)
        {
            _animator.CrossFade(stateHash, CrossFadeDuration, 0);
            _currentStateHash = stateHash;
        }

        _animator.speed = speed;
    }
}
