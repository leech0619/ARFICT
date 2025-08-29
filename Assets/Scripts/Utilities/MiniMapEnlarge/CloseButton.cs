using UnityEngine;

/// <summary>
/// Handles close button functionality for enlarged minimap
/// </summary>
public class CloseButton : MonoBehaviour
{
    public ResetMapButton resetMapButton; // Fallback reference for map reset
    public MiniMapController miniMapController; // Main controller for minimap operations

    public void OnCloseButtonClick()
    {
        // Restore minimap to normal size using primary method
        if (miniMapController != null)
        {
            miniMapController.RestoreMinimap();
        }
        else if (resetMapButton != null)
        {
            // Fallback to legacy method if primary controller unavailable
            resetMapButton.ToggleMapSize();
        }
    }

    public void HideButton()
    {
        gameObject.SetActive(false); // Hide close button
    }

    public void ShowButton()
    {
        gameObject.SetActive(true); // Show close button
    }
}
