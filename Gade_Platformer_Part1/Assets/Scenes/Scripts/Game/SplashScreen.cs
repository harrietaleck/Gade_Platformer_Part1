using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [Header("References")]
    public Image splashImage;

    [Header("Timing")]
    public float fadeInDuration  = 1.2f;
    public float holdDuration    = 2.0f;
    public float fadeOutDuration = 0.8f;

    [Header("Scene")]
    public string nextSceneName = "MainMenu";

    void Start()
    {
        Time.timeScale = 1f;
        SetAlpha(splashImage, 0f);
        StartCoroutine(SplashRoutine());
    }

    IEnumerator SplashRoutine()
    {
        yield return StartCoroutine(FadeImageRealtime(splashImage, 0f, 1f, fadeInDuration, Ease.OutCubic));
        yield return new WaitForSecondsRealtime(holdDuration);
        yield return StartCoroutine(FadeImageRealtime(splashImage, 1f, 0f, fadeOutDuration, Ease.InCubic));
        SceneManager.LoadScene(nextSceneName);
    }

    enum Ease { Linear, OutCubic, InCubic }

    IEnumerator FadeImageRealtime(Image img, float from, float to, float duration, Ease ease = Ease.Linear)
    {
        if (img == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = ApplyEase(Mathf.Clamp01(t / duration), ease);
            SetAlpha(img, Mathf.Lerp(from, to, p));
            yield return null;
        }
        SetAlpha(img, to);
    }

    static float ApplyEase(float p, Ease ease)
    {
        switch (ease)
        {
            case Ease.OutCubic: return 1f - Mathf.Pow(1f - p, 3f);
            case Ease.InCubic:  return p * p * p;
            default:            return p;
        }
    }

    static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color; c.a = a; g.color = c;
    }
}
