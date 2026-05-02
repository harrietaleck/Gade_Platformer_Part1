using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneryLoading1 : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            SceneManager.LoadScene("Expert");
        }
    }

}