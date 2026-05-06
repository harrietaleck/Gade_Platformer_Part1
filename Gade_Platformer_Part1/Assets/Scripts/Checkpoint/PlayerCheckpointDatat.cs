using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
public class PlayerCheckpointDatat : MonoBehaviour
{
    private Checkpoint checkpointStack = new Checkpoint();

    public int lives = 3;
    public int score = 0;
    public int amount = 1;

    public TMP_Text livesText;
    public TMP_Text scoreText;

    private void Start()
    {
        //save the player starting point at the checkpoint
        CheckpointSave();
    }
    void Update()
    {
        UIText();
    }

    //Save the check point of the player (position, lives, score) using PlayerPrefs saving system
    void SaveCheckpoint()
    {
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.SetFloat("CheckpointZ", transform.position.z);
        PlayerPrefs.SetInt("CheckpointLives", lives);
        PlayerPrefs.SetInt("CheckpointScore", score);
        PlayerPrefs.Save();
        Debug.Log("Checkpoint Saved! Position:" + transform.position + " Lives " + lives + " scores: " + score);
    }
    //Load the checkpoint data and respawn the player at the latest checkpoint (position, lives and score}
    void PlayerDied()
    {
        //Load the checkpoint position from the playerprefs save system
        float x = PlayerPrefs.GetFloat("CheckpointX", transform.position.x);
        float y = PlayerPrefs.GetFloat("CheckpointY", transform.position.y);
        float z = PlayerPrefs.GetFloat("CheckpointZ", transform.position.z);

        //Set the respawn position based on the latest checkpoint
        Vector3 respawnPos = new Vector3(x, y, z);

        CharacterController controller = GetComponent<CharacterController>();
        //Disable thr controler respawning the player
        if (controller != null)
        {
            controller.enabled = false; // VERY IMPORTANT
        }

        transform.position = respawnPos;

        //Re-enable the controller aftwer respawn
        if (controller != null)
        {
            controller.enabled = true;
        }
        //Set the lives and score of the player to the checkpoint data
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("Lives", lives);
        PlayerPrefs.Save();

        Debug.Log("Respawned at: " + respawnPos);
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
        lives -= amount;
        if (lives == 0)
            Debug.Log("Player dies");
    }
    void UIText()
    {
        //Update the HUD Texts
        if (livesText == null) return;
        if (scoreText == null) return;

        livesText.text = "Lives: " + lives;
        scoreText.text = "Score: " + score;

    }
}