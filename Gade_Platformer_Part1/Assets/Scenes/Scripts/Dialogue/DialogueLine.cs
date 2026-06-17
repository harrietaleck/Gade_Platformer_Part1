using UnityEngine;

// One line of dialogue.
// Stored in SceneDialogueData assets and loaded into the
// DialogueQueue<DialogueLine> (Queue ADT) at scene start.
[System.Serializable]
public class DialogueLine
{
    // Name shown in the speaker label on the dialogue panel.
    public string speakerName;

    // Body text shown in the dialogue text box.
    public string message;

    // Optional portrait sprite displayed next to the text.
    // Leave null to hide the portrait image for that line.
    public Sprite portrait;
}