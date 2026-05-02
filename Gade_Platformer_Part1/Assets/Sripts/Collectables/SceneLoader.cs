using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        //Check iif the oject colliding is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            //Load the next scene
            SceneManager.LoadScene("Advanced");
        }
    }
}
