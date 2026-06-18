// ============================================================
// WolfBossAnimDriver.cs  —  Animation driver for the Wolfboss enemy
//
// Attaches to the Wolfboss_A root GameObject alongside BossAIagent.
// The Wolfboss_Controller has 10 states (walk, run, die, attack2,
// sturn, and more) driven by exit-time transitions. Because the
// controller has NO parameters or AnyState transitions, this script
// uses CrossFade() to switch states from code based on movement
// speed (from NavMeshAgent) and distance to the player.
//
// State flow:
//   Moving slowly → "walk"
//   Moving fast   → "run"
//   Within attack range and cooldown elapsed → "attack2"
//   Die called    → "die"
//   Stun called   → "sturn"
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WolfBossAnimDriver : MonoBehaviour
{
    // ── State names (must match the Wolfboss_Controller state names exactly) ──
    const string StateWalk    = "walk";
    const string StateRun     = "run";
    const string StateAttack  = "attack2";
    const string StateDie     = "die";
    const string StateStun    = "sturn";

    [Header("Thresholds")]
    [Tooltip("Below this NavMeshAgent speed the boss plays 'walk'.")]
    public float walkThreshold = 0.15f;

    [Tooltip("Above this speed the boss switches from 'walk' to 'run'.")]
    public float runThreshold  = 3.5f;

    [Tooltip("Distance at which the boss plays the attack animation.")]
    public float attackRange   = 3.0f;

    [Tooltip("Minimum seconds between attack animation triggers.")]
    public float attackCooldown = 2.0f;

    [Tooltip("CrossFade blend time between states (seconds).")]
    public float crossFadeTime = 0.20f;

    // ── Runtime ──────────────────────────────────────────────────────────────
    private Animator     _animator;
    private NavMeshAgent _agent;
    private Transform    _player;
    private string       _currentState = "";
    private bool         _isDead       = false;
    private bool         _isAttacking  = false;
    private float        _lastAttackTime = -99f;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        // Find the Animator anywhere in the hierarchy — Wolfboss_A stores its
        // Animator on a child model object, not on the root.
        _animator = GetComponentInChildren<Animator>(true);
    }

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        if (_animator == null)
        {
            Debug.LogWarning($"[WolfBossAnimDriver] No Animator found on '{name}' or its children.");
            enabled = false;
            return;
        }

        _animator.applyRootMotion = false;
        _animator.cullingMode     = AnimatorCullingMode.AlwaysAnimate;

        // Begin in the walk state (boss is always moving on patrol).
        CrossFadeTo(StateWalk);
    }

    private void LateUpdate()
    {
        // Dead or mid-attack: don't override the current clip.
        if (_isDead || _isAttacking || _animator == null) return;

        // ── Attack check ────────────────────────────────────────
        if (_player != null)
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= attackRange && Time.time - _lastAttackTime >= attackCooldown)
            {
                _lastAttackTime = Time.time;
                StartCoroutine(PlayAttack());
                return;
            }
        }

        // ── Movement-based state ────────────────────────────────
        Vector3 vel = _agent.velocity;
        vel.y = 0f;
        float speed = vel.magnitude;

        if (speed < walkThreshold)
            CrossFadeTo(StateWalk);
        else if (speed < runThreshold)
            CrossFadeTo(StateWalk);
        else
            CrossFadeTo(StateRun);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called externally (e.g. when the boss's health reaches zero).</summary>
    public void TriggerDie()
    {
        if (_isDead) return;
        _isDead = true;
        StopAllCoroutines();
        CrossFadeTo(StateDie);
    }

    /// <summary>Called externally when the boss is stunned.</summary>
    public void TriggerStun() => StartCoroutine(PlayStun());

    // ── Internal helpers ──────────────────────────────────────────────────────

    private IEnumerator PlayAttack()
    {
        _isAttacking = true;
        CrossFadeTo(StateAttack);

        // Wait for the attack clip to start, then wait for it to (roughly) finish.
        // We sample the clip length after a brief delay to let the transition complete.
        yield return new WaitForSeconds(0.05f);
        float clipLen = GetCurrentClipLength();
        yield return new WaitForSeconds(Mathf.Max(0.5f, clipLen - 0.1f));

        _isAttacking = false;
        // Return to walk so movement state logic resumes next frame.
        CrossFadeTo(StateWalk);
    }

    private IEnumerator PlayStun()
    {
        CrossFadeTo(StateStun);
        yield return new WaitForSeconds(0.05f);
        float clipLen = GetCurrentClipLength();
        yield return new WaitForSeconds(Mathf.Max(0.5f, clipLen - 0.1f));
        CrossFadeTo(StateWalk);
    }

    private void CrossFadeTo(string state)
    {
        if (_currentState == state) return;
        _animator.CrossFade(state, crossFadeTime, 0);
        _currentState = state;
    }

    private float GetCurrentClipLength()
    {
        var info = _animator.GetCurrentAnimatorClipInfo(0);
        if (info.Length > 0) return info[0].clip.length;
        return 1.0f; // safe fallback
    }
}
