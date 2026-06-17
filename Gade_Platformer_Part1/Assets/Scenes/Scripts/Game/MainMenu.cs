using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string beginnerSceneName = "Beginner";
    public string advancedSceneName = "Advanced";
    public string expertSceneName = "Expert";

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject confirmQuitPanel;

    [Header("Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        Time.timeScale = 1f;

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null)  creditsPanel.SetActive(false);
        if (confirmQuitPanel != null) confirmQuitPanel.SetActive(false);

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    // ── Level Buttons ──────────────────────────────────────────────

    public void LoadBeginner()
    {
        SceneManager.LoadScene(beginnerSceneName);
    }

    public void LoadAdvanced()
    {
        SceneManager.LoadScene(advancedSceneName);
    }

    public void LoadExpert()
    {
        SceneManager.LoadScene(expertSceneName);
    }

    // ── Settings Panel ─────────────────────────────────────────────

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // ── Credits Panel ──────────────────────────────────────────────

    public void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    // ── Quit ───────────────────────────────────────────────────────

    public void OpenConfirmQuit()
    {
        if (confirmQuitPanel != null) confirmQuitPanel.SetActive(true);
    }

    public void CancelQuit()
    {
        if (confirmQuitPanel != null) confirmQuitPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
