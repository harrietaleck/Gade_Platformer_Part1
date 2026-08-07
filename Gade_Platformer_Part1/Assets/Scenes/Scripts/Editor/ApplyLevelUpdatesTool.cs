#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Applies Beginner-level gameplay updates to Advanced + Expert.
public static class ApplyLevelUpdatesTool
{
    const string AdvancedPath = "Assets/Scenes/Scenes/Advanced.unity";
    const string ExpertPath   = "Assets/Scenes/Scenes/Expert.unity";

    [MenuItem("Tools/Apply Beginner Updates To All Levels")]
    public static void ApplyToAllLevels()
    {
        string current = SceneManager.GetActiveScene().path;
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Apply Beginner Updates To All Levels ===\n");

        report.AppendLine(ProcessScene(AdvancedPath));
        report.AppendLine();
        report.AppendLine(ProcessScene(ExpertPath));

        if (!string.IsNullOrEmpty(current))
            EditorSceneManager.OpenScene(current);

        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("Level Updates", report.ToString(), "OK");
    }

    static string ProcessScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Scene: " + scene.name);

        int attackFixed = 0;
        var attacks = Object.FindObjectsOfType<AiAttack>();
        foreach (var attack in attacks)
        {
            var so = new SerializedObject(attack);
            SetFloat(so, "chaseRange", 4f);
            SetFloat(so, "hitRadius", 1.4f);
            SetFloat(so, "attackCooldown", 1.5f);
            SetFloat(so, "chaseSpeed", 6.5f);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(attack);
            attackFixed++;
        }
        sb.AppendLine("  AiAttack tuned: " + attackFixed);

        int deathAdded = 0;
        var transforms = Object.FindObjectsOfType<Transform>();
        foreach (var t in transforms)
        {
            string n = t.name.ToLowerInvariant();
            bool looksLikeKillZone =
                n.Contains("water") || n.Contains("death") ||
                n.Contains("kill") || n.Contains("fallzone");

            if (!looksLikeKillZone) continue;
            if (t.GetComponent<DeathTrigger>() != null) continue;

            var col = t.GetComponent<Collider>();
            if (col == null) continue;

            t.gameObject.AddComponent<DeathTrigger>();
            col.isTrigger = true;
            EditorUtility.SetDirty(t.gameObject);
            deathAdded++;
            sb.AppendLine("  + DeathTrigger on '" + t.name + "'");
        }
        sb.AppendLine("  DeathTriggers added: " + deathAdded);

        bool hasGameOver = Object.FindObjectOfType<GameOverScreen>() != null;
        bool hasUIManager = Object.FindObjectOfType<UIManager>() != null;
        bool hasPlayerData = Object.FindObjectOfType<PlayerCheckpointDatat>() != null;
        int patrolCount = Object.FindObjectsOfType<Patrol>().Length;

        sb.AppendLine("  GameOverScreen: " + (hasGameOver ? "OK" : "MISSING"));
        sb.AppendLine("  UIManager: " + (hasUIManager ? "OK" : "MISSING"));
        sb.AppendLine("  PlayerCheckpointDatat: " + (hasPlayerData ? "OK" : "MISSING"));
        sb.AppendLine("  Patrol wolves: " + patrolCount);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        sb.AppendLine("  Saved.");
        return sb.ToString();
    }

    static void SetFloat(SerializedObject so, string prop, float value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.floatValue = value;
    }
}
#endif
