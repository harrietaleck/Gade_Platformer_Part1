using UnityEditor;
using UnityEditor.SceneManagement;

// Automatically loads the first Build Settings scene (SplashScreen)
// whenever the user presses Play in the editor.
[InitializeOnLoad]
public static class PlayFromFirstScene
{
    const string MenuPath = "Tools/GameSetup/Play From First Scene (Toggle)";
    const string PrefKey  = "PlayFromFirstScene_Enabled";

    static PlayFromFirstScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        Enabled = !Enabled;
        UnityEngine.Debug.Log("Play From First Scene: " + (Enabled ? "ON" : "OFF"));
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    static string previousScene;

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!Enabled) return;

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Store current scene so we can return to it after play
            previousScene = EditorSceneManager.GetActiveScene().path;

            // If the first build scene exists and we're not already in it
            if (EditorBuildSettings.scenes.Length > 0)
            {
                string firstScene = EditorBuildSettings.scenes[0].path;
                if (EditorSceneManager.GetActiveScene().path != firstScene)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(firstScene);
                    else
                        EditorApplication.isPlaying = false;
                }
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Restore previous scene after stopping play
            if (!string.IsNullOrEmpty(previousScene))
                EditorSceneManager.OpenScene(previousScene);
        }
    }
}
