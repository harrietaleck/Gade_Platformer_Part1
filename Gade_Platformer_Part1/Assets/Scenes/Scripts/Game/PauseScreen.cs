using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseScreen : MonoBehaviour
{
    public GameObject pauseScreenUI;
    void Update()
    {
        //Get the key to exit the game
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Pause the game
            Pause();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            //Restart the game
            RestartGame();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            //Resume the game
            Resume();
        }
    }
    public void Pause()
    {
        pauseScreenUI.SetActive(true);
        //Pause the game
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        pauseScreenUI.SetActive(false);
        //Resume the game
        Time.timeScale = 1f;
    }
    public void RestartGame()
    {
        //Restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //Resume the game
        Time.timeScale = 1f;
    }

}
