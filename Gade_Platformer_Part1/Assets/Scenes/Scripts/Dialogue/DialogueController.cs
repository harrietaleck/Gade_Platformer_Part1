using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public Button nextButton;

    // Drag the portrait Image component here in the Inspector.
    // When a DialogueLine has a portrait Sprite set, it is displayed here.
    // When portrait is null the image is hidden so the panel looks clean.
    public Image portraitImage;

    [Header("Dialogue data (drag 3 assets here)")]
    public List<SceneDialogueData> allSceneDialogueData = new List<SceneDialogueData>();

    // Queue ADT � holds lines in order 
    private DialogueQueue<DialogueLine> dialogueQueue = new DialogueQueue<DialogueLine>();

    private void Start()
    {
        LoadDialogueForCurrentScene();

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextLine);

        ShowNextLine();
    }

    private void PauseGameplay()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGameplay()
    {
        Time.timeScale = 1f;
    }

    private void LoadDialogueForCurrentScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        dialogueQueue.Clear();

        SceneDialogueData sceneData = allSceneDialogueData.Find(d => d != null && d.sceneName == activeSceneName);

        if (sceneData == null || sceneData.lines == null || sceneData.lines.Count == 0)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            return;
        }

        foreach (DialogueLine line in sceneData.lines)
            dialogueQueue.Enqueue(line);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        PauseGameplay();
    }

    public void ShowNextLine()
    {
        if (dialogueQueue.IsEmpty())
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            ResumeGameplay();
            return;
        }

        DialogueLine nextLine = dialogueQueue.Dequeue();

        if (speakerNameText != null)
            speakerNameText.text = nextLine.speakerName;

        if (dialogueText != null)
            dialogueText.text = nextLine.message;

        // Show or hide the portrait image based on whether this line has one.
        if (portraitImage != null)
        {
            if (nextLine.portrait != null)
            {
                portraitImage.sprite = nextLine.portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }
    }
}