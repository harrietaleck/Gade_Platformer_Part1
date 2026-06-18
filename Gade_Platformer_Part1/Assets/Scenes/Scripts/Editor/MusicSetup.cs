// ============================================================
// MusicSetup.cs — Tools > Setup Game Music
//
// Adds a GameMusicManager to the CURRENT scene with the
// GAMEPLAYSOUND clip already assigned.
//
// Run this ONCE on your MainMenu scene (or whichever scene
// loads first). The manager uses DontDestroyOnLoad so it
// carries across Beginner, Advanced, and Expert automatically.
// ============================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MusicSetup
{
    const string MusicClipPath = "Assets/GameSoundPlay/GAMEPLAYSOUND.mp3";

    [MenuItem("Tools/Setup Game Music (Current Scene)")]
    public static void SetupMusic()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicClipPath);
        if (clip == null)
        {
            EditorUtility.DisplayDialog("Music Setup",
                $"Could not find music clip at:\n{MusicClipPath}\n\nCheck the path.", "OK");
            return;
        }

        // Remove any stale managers first
        foreach (var old in Object.FindObjectsOfType<GameMusicManager>(includeInactive: true))
            Object.DestroyImmediate(old.gameObject);

        // Create fresh manager
        var go = new GameObject("GameMusicManager");
        Undo.RegisterCreatedObjectUndo(go, "Setup Game Music");

        var mgr = go.AddComponent<GameMusicManager>();
        mgr.musicClip = clip;
        mgr.volume    = 0.4f;

        // AudioSource is added by [RequireComponent], configure it now
        var src = go.GetComponent<AudioSource>();
        src.clip         = clip;
        src.loop         = true;
        src.playOnAwake  = true;
        src.volume       = 0.4f;
        src.spatialBlend = 0f;

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Music Setup",
            $"GameMusicManager added to '{SceneManager.GetActiveScene().name}'.\n\n" +
            $"Clip: GAMEPLAYSOUND.mp3\n" +
            $"Volume: 0.4  |  Loop: ON  |  DontDestroyOnLoad: ON\n\n" +
            "Press Ctrl+S to save.\n\n" +
            "The music will play from this scene and continue\n" +
            "across all scenes automatically.",
            "OK");
    }
}
#endif
