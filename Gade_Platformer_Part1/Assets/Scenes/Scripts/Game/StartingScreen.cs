using UnityEngine;
using System.Collections;

public class StartingScreen : MonoBehaviour
{
    public GameObject startingScreenUI;
    private float delayTime = 3f; // seconds before switching
    private float timer = 0f;
    
    void Update()
    {
        startingScreenUI.SetActive(true);
        //Increment time to 5 seconds 
        timer += Time.deltaTime;
        Debug.Log(timer);

        if (timer >= delayTime)
        {
            Scene2();

        }
    }
    public void Scene2()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Beginner");
    }
}
