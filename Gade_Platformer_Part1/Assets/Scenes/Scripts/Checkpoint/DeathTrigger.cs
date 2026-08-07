using UnityEngine;
using UnityEngine.SceneManagement;

// Fall zone / kill plane placed below the platforms.
// Falling off a platform into this trigger is an INSTANT game over:
// the Game Over screen is shown and the player can start the level over
// via the Retry button (or the level reloads directly as a fallback).
public class DeathTrigger : MonoBehaviour
{
    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        PlayerCheckpointDatat player = other.GetComponent<PlayerCheckpointDatat>();
        if (player == null) return;

        _triggered = true;

        // --- SFX: fall/hit sound ---
        SFXManager.Instance?.PlaySound("hit");

        // Falling off ends the run immediately.
        int finalScore = GameManager.Instance != null ? GameManager.Instance.score : player.score;

        // Keep displayed lives consistent with a game over.
        player.lives = 0;
        if (GameManager.Instance != null)
            GameManager.Instance.lives = 0;

        if (GameOverScreen.Instance != null)
        {
            GameOverScreen.Instance.ShowGameOver(finalScore, 0);
        }
        else
        {
            // No Game Over UI in this scene — restart the level so the player starts over.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
