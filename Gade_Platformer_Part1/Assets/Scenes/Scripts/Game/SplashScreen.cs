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

    [Header("Skip")]
    [Tooltip("If enabled, pressing any key/mouse button skips the splash and starts the game immediately.")]
    public bool allowSkip = true;
    [Tooltip("Fade-out time used when the player skips manually.")]
    public float skipFadeOutDuration = 0.25f;

    bool _finished;

    void Start()
    {
        Time.timeScale = 1f;
        SetAlpha(splashImage, 0f);
        StartCoroutine(SplashRoutine());
    }

    void Update()
    {
        if (!allowSkip || _finished) return;
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            Skip();
        }
    }

    IEnumerator SplashRoutine()
    {
        yield return StartCoroutine(FadeImageRealtime(splashImage, 0f, 1f, fadeInDuration, Ease.OutCubic));
        yield return new WaitForSecondsRealtime(holdDuration);
        yield return StartCoroutine(FadeImageRealtime(splashImage, 1f, 0f, fadeOutDuration, Ease.InCubic));
        LoadNext();
    }

    void Skip()
    {
        if (_finished) return;
        StopAllCoroutines();
        StartCoroutine(SkipRoutine());
    }

    IEnumerator SkipRoutine()
    {
        float startAlpha = splashImage != null ? splashImage.color.a : 1f;
        yield return StartCoroutine(FadeImageRealtime(splashImage, startAlpha, 0f, skipFadeOutDuration, Ease.InCubic));
        LoadNext();
    }

    void LoadNext()
    {
        if (_finished) return;
        _finished = true;
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
