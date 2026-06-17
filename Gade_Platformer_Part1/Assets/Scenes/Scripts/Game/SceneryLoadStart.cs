using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneryLoadStart : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger!");
            SceneManager.LoadScene("StartScreen");
        }
    }
}