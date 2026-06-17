using UnityEngine;

// Attach to a GuidePanel GameObject inside the Canvas.
// Call Open() from the Pause Menu or Main Menu "Guide" button.
public class GuideScreen : MonoBehaviour
{
    [Header("Panel")]
    public GameObject guidePanel;

    private void Start()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
    }

    public void Open()
    {
        if (guidePanel != null)
            guidePanel.SetActive(true);
    }

    public void Close()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
    }
}
