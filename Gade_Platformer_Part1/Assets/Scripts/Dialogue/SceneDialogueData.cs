using System.Collections.Generic;
using UnityEngine;

// Create assets: Project window → Create → Dialogue → Scene Dialogue Data
[CreateAssetMenu(fileName = "SceneDialogueData", menuName = "Dialogue/Scene Dialogue Data")]
public class SceneDialogueData : ScriptableObject
{
    public string sceneName;
    public List<DialogueLine> lines = new List<DialogueLine>();
}