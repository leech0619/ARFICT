using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Legacy minimap reset button - now delegates to MiniMapController
/// </summary>
public class ResetMapButton : MonoBehaviour
{
    public RectTransform minimapRectTransform; // Minimap UI component
    public Camera topDownCamera; // Minimap camera
    public float originalCameraHeight = 5.0f; // Default camera height
    public CloseButton closeButton; // Close button reference
    public MiniMapController miniMapController; // Main controller (preferred method)

    // Original state storage for fallback
    private Vector2 originalSize;
    private Vector2 originalAnchoredPosition;

    void Start()
    {
        // Store original minimap layout for restoration
        originalSize = minimapRectTransform.sizeDelta;
        originalAnchoredPosition = minimapRectTransform.anchoredPosition;
    }

    // Delegates to MiniMapController for consistent behavior
    public void ToggleMapSize()
    {
        if (miniMapController.isFullscreen)
        {
            // Use main controller's restore method
            miniMapController.RestoreMinimap();
        }
        else
        {
            // Enlargement handled by MiniMapController click detection
            Debug.Log("Use MiniMapController for enlargement");
        }
    }
}
