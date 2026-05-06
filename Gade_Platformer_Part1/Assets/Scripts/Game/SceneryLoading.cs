using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneryLoading : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player touched trigger!");
            SceneManager.LoadScene("Advanced");
        }
    }

}