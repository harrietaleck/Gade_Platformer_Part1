// ============================================================
// PlayerHurtEffect.cs  —  Screen flash when the player is hurt
//
// Attach this component to the Player GameObject.
// AiAttack triggers Player.TriggerHurt(), which calls
// TriggerFlash() here. A translucent red overlay briefly
// covers the screen and fades out, giving instant visual
// feedback that the player took damage — without needing
// a dedicated hurt animation clip in the Animator.
//
// The overlay Canvas is created at runtime in Start() so no
// manual scene setup is required.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHurtEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("Colour of the screen flash. Alpha controls maximum opacity.")]
    public Color flashColor = new Color(1f, 0.05f, 0.05f, 0.40f);

    [Tooltip("How long the flash takes to fade out after the peak.")]
    public float fadeDuration = 0.50f;

    // ---- Runtime objects ----
    private Image   overlay;
    private Coroutine flashRoutine;

    // ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Create a dedicated Canvas that always renders on top of everything.
        // DontDestroyOnLoad ensures it persists across scene loads.
        var canvasGO = new GameObject("HurtFlashCanvas",
                                      typeof(Canvas),
                                      typeof(CanvasScaler),
                                      typeof(GraphicRaycaster));
        DontDestroyOnLoad(canvasGO);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;          // always on top

        // Full-screen Image child.
        var imgGO = new GameObject("HurtOverlay",
                                    typeof(RectTransform),
                                    typeof(Image));
        imgGO.transform.SetParent(canvasGO.transform, false);

        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = rt.offsetMax = Vector2.zero;

        overlay = imgGO.GetComponent<Image>();
        overlay.color          = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        overlay.raycastTarget  = false;     // never block UI clicks
    }

    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Immediately shows the red flash and fades it out over fadeDuration.
    /// Safe to call while a flash is already in progress — restarts it.
    /// </summary>
    public void TriggerFlash()
    {
        if (overlay == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Snap to peak colour immediately.
        overlay.color = flashColor;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t     = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(flashColor.a, 0f, t);
            overlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // Ensure fully transparent when done.
        overlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        flashRoutine  = null;
    }
}
