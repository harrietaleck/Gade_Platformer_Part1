// Deprecated: legacy state machine, never wired up. Player.cs drives
// the Animator directly via the "State" int parameter. Safe to delete
// from the Project window.
// PlayerStateMachine.cs � Sound Effect System (Part 3 D3)
// Contains: HashMapNode, CustomHashMap, SFXManager, SFXBootstrap
// Placed here (outside Assets/Scripts/) because that folder is behind a
// Windows junction that blocks Unity's file watcher.
using UnityEngine;


// ============================================================
// PlayerStateMachine.cs�  Sound Effect System (Part 3 D3)
//
// This file contains four things:
//   1. HashMapNode<TKey,TValue>   � one node in the hash map's chain
//   2. CustomHashMap<TKey,TValue> � hand-written generic hash map ADT
//   3. SFXManager                 � singleton that stores clips in the
//                                   hash map and plays them by key
//   4. SFXBootstrap               � auto-adds SFXManager at game start
//
// WHY IT LIVES HERE:
//   Assets/Scripts/ is behind a Windows junction (Assets/Scenes/Scripts/)
//   which blocks Unity's file watcher. This folder is NOT behind that
//   junction, so Unity detects and compiles it correctly.
// ============================================================

// ============================================================
// HashMapNode<TKey, TValue>
//
// A single key-value entry in the hash map's linked list chain.
// When two keys hash to the same bucket index (a collision),
// they form a chain via the 'Next' pointer � this is called
// separate chaining collision resolution.
// ============================================================
public class HashMapNode<TKey, TValue>
{
    public TKey Key;    // the lookup key (e.g. "jump")
    public TValue Value;  // the stored value (e.g. an AudioClip)

    // Points to the next node in this bucket's chain.
    // null means this is the last (or only) node in the chain.
    public HashMapNode<TKey, TValue> Next;

    public HashMapNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        Next = null; // new nodes start with no chain
    }
}

// ============================================================
// CustomHashMap<TKey, TValue>  �  Hand-Written Hash Map (Part 3 D3)
//
// Implements a hash map using separate chaining:
//   - An array of 'buckets', each being the head of a linked list.
//   - Keys are mapped to a bucket index via GetHashCode() % capacity.
//   - Collisions are resolved by appending to the linked list at
//     that bucket (separate chaining).
//
// Average time complexity: O(1) for Put, Get, ContainsKey, Remove
// (assuming a good hash function and low load factor).
//
// Used by SFXManager to map string keys -> AudioClip references.
// ============================================================
public class CustomHashMap<TKey, TValue>
{
    // Array of bucket heads � each slot is the start of a linked list.
    private HashMapNode<TKey, TValue>[] buckets;

    private int capacity; // total number of buckets
    private int count;    // number of key-value pairs stored

    // Read-only access to internal stats (useful for debugging).
    public int Count => count;
    public int Capacity => capacity;

    // Constructor: choose a prime capacity to reduce clustering.
    // A prime number spreads keys more evenly across buckets.
    public CustomHashMap(int initialCapacity = 37)
    {
        capacity = initialCapacity > 0 ? initialCapacity : 37;
        buckets = new HashMapNode<TKey, TValue>[capacity];
        count = 0;
    }

    // Convert any key to a valid bucket index.
    // Math.Abs prevents negative indices from negative hash codes.
    private int GetBucketIndex(TKey key) =>
        System.Math.Abs(key.GetHashCode()) % capacity;

    // ------------------------------------------------------------------
    // Put � insert a new key-value pair, or update an existing one.
    // Walks the chain at the target bucket:
    //   - If the key is found, update its value in-place.
    //   - If not found, prepend a new node at the bucket head (O(1)).
    // ------------------------------------------------------------------
    public void Put(TKey key, TValue value)
    {
        int index = GetBucketIndex(key);

        // Walk the existing chain looking for a matching key.
        for (var n = buckets[index]; n != null; n = n.Next)
            if (n.Key.Equals(key)) { n.Value = value; return; } // update

        // Key not found � prepend a new node at the head of the chain.
        var node = new HashMapNode<TKey, TValue>(key, value);
        node.Next = buckets[index]; // link old head as next
        buckets[index] = node;           // new node becomes the head
        count++;
    }

    // ------------------------------------------------------------------
    // Get � retrieve a value by key.
    // Throws KeyNotFoundException if the key doesn't exist.
    // Use TryGet instead when absence is a normal case.
    // ------------------------------------------------------------------
    public TValue Get(TKey key)
    {
        for (var n = buckets[GetBucketIndex(key)]; n != null; n = n.Next)
            if (n.Key.Equals(key)) return n.Value;

        throw new System.Collections.Generic.KeyNotFoundException(
            "CustomHashMap: key '" + key + "' not found.");
    }

