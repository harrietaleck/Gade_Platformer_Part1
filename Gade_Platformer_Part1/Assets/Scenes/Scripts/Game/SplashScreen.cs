using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [Header("Splash Settings")]
    public Image splashImage;
    public float displayDuration = 3f;
    public float fadeDuration = 0.5f;
    public string nextSceneName = "StartScreen";

    private void Start()
    {
        if (splashImage != null)
        {
            Color c = splashImage.color;
            c.a = 1f;
            splashImage.color = c;
        }

        StartCoroutine(SplashRoutine());
    }

    private IEnumerator SplashRoutine()
    {
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color c = splashImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            splashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        splashImage.color = c;
    }
}
