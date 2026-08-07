using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// ============================================================
// DialogueController — smooth level intro dialogue
//
// Keeps a brief pause so the player can read, but feels fast:
//  • Typewriter reveal (Space/Click skips to full line)
//  • Auto-advance after a short hold
//  • Space / Enter / Click / Next → next line
//  • Esc / Skip All → dismiss and start playing immediately
// ============================================================
public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public Button nextButton;
    public Button skipButton;
    public Image portraitImage;

    [Header("Dialogue data (drag scene assets here)")]
    public List<SceneDialogueData> allSceneDialogueData = new List<SceneDialogueData>();

    [Header("Feel")]
    [Tooltip("Characters revealed per second during typewriter.")]
    public float charsPerSecond = 48f;
    [Tooltip("Realtime seconds to keep a finished line on screen before auto-next.")]
    public float autoAdvanceDelay = 1.35f;
    [Tooltip("Minimum hold even for very short lines.")]
    public float minHoldSeconds = 0.55f;
    [Tooltip("Fade the panel in/out.")]
    public float fadeSeconds = 0.2f;

    DialogueQueue<DialogueLine> dialogueQueue = new DialogueQueue<DialogueLine>();
    CanvasGroup _canvasGroup;
    Coroutine _flowRoutine;
    bool _isActive;
    bool _lineFullyShown;
    bool _advanceRequested;
    bool _skipAllRequested;
    string _fullLine = "";

    void Start()
    {
        EnsureCanvasGroup();
        EnsureDialogueAssets();
        WireButtons();
        LoadDialogueForCurrentScene();

        if (!_isActive)
        {
            HidePanelImmediate();
            return;
        }

        _flowRoutine = StartCoroutine(DialogueFlow());
    }

    // Makes sure Beginner / Advanced / Expert dialogue assets are available
    // even if the Inspector list was cleared or failed to deserialize.
    void EnsureDialogueAssets()
    {
        if (allSceneDialogueData == null)
            allSceneDialogueData = new List<SceneDialogueData>();

        allSceneDialogueData.RemoveAll(d => d == null);
        if (allSceneDialogueData.Count > 0) return;

#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SceneDialogueData");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SceneDialogueData>(path);
            if (asset != null && !allSceneDialogueData.Contains(asset))
                allSceneDialogueData.Add(asset);
        }
