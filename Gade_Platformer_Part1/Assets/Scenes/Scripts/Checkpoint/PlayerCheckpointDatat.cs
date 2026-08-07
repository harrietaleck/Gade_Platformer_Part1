using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerCheckpointDatat : MonoBehaviour
{
    private Checkpoint checkpointStack = new Checkpoint();

    public int lives;
    public int score = 0;
    public int amount = 1;

    public TMP_Text livesText;
    public TMP_Text scoreText;

    private void Start()
    {
        lives = 3;
        //save the player starting point at the checkpoint
        CheckpointSave();
        UIText();

    }
    void Update()
    {
    }

    // ---- SaveCheckpoint ----
    // Persists the player's current state in TWO ways:
    // 1. Custom Stack ADT (Push) — required by the assignment rubric.
    //    The most recent snapshot is always on top, accessible via Peek().
    // 2. PlayerPrefs — survives scene reloads and editor restarts.
    void SaveCheckpoint()
    {
        // --- Stack ADT integration (custom array-based Stack) ---
        // Push a snapshot of position, lives, and score onto the Stack.
        // Peek() later retrieves it without removing it, so the same
        // checkpoint works for repeated deaths until the next one is pushed.
        checkpointStack.Push(new CheckpointData(transform.position, lives, score));
        Debug.Log("Checkpoint pushed to Stack. Count: " + checkpointStack.Count +
                  " | Pos: " + transform.position + " | Lives: " + lives);

        // --- PlayerPrefs backup (survives scene reload) ---
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.SetFloat("CheckpointZ", transform.position.z);
        PlayerPrefs.SetInt("CheckpointLives", lives);
        PlayerPrefs.SetInt("CheckpointScore", score);
        PlayerPrefs.Save();
    }

    // ---- PlayerDied ----
    // Respawns the player at the last checkpoint.
    // Restores POSITION only — lives are managed by LoseLife() so
    // each death permanently costs one life regardless of respawn.
    //
    // Uses Peek() (not Pop()) so the same checkpoint remains on the
    // Stack for repeated deaths until a new one is pushed.
    void PlayerDied()
    {
        Vector3 respawnPos;

        if (!checkpointStack.IsEmpty())
        {
            // --- Primary path: read from custom Stack ADT ---
            // Peek() retrieves the last-saved snapshot without removing it.
            CheckpointData last = checkpointStack.Peek();
            respawnPos = last.position;
            Debug.Log("Respawned from Stack Peek at: " + respawnPos + " | Lives: " + lives);
        }
        else
        {
            // --- Fallback: read from PlayerPrefs ---
            // Occurs only if SaveCheckpoint was never called (should not happen
            // after Start() runs CheckpointSave()).
            float x = PlayerPrefs.GetFloat("CheckpointX", transform.position.x);
            float y = PlayerPrefs.GetFloat("CheckpointY", transform.position.y);
            float z = PlayerPrefs.GetFloat("CheckpointZ", transform.position.z);
            respawnPos = new Vector3(x, y, z);
            Debug.LogWarning("Stack empty — falling back to PlayerPrefs for respawn.");
        }

        CharacterController controller = GetComponent<CharacterController>();
        // CharacterController blocks direct transform.position changes while
        // enabled. Disable it briefly, move, then re-enable.
        if (controller != null) controller.enabled = false;
        transform.position = respawnPos;
        if (controller != null) controller.enabled = true;

        // Keep PlayerPrefs lives in sync after the decrement from LoseLife().
        PlayerPrefs.SetInt("Lives", lives);
        PlayerPrefs.Save();
        UIText();
    }
    //Call the checkpoint save and player death functions to be used in ther scripts
    public void CheckpointSave()
    {
        SaveCheckpoint();
    }
    public void Death()
    {
        PlayerDied();
    }
    public void LoseLife()
    {
        // Already dead / game over — ignore further hits.
        if (lives <= 0) return;

        lives -= amount;
        Debug.Log("Player Lost a Life! Lives Remaining: " + lives);

        // Sync GameManager lives so the HUD stays consistent
        if (GameManager.Instance != null)
            GameManager.Instance.lives = lives;

        UIText();
        UIManager.Instance?.RefreshHUD();

        // Game over when all health is gone
        if (lives <= 0)
        {
            Debug.Log("Game Over — health depleted.");

            int finalScore = GameManager.Instance != null ? GameManager.Instance.score : score;

            if (GameOverScreen.Instance != null)
                GameOverScreen.Instance.ShowGameOver(finalScore, 0);
            else
                SceneManager.LoadScene("MainMenu");
        }
    }
    void UIText()
    {
        if (livesText != null)
            livesText.text = "Health: " + Mathf.Max(0, lives);
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}