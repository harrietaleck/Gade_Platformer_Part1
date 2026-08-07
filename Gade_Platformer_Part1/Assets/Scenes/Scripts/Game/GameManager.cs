using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Survival stats (deplete over time)
    [Header("Survival Stats")]
    public int lives        = 3;
    public int score        = 0;
    public int thermalStones  = 10;
    public int temperature    = 30;
    public int foodSupplies   = 10;
    public int winterClothing = 0;
    public float timerCount   = 0f;

    [Header("Freeze Timer")]
    [Tooltip("Seconds of freeze exposure before the player dies (game over).")]
    public float freezeLimitSeconds = 60f;
    [Tooltip("How fast freeze builds (1 = real-time seconds).")]
    public float freezeRate = 1f;
    [HideInInspector] public float freezeTime = 0f;
    bool _freezeGameOverTriggered;

    // Collectibles actually picked up (scorecard / progression)
    [Header("Pickup Counters (read-only in play)")]
    public int thermalStonesCollected  = 0;
    public int foodSuppliesCollected   = 0;
    public int winterClothingCollected = 0;

    // Total pickups this scene — resets when a new scene loads
    [HideInInspector] public int pickupsThisScene = 0;

    // Level goals
    [Header("Level Progression Goals")]
    public int beginnerGoal  = 7;   // pickups needed in Beginner  → load Advanced
    public int advancedGoal  = 3;   // pickups needed in Advanced  → load Expert

    private bool _progressionLocked = false; // prevents double-load

    PlayerCheckpointDatat playerCheckpointDatat;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pickupsThisScene   = 0;
        _progressionLocked = false;
        freezeTime = 0f;
        _freezeGameOverTriggered = false;
        playerCheckpointDatat = Object.FindObjectOfType<PlayerCheckpointDatat>();

        // Sync lives from player data when entering a level
        if (playerCheckpointDatat != null)
            lives = playerCheckpointDatat.lives;

        UIManager.Instance?.RefreshHUD();
    }

    private void Start()
    {
        playerCheckpointDatat = Object.FindObjectOfType<PlayerCheckpointDatat>();
        UIManager.Instance?.RefreshHUD();
    }

    // ── Score / Collectible Add methods ──────────────────────────
    public void AddScore(int amount)
    {
        score += amount;
        UIManager.Instance?.RefreshHUD();
    }

    public void AddThermalStone(int amount)
    {
        thermalStones          += amount;
        thermalStonesCollected += amount;
        pickupsThisScene       += amount;
        // Warmth: collecting thermal stones slightly reduces freeze
        freezeTime = Mathf.Max(0f, freezeTime - amount * 3f);
        UIManager.Instance?.RefreshHUD();
        CheckLevelProgression();
    }

    public void AddFoodSupply(int amount)
    {
        foodSupplies           += amount;
        foodSuppliesCollected  += amount;
        pickupsThisScene       += amount;
        freezeTime = Mathf.Max(0f, freezeTime - amount * 2f);
        UIManager.Instance?.RefreshHUD();
        CheckLevelProgression();
    }

    public void AddWinterClothing(int amount)
    {
        winterClothing             += amount;
        winterClothingCollected    += amount;
        pickupsThisScene           += amount;
        freezeTime = Mathf.Max(0f, freezeTime - amount * 5f);
        UIManager.Instance?.RefreshHUD();
        CheckLevelProgression();
    }

    public void GainLife(int amount = 1)
    {
        lives += amount;
        if (playerCheckpointDatat != null)
            playerCheckpointDatat.lives = lives;
        UIManager.Instance?.RefreshHUD();
    }

    // ── Level progression ────────────────────────────────────────
    private void CheckLevelProgression()
    {
        if (_progressionLocked) return;

        string scene = SceneManager.GetActiveScene().name;

        bool shouldProgress =
            (scene == "Beginner" && pickupsThisScene >= beginnerGoal) ||
            (scene == "Advanced" && pickupsThisScene >= advancedGoal);

        if (!shouldProgress) return;

        _progressionLocked = true;

        string nextScene = scene == "Beginner" ? "Advanced" : "Expert";
        Debug.Log($"[GameManager] {scene}: {pickupsThisScene} pickups reached — loading {nextScene}");
        SceneManager.LoadScene(nextScene);
    }

    // ── Survival + freeze timer ───────────────────────────────────
    void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (_freezeGameOverTriggered) return;

        // Skip freeze/survival on menu / splash scenes
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "SplashScreen" || scene == "StartScreen")
            return;

        if (playerCheckpointDatat == null)
            playerCheckpointDatat = Object.FindObjectOfType<PlayerCheckpointDatat>();

        // Already dead
        if (playerCheckpointDatat != null && playerCheckpointDatat.lives <= 0)
            return;

        // Freeze builds over time; winter gear / stones slow it down
        float rate = freezeRate;
        if (winterClothing > 0) rate *= 0.55f;
        if (thermalStones > 5)  rate *= 0.75f;
        if (thermalStones <= 0 && foodSupplies <= 0) rate *= 1.5f;

        freezeTime += Time.deltaTime * rate;
        UIManager.Instance?.RefreshFreezeUI();

        if (freezeTime >= freezeLimitSeconds)
        {
            TriggerFreezeGameOver();
            return;
        }

        // Legacy survival drain (thermal / food) — keeps Panel1 updating
        timerCount += Time.deltaTime;
        if (timerCount >= 5.0f)
        {
            int randomSubtract = Random.Range(1, 3);
            thermalStones = Mathf.Max(0, thermalStones - randomSubtract);
            foodSupplies  = Mathf.Max(0, foodSupplies  - randomSubtract);
            UIManager.Instance?.RefreshHUD();
            timerCount = 0f;
        }
    }

    void TriggerFreezeGameOver()
    {
        if (_freezeGameOverTriggered) return;
        _freezeGameOverTriggered = true;

        Debug.Log("[GameManager] Freeze timer reached 1 minute — Game Over.");

        int finalScore = score;
        if (playerCheckpointDatat != null)
        {
            playerCheckpointDatat.lives = 0;
            finalScore = Mathf.Max(finalScore, playerCheckpointDatat.score);
        }
        lives = 0;

        if (GameOverScreen.Instance != null)
            GameOverScreen.Instance.ShowGameOver(finalScore, 0);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── Total collected helper (used by scoreboard) ───────────────
    public int TotalCollected =>
        thermalStonesCollected + foodSuppliesCollected + winterClothingCollected;
}
