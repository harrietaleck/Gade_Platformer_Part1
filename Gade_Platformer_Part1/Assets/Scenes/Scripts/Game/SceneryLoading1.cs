using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneryLoading1 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger!");
            SceneManager.LoadScene("Expert");
        }
    }

}