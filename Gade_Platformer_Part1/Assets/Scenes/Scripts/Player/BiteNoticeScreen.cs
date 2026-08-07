using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// BiteNoticeScreen — brief "You have been bitten" overlay
// Shown when a wolf bite costs a life and the player respawns
// (not shown on final death — Game Over handles that).
// ============================================================
public class BiteNoticeScreen : MonoBehaviour
{
    public static BiteNoticeScreen Instance { get; private set; }

    [Tooltip("How long the notice stays on screen (realtime seconds).")]
    public float displayDuration = 1.75f;

    [Tooltip("Fade-out time after the hold.")]
    public float fadeOutDuration = 0.35f;

    GameObject _overlay;
    CanvasGroup _group;
    Coroutine _routine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Show the bite notice. Safe to call repeatedly.</summary>
    public void Show()
    {
        BuildIfNeeded();
        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine());
    }

    /// <summary>Find or create a singleton on demand (no scene setup required).</summary>
    public static void ShowBiteNotice()
    {
        if (Instance == null)
        {
            var go = new GameObject("BiteNoticeScreen");
            DontDestroyOnLoad(go);
            go.AddComponent<BiteNoticeScreen>();
        }
        Instance.Show();
    }

    void BuildIfNeeded()
    {
        if (_overlay != null) return;

        _overlay = new GameObject("BiteNoticeOverlay", typeof(RectTransform));
        DontDestroyOnLoad(_overlay);

        var canvas = _overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1100;
        _overlay.AddComponent<GraphicRaycaster>();
        _group = _overlay.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        var root = _overlay.GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;

        // Dim backdrop
        var bgGo = new GameObject("Backdrop", typeof(RectTransform));
        bgGo.transform.SetParent(_overlay.transform, false);
        Stretch(bgGo.GetComponent<RectTransform>());
        var bg = bgGo.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.02f, 0.02f, 0.72f);
        bg.raycastTarget = false;

        // Center card
        var cardGo = new GameObject("Card", typeof(RectTransform));
        cardGo.transform.SetParent(_overlay.transform, false);
        var cardR = cardGo.GetComponent<RectTransform>();
        cardR.anchorMin = new Vector2(0.18f, 0.38f);
        cardR.anchorMax = new Vector2(0.82f, 0.62f);
        cardR.offsetMin = cardR.offsetMax = Vector2.zero;
        var cardImg = cardGo.AddComponent<Image>();
        cardImg.color = new Color(0.12f, 0.08f, 0.08f, 0.95f);
        cardImg.raycastTarget = false;

        var title = MakeText(cardGo.transform, "Title", "YOU HAVE BEEN BITTEN",
                             42, FontStyles.Bold, new Color(1f, 0.35f, 0.32f));
        var titleR = title.rectTransform;
        titleR.anchorMin = new Vector2(0.05f, 0.45f);
        titleR.anchorMax = new Vector2(0.95f, 0.92f);
        titleR.offsetMin = titleR.offsetMax = Vector2.zero;

        var sub = MakeText(cardGo.transform, "Subtitle", "Respawning at checkpoint…",
                           22, FontStyles.Normal, new Color(0.92f, 0.90f, 0.88f));
        var subR = sub.rectTransform;
        subR.anchorMin = new Vector2(0.05f, 0.08f);
        subR.anchorMax = new Vector2(0.95f, 0.42f);
        subR.offsetMin = subR.offsetMax = Vector2.zero;
    }

    IEnumerator ShowRoutine()
    {
        _overlay.SetActive(true);
        _group.alpha = 1f;
        _group.blocksRaycasts = true;

        // Brief pause so the player reads the notice before continuing.
        float prevScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(displayDuration);

        float fade = 0f;
        while (fade < fadeOutDuration)
        {
            fade += Time.unscaledDeltaTime;
            _group.alpha = 1f - Mathf.Clamp01(fade / fadeOutDuration);
            yield return null;
        }

        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _overlay.SetActive(false);

        // Restore gameplay unless something else (Game Over) already paused.
        if (Time.timeScale == 0f)
            Time.timeScale = prevScale > 0f ? prevScale : 1f;

        _routine = null;
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    static TMP_Text MakeText(Transform parent, string name, string text,
                             float size, FontStyles style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }
}