    // ------------------------------------------------------------------
    // TryGet � safe get that returns false instead of throwing.
    // Used by SFXManager.PlaySound() so missing clips log a warning
    // instead of crashing the game.
    // ------------------------------------------------------------------
    public bool TryGet(TKey key, out TValue value)
    {
        for (var n = buckets[GetBucketIndex(key)]; n != null; n = n.Next)
            if (n.Key.Equals(key)) { value = n.Value; return true; }

        value = default; // set out param to type default (null for objects)
        return false;
    }

    // ------------------------------------------------------------------
    // ContainsKey � check existence without retrieving the value.
    // ------------------------------------------------------------------
    public bool ContainsKey(TKey key)
    {
        for (var n = buckets[GetBucketIndex(key)]; n != null; n = n.Next)
            if (n.Key.Equals(key)) return true;
        return false;
    }

    // ------------------------------------------------------------------
    // Remove � unlink a node from its chain and decrement count.
    // Tracks the previous node so we can splice the target out.
    // Returns false if the key wasn't present.
    // ------------------------------------------------------------------
    public bool Remove(TKey key)
    {
        int index = GetBucketIndex(key);
        HashMapNode<TKey, TValue> prev = null;

        for (var n = buckets[index]; n != null; prev = n, n = n.Next)
        {
            if (!n.Key.Equals(key)) continue;

            // Splice out: if no previous node, the next node becomes head.
            if (prev == null) buckets[index] = n.Next;
            else prev.Next = n.Next;
            count--;
            return true;
        }
        return false; // key wasn't in the map
    }

    // ------------------------------------------------------------------
    // Clear � remove all entries without reallocating the bucket array.
    // Faster than creating a new map when you want to reuse the capacity.
    // ------------------------------------------------------------------
    public void Clear()
    {
        for (int i = 0; i < capacity; i++) buckets[i] = null;
        count = 0;
    }
}

// ============================================================
// SFXManager  �  HashMap-backed Sound Effect Manager (Part 3 D3)
//
// A singleton MonoBehaviour that stores AudioClips in a
// CustomHashMap<string, AudioClip> and plays them by string key.
//
// HOW IT WORKS:
//   1. SFXBootstrap (below) adds this component to GameManager at
//      game start via RuntimeInitializeOnLoadMethod.
//   2. Awake() calls AutoLoadClipsInEditor() which uses AssetDatabase
//      to find all clips by filename � no manual dragging needed.
//   3. Each clip is registered in the CustomHashMap under a key
//      like "jump", "walk", "run", etc.
//   4. Other scripts call SFXManager.Instance.PlaySound("jump") to
//      look up and play the right clip.
//
// [RequireComponent] ensures an AudioSource is always present.
// ============================================================
[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    // Singleton: only one SFXManager exists; accessible from anywhere.
    public static SFXManager Instance { get; private set; }

    // Inspector slots � dragged clips are used in standalone builds.
    // In the Editor, AutoLoadClipsInEditor() fills these automatically.
    [Header("Sound Clips (auto-loaded in Editor; drag here for builds)")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioClip triggerSound;

    // The Part 3 D3 required data structure: maps key -> AudioClip.
    private CustomHashMap<string, AudioClip> soundMap;

    // Cached AudioSource used for PlayOneShot calls.
    private AudioSource audioSource;

    private void Awake()
    {
        // --- Singleton pattern ---
        // Only the first SFXManager survives; extras are destroyed.
        // DontDestroyOnLoad keeps it alive when loading new scenes.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this); // destroy the duplicate component, not the GameObject
            return;
        }

        // Get (or add) the AudioSource this component requires.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // In the Editor, find clips by filename automatically so
        // the designer doesn't need to drag anything into Inspector slots.
        // In standalone builds this block is stripped out.
#if UNITY_EDITOR
        AutoLoadClipsInEditor();
#endif

        // Build the hash map and register every clip.
        // RegisterSound silently skips null clips (missing/unassigned).
        soundMap = new CustomHashMap<string, AudioClip>(17); // 17 is prime
        RegisterSound("jump", jumpSound);
        RegisterSound("land", landSound);
        RegisterSound("walk", walkSound);
        RegisterSound("run", runSound);
        RegisterSound("collect", collectSound);
        RegisterSound("hit", hitSound);
        RegisterSound("checkpoint", checkpointSound);
        RegisterSound("trigger", triggerSound);
    }

#if UNITY_EDITOR
    // ------------------------------------------------------------------
    // AutoLoadClipsInEditor � Editor-only automatic clip discovery.
    // Searches the AssetDatabase by filename so clips load without
    // any manual Inspector wiring during development.
    // Runs inside Awake() at play time in the Editor.
    // ------------------------------------------------------------------
    private void AutoLoadClipsInEditor()
    {
        // Rocky-ground footstep pack (Assets/Classic Footstep SFX/).
        if (jumpSound == null) jumpSound = FindClip("Rocky_ground_jump1");
        if (landSound == null) landSound = FindClip("Rocky_ground_step4");
        if (walkSound == null) walkSound = FindClip("Rocky_ground_walking_loop");
        if (runSound == null) runSound = FindClip("Rocky_ground_running_loop0");

        // Casual Game Sounds pack � search by keyword inside the folder.
        if (collectSound == null) collectSound = FindClipByKeyword("collect", "Assets/Casual Game Sounds U6");
        if (hitSound == null) hitSound = FindClipByKeyword("hit", "Assets/Casual Game Sounds U6");
        if (checkpointSound == null) checkpointSound = FindClipByKeyword("checkpoint", "Assets/Casual Game Sounds U6");
        if (triggerSound == null) triggerSound = FindClipByKeyword("trigger", "Assets/Casual Game Sounds U6");
    }

    // Search the whole project for an AudioClip by exact filename.
    private static AudioClip FindClip(string clipName)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip " + clipName);
        if (guids.Length == 0)
        {
            Debug.LogWarning("SFXManager: clip not found: " + clipName);
            return null;
        }
        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
        AudioClip c = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (c != null) Debug.Log("SFXManager: loaded " + clipName + " from " + path);
        return c;
    }

    // Search a specific folder for a clip whose filename contains 'keyword'.
    private static AudioClip FindClipByKeyword(string keyword, string folder)
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
        foreach (string g in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
            if (System.IO.Path.GetFileNameWithoutExtension(path)
                    .ToLower().Contains(keyword.ToLower()))
            {
                AudioClip c = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (c != null)
                {
                    Debug.Log("SFXManager: loaded " + keyword + " -> " + path);
                    return c;
                }
            }
        }
        Debug.LogWarning("SFXManager: no clip matching '" + keyword + "' in " + folder);
        return null;
    }
