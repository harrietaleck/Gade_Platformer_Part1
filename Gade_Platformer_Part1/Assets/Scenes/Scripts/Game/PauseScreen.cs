using UnityEngine;
using System.Collections;

public class PauseScreen : MonoBehaviour
{
    public GameObject pauseScreenUI;
    public GameObject MenuScreenUI;
    public void ExitGamePlay()
    {
        Application.Quit();
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
    public void ReturnToMenu()
    {
        MenuScreenUI.SetActive(true);
        Time.timeScale = 0f;
    }

}
