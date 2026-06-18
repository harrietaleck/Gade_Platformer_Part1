// ============================================================
// HUDLayoutFixer.cs  —  Tools ▶ Fix HUD Layout (Current Scene)
//
// Moves Panel2 (Lives + Score) inward from the right edge so
// the values are fully visible on-screen.
// Also nudges Panel1 (WinterClothing / FoodSupply / ThermalStones)
// inward from the left if needed.
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HUDLayoutFixer
{
    [MenuItem("Tools/Fix HUD Layout (Current Scene)")]
    public static void FixHUD()
    {
        int fixed_count = 0;

        // ── Panel2: Lives + Score (right side) ───────────────────────────
        var panel2 = GameObject.Find("Panel2");
        if (panel2 != null)
        {
            var r = panel2.GetComponent<RectTransform>();
            if (r != null)
            {
                // Anchor to top-right, move inward so text isn't clipped
                r.anchorMin        = new Vector2(0.62f, 0.88f);
                r.anchorMax        = new Vector2(0.62f, 0.88f);
                r.anchoredPosition = Vector2.zero;
                r.sizeDelta        = new Vector2(220f, 60f);
                EditorUtility.SetDirty(panel2);
                fixed_count++;
            }
        }

        // ── Panel1: Collectables display (left side) ─────────────────────
        var panel1 = GameObject.Find("Panel1");
        if (panel1 != null)
        {
            var r = panel1.GetComponent<RectTransform>();
            if (r != null)
            {
                r.anchorMin        = new Vector2(0.02f, 0.88f);
                r.anchorMax        = new Vector2(0.02f, 0.88f);
                r.anchoredPosition = Vector2.zero;
                r.sizeDelta        = new Vector2(240f, 80f);
                EditorUtility.SetDirty(panel1);
                fixed_count++;
            }
        }

        if (fixed_count == 0)
        {
            EditorUtility.DisplayDialog("HUD Layout Fixer",
                "Could not find Panel1 or Panel2 in the current scene.\n" +
                "Make sure the correct game scene is open.", "OK");
            return;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("HUD Layout Fixer",
            $"Adjusted {fixed_count} panel(s) in '{SceneManager.GetActiveScene().name}'.\n" +
            "Press Ctrl+S to save.",
            "OK");
    }
}
#endif
