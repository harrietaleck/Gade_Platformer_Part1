using UnityEngine;

// ============================================================
// MOVEplatform — ping-pong platform with ice look + rider carry
// Moves between pointA and pointB at a steady speed, carries the
// player (CharacterController) so you don't slide off, and applies
// the ice platform material automatically.
// ============================================================
[RequireComponent(typeof(Collider))]
public class MOVEplatform : MonoBehaviour
{
    [Header("Path")]
    public Transform pointA;
    public Transform pointB;

    [Header("Motion")]
    [Tooltip("Units per second along the path.")]
    public float speed = 4f;
    [Tooltip("If true, starts moving as soon as the level loads.")]
    public bool startActive = true;
    [Tooltip("How close to a waypoint before reversing.")]
    public float arriveThreshold = 0.05f;

    [Header("Look")]
    public bool applyIceMaterial = true;
    public Material iceMaterialOverride;

    bool _activated;
    bool _movingToB = true;
    Vector3 _lastPosition;
    CharacterController _rider;
    Material _iceMat;

    public bool IsActive => _activated;

    void Awake()
    {
        EnsureSolidCollider();
        EnsureRideTrigger();
        if (applyIceMaterial)
            ApplyIceLook();
    }

    void Start()
    {
        _lastPosition = transform.position;

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning($"MOVEplatform on '{name}': pointA/pointB missing — disabled.");
            enabled = false;
            return;
        }

        // Start nearer to A so the first trip goes toward B.
        if (Vector3.Distance(transform.position, pointA.position) > 0.25f)
            transform.position = pointA.position;

        _lastPosition = transform.position;
        _movingToB = true;

        if (startActive)
            Activate();
    }

    void Update()
    {
        if (!_activated || pointA == null || pointB == null) return;

        Transform target = _movingToB ? pointB : pointA;
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) <= arriveThreshold)
            _movingToB = !_movingToB;

        // Carry whoever is standing on the platform.
        Vector3 delta = transform.position - _lastPosition;
        if (_rider != null && _rider.enabled && delta.sqrMagnitude > 0f)
            _rider.Move(delta);

        _lastPosition = transform.position;
    }

    public void Activate()
    {
        _activated = true;
    }

    public void Deactivate()
    {
        _activated = false;
    }

    // ── Rider tracking (called by RideTrigger child) ─────────────
    public void RegisterRider(Collider other)
    {
        if (other == null || !other.CompareTag("Player")) return;
        var cc = other.GetComponent<CharacterController>();
        if (cc != null) _rider = cc;
    }

    public void UnregisterRider(Collider other)
    {
        if (other == null) return;
        var cc = other.GetComponent<CharacterController>();
        if (cc != null && _rider == cc)
            _rider = null;
    }

    // ── Setup helpers ────────────────────────────────────────────
    void EnsureSolidCollider()
    {
        // Capsule colliders make terrible walkable tops — prefer a BoxCollider.
        var capsule = GetComponent<CapsuleCollider>();
        var box = GetComponent<BoxCollider>();

        if (capsule != null)
            capsule.enabled = false;

        if (box == null)
        {
            Vector3 worldSize = new Vector3(1f, 0.4f, 1f);
            var rend = GetComponent<Renderer>();
            if (rend != null)
                worldSize = rend.bounds.size;

            box = gameObject.AddComponent<BoxCollider>();
            Vector3 lossy = transform.lossyScale;
            box.size = new Vector3(
                worldSize.x / Mathf.Max(0.0001f, Mathf.Abs(lossy.x)),
                Mathf.Max(0.2f, worldSize.y / Mathf.Max(0.0001f, Mathf.Abs(lossy.y))),
                worldSize.z / Mathf.Max(0.0001f, Mathf.Abs(lossy.z))
            );
            box.center = Vector3.zero;
        }

        box.isTrigger = false;
        box.enabled = true;
    }

    void EnsureRideTrigger()
    {
        Transform existing = transform.Find("RideTrigger");
        if (existing != null) return;

        var go = new GameObject("RideTrigger");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;

        // Slightly taller than the platform so the player stays registered while standing.
        var solid = GetComponent<BoxCollider>();
        if (solid != null)
        {
            box.center = solid.center + Vector3.up * (solid.size.y * 0.55f);
            box.size = new Vector3(solid.size.x * 0.95f, solid.size.y + 1.2f, solid.size.z * 0.95f);
        }
        else
        {
            box.center = new Vector3(0f, 0.8f, 0f);
            box.size = new Vector3(1.1f, 1.6f, 1.1f);
        }

        var ride = go.AddComponent<PlatformRideTrigger>();
        ride.platform = this;
    }

    void ApplyIceLook()
    {
        if (iceMaterialOverride != null)
            _iceMat = iceMaterialOverride;

        if (_iceMat == null)
        {
#if UNITY_EDITOR
            _iceMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Materials/IcePlatformMat.mat");
#endif
            if (_iceMat == null)
                _iceMat = Resources.Load<Material>("IcePlatformMat");
        }

        if (_iceMat == null) return;

        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            // Don't restyle unrelated child meshes if named RideTrigger
            if (r.transform.name == "RideTrigger") continue;
            r.sharedMaterial = _iceMat;
        }
    }
}

// Child trigger that notifies the parent platform when the player is riding.
public class PlatformRideTrigger : MonoBehaviour
{
    public MOVEplatform platform;

    void OnTriggerEnter(Collider other)
    {
        platform?.RegisterRider(other);
    }

    void OnTriggerStay(Collider other)
    {
        platform?.RegisterRider(other);
    }

    void OnTriggerExit(Collider other)
    {
        platform?.UnregisterRider(other);
    }
}
