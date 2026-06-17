using UnityEngine;

// ============================================================
// SFXManager  �  HashMap-backed Sound Effect Manager (Part 3 D3)
//
// A singleton MonoBehaviour that stores AudioClips in a
// CustomHashMap<string, AudioClip> and plays them by string key.
//
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
    // BeforeSceneLoad runs once at startup — we use it solely to subscribe
    // to sceneLoaded, which then fires every time any scene is loaded.
    // This ensures SFXManager is created as soon as a GameManager-containing
    // scene appears (Beginner, Advanced, Expert) regardless of start order.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                                       UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // If SFXManager already exists (persisted from a previous scene), stop here.
        if (SFXManager.Instance != null) return;

        // Find the GameManager — it hosts the AudioSource + SFXManager.
        // Prefer the static Instance so we never accidentally reference a
        // duplicate that is mid-Destroy.
        GameObject gm = (GameManager.Instance != null)
            ? GameManager.Instance.gameObject
            : GameObject.Find("GameManager");

        if (gm == null) return;   // SplashScreen / MainMenu — no GameManager yet

        // AudioSource must exist before SFXManager is added
        // because [RequireComponent(typeof(AudioSource))] checks for it.
        if (gm.GetComponent<AudioSource>() == null)
            gm.AddComponent<AudioSource>();

        if (gm.GetComponent<SFXManager>() == null)
            gm.AddComponent<SFXManager>();
    }
}
