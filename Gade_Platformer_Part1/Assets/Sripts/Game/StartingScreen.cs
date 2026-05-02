using UnityEngine;
using System.Collections;

public class StartingScreen : MonoBehaviour
{
    public GameObject startingScreenUI;
    public GameObject menuScreenUI;

    void Start()
    {
        ShowStartingScreen();
    }
    void ShowStartingScreen()
    {
        startingScreenUI.SetActive(true);
        menuScreenUI.SetActive(false);
    }
    public void Play()
    {
        startingScreenUI.SetActive(false);
        menuScreenUI.SetActive(false);
    }
    public void ExitGamePlay()
    {
        Application.Quit();
    }
    public void Menu()
    {
        startingScreenUI.SetActive(false);
        menuScreenUI.SetActive(true);
    }
    public void Resume()
    {
        startingScreenUI.SetActive(true);
        menuScreenUI.SetActive(false);
    }
    public void Scene2()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Advanced");
    }

}