#endif

        // Standalone builds: place copies under Resources/Dialogue if needed.
        foreach (var asset in Resources.LoadAll<SceneDialogueData>("Dialogue"))
        {
            if (asset != null && !allSceneDialogueData.Contains(asset))
                allSceneDialogueData.Add(asset);
        }
    }

    void Update()
    {
        if (!_isActive) return;

        if (ReadSkipAllPressed())
        {
            _skipAllRequested = true;
            _advanceRequested = true;
            return;
        }

        if (ReadAdvancePressed())
            _advanceRequested = true;
    }

    void WireButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => _advanceRequested = true);
            var label = nextButton.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = "Next  /  Space";
        }

        EnsureSkipButton();
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() =>
            {
                _skipAllRequested = true;
                _advanceRequested = true;
            });
        }
    }

    void EnsureSkipButton()
    {
        if (skipButton != null || dialoguePanel == null) return;

        // Reuse existing skip if someone named it in the scene
        var existing = dialoguePanel.GetComponentsInChildren<Button>(true);
        foreach (var b in existing)
        {
            if (b.name.IndexOf("Skip", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                skipButton = b;
                return;
            }
        }

        // Create a small Skip All button near the Next button
        var go = new GameObject("Button_SkipAll", typeof(RectTransform));
        Transform parent = nextButton != null ? nextButton.transform.parent : dialoguePanel.transform;
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (nextButton != null)
        {
            var nrt = nextButton.GetComponent<RectTransform>();
            rt.anchorMin = nrt.anchorMin;
            rt.anchorMax = nrt.anchorMax;
            rt.pivot = nrt.pivot;
            rt.sizeDelta = nrt.sizeDelta;
            rt.anchoredPosition = nrt.anchoredPosition + new Vector2(-nrt.sizeDelta.x - 12f, 0f);
            if (rt.sizeDelta.sqrMagnitude < 1f)
            {
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-210f, 18f);
                rt.sizeDelta = new Vector2(130f, 40f);
            }
        }
        else
        {
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-24f, 18f);
            rt.sizeDelta = new Vector2(130f, 40f);
        }

        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.28f, 0.38f, 0.95f);
        skipButton = go.AddComponent<Button>();
        skipButton.targetGraphic = img;

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tr = textGo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "Skip All";
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    void EnsureCanvasGroup()
    {
        if (dialoguePanel == null) return;
        _canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
    }

    void LoadDialogueForCurrentScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        dialogueQueue.Clear();
        _isActive = false;

        SceneDialogueData sceneData = allSceneDialogueData.Find(
            d => d != null && d.sceneName == activeSceneName);

        if (sceneData == null || sceneData.lines == null || sceneData.lines.Count == 0)
            return;

        foreach (DialogueLine line in sceneData.lines)
        {
            if (line == null) continue;
            if (string.IsNullOrWhiteSpace(line.message)) continue;
            dialogueQueue.Enqueue(line);
        }

        _isActive = !dialogueQueue.IsEmpty();
    }

    IEnumerator DialogueFlow()
    {
        PauseGameplay();
        yield return FadePanel(true);

        while (!dialogueQueue.IsEmpty() && !_skipAllRequested)
        {
            DialogueLine line = dialogueQueue.Dequeue();
            ApplyLineMeta(line);
            yield return RevealLine(line.message ?? "");

            if (_skipAllRequested) break;

            // Hold on the finished line; advance early on input
            float hold = Mathf.Max(minHoldSeconds, autoAdvanceDelay);
            float elapsed = 0f;
            _advanceRequested = false;
            while (elapsed < hold && !_advanceRequested && !_skipAllRequested)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _advanceRequested = false;
        }

        // Drain queue on skip
        dialogueQueue.Clear();
        yield return FadePanel(false);
        HidePanelImmediate();
        ResumeGameplay();
        _isActive = false;
        _flowRoutine = null;
    }

    void ApplyLineMeta(DialogueLine line)
    {
        if (speakerNameText != null)
            speakerNameText.text = string.IsNullOrWhiteSpace(line.speakerName)
                ? "Guide" : line.speakerName.Trim();

        if (portraitImage != null)
        {
            if (line.portrait != null)
            {
                portraitImage.sprite = line.portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator RevealLine(string message)
    {
        _fullLine = message;
        _lineFullyShown = false;
        _advanceRequested = false;

        if (dialogueText == null)
        {
            _lineFullyShown = true;
            yield break;
        }

        if (string.IsNullOrEmpty(message) || charsPerSecond <= 0f)
        {
            dialogueText.text = message;
            _lineFullyShown = true;
            yield break;
        }

        dialogueText.text = "";
        float charDelay = 1f / charsPerSecond;
        for (int i = 1; i <= message.Length; i++)
        {
            if (_advanceRequested || _skipAllRequested)
            {
                // First press finishes the line instead of skipping past it
                dialogueText.text = message;
                _lineFullyShown = true;
                _advanceRequested = false;
                yield return null;
                yield break;
            }

            dialogueText.text = message.Substring(0, i);
            yield return new WaitForSecondsRealtime(charDelay);
        }

        _lineFullyShown = true;
    }

    IEnumerator FadePanel(bool show)
    {
        if (dialoguePanel == null) yield break;

        dialoguePanel.SetActive(true);
        if (_canvasGroup == null)
            yield break;

        float start = _canvasGroup.alpha;
        float end = show ? 1f : 0f;
        _canvasGroup.blocksRaycasts = show;
        _canvasGroup.interactable = show;

        if (fadeSeconds <= 0.01f)
        {
            _canvasGroup.alpha = end;
            yield break;
        }

        float t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, end, Mathf.Clamp01(t / fadeSeconds));
            yield return null;
        }
        _canvasGroup.alpha = end;
    }

    void HidePanelImmediate()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void PauseGameplay() => Time.timeScale = 0f;
    void ResumeGameplay() => Time.timeScale = 1f;

    // Public entry used by Next button / MCP tests
    public void ShowNextLine()
    {
        if (!_isActive)
        {
            // Fallback: finish any leftover if flow not running
            if (dialogueQueue.IsEmpty())
            {
                HidePanelImmediate();
                ResumeGameplay();
            }
            return;
        }
        _advanceRequested = true;
    }

    public void SkipAll()
    {
        _skipAllRequested = true;
        _advanceRequested = true;
    }

    static bool ReadAdvancePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null &&
            (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
             kb.numpadEnterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame))
            return true;
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER || !ENABLE_INPUT_SYSTEM
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(0))
            return true;
#endif
        return false;
    }

    static bool ReadSkipAllPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame))
            return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER || !ENABLE_INPUT_SYSTEM
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            return true;
#endif
        return false;
    }
}
