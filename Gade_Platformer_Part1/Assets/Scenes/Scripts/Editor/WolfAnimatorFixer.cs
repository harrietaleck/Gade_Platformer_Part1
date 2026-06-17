// ============================================================
// WolfAnimatorFixer.cs  —  Editor utility: Tools ▶ Fix Wolf Animators
//
// Finds every enemy with an EnemyWolfAnimator component in the
// currently open scene, locates the "WolfVisual" child Animator,
// and assigns Wolf_animation.controller if it is missing.
// Marks the scene dirty so a normal Ctrl+S saves the fix permanently.
//
// Run ONCE per scene (Beginner, Advanced, Expert) after opening it,
// then save (Ctrl+S). After that the controller is baked in and this
// script is no longer needed at runtime.
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WolfAnimatorFixer
{
    const string ControllerPath = "Assets/Wolf_Animated/Model/Wolf_animation.controller";

    [MenuItem("Tools/Fix Wolf Animators (Current Scene)")]
    public static void FixWolfAnimators()
    {
        // Load the controller asset.
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        if (ctrl == null)
        {
            EditorUtility.DisplayDialog("Wolf Animator Fixer",
                $"Could not find controller at:\n{ControllerPath}\n\nCheck that the Wolf_Animated package is imported.",
                "OK");
            return;
        }

        int fixed_count = 0;
        int already_count = 0;

        // Find every EnemyWolfAnimator in the open scene(s).
        foreach (var wolfAnim in Object.FindObjectsOfType<EnemyWolfAnimator>(includeInactive: true))
        {
            var wolfVisual = wolfAnim.transform.Find("WolfVisual");
            if (wolfVisual == null) continue;

            var anim = wolfVisual.GetComponentInChildren<Animator>(true);
            if (anim == null) continue;

            if (anim.runtimeAnimatorController == null)
            {
                Undo.RecordObject(anim, "Assign Wolf Animation Controller");
                anim.runtimeAnimatorController = ctrl;
                EditorUtility.SetDirty(anim);
                fixed_count++;
                Debug.Log($"[WolfAnimatorFixer] Assigned controller to '{wolfAnim.name}/WolfVisual'.");
            }
            else
            {
                already_count++;
            }
        }

        // Mark scene dirty so Save picks up the changes.
        if (fixed_count > 0)
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Wolf Animator Fixer",
            $"Done.\n• Fixed: {fixed_count} wolf(s)\n• Already correct: {already_count} wolf(s)\n\nPress Ctrl+S to save the scene.",
            "OK");
    }
}
#endif