#endif

    // ------------------------------------------------------------------
    // RegisterSound � add a clip to the hash map under a string key.
    // Silently skips null clips so missing Inspector slots don't crash.
    // ------------------------------------------------------------------
    public void RegisterSound(string key, AudioClip clip)
    {
        if (clip == null) return;   // nothing to register
        soundMap.Put(key, clip);    // store in the CustomHashMap
    }

    // ------------------------------------------------------------------
    // PlaySound � look up the key in the hash map and play the clip.
    // PlayOneShot allows the same clip to overlap itself (e.g. footsteps).
    // Logs a warning if the key has no registered clip.
    // ------------------------------------------------------------------
    public void PlaySound(string key)
    {
        AudioClip clip;
        if (soundMap.TryGet(key, out clip))
            audioSource.PlayOneShot(clip);
        else
            Debug.LogWarning("SFXManager: no clip for key '" + key + "'.");
    }

    // ------------------------------------------------------------------
    // PlaySoundAtPosition � 3D positional audio at a world point.
    // Creates a temporary AudioSource at 'position' so the sound
    // appears to come from that location in the world.
    // ------------------------------------------------------------------
    public void PlaySoundAtPosition(string key, Vector3 position)
    {
        AudioClip clip;
        if (soundMap.TryGet(key, out clip))
            AudioSource.PlayClipAtPoint(clip, position);
    }

    // Returns true if a clip is registered under the given key.
    public bool HasSound(string key) => soundMap.ContainsKey(key);
}

// ============================================================
// SFXBootstrap  �  Automatic Runtime Setup (Part 3 D3)
//
// [RuntimeInitializeOnLoadMethod] tells Unity to call Init()
// automatically after every scene loads � no component in the
// scene and no changes to existing scripts are required.
//
// Init() finds the GameManager GameObject, adds an AudioSource
// (required by SFXManager), then adds SFXManager itself.
// SFXManager.Awake() runs immediately and loads all clips.
// ============================================================
public static class SFXBootstrap
{
    // AfterSceneLoad fires once per scene, after all Awake() calls.
    // GameManager.Instance (set in Awake) is used instead of Find("GameManager"):
    // Find can return a duplicate GameManager that Destroy()s itself, which would
    // destroy the AudioSource and silence all SFX for the rest of the scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        GameObject gm = (GameManager.Instance != null)
            ? GameManager.Instance.gameObject
            : GameObject.Find("GameManager");
        if (gm == null) return; /*�
�
        */ // AudioSource must exist before SFXManager is added
        // because [RequireComponent] checks for it on AddComponent.
        if (gm.GetComponent<AudioSource>() == null)
            gm.AddComponent<AudioSource>();

        // Add SFXManager only if it isn't already on the object
        // (guards against SFXBootstrap running more than once).
        if (gm.GetComponent<SFXManager>() == null)
            gm.AddComponent<SFXManager>();
    }
}
