using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public ResetMapButton resetMapButton;
    public MiniMapController miniMapController; // Add direct reference to MiniMapController

    public void OnCloseButtonClick()
    {
        // Use MiniMapController's restore method for consistent behavior
        if (miniMapController != null)
        {
            miniMapController.RestoreMinimap();
        }
        else if (resetMapButton != null)
        {
            // Fallback to old method
            resetMapButton.ToggleMapSize();
        }
    }

    public void HideButton()
    {
        gameObject.SetActive(false);
    }

    public void ShowButton()
    {
        gameObject.SetActive(true);
    }
}
