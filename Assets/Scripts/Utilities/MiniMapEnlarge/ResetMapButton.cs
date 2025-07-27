using UnityEngine;
using UnityEngine.UI;

public class ResetMapButton : MonoBehaviour
{
    public RectTransform minimapRectTransform;
    public Camera topDownCamera;
    public float originalCameraHeight = 5.0f;
    public CloseButton closeButton;
    public MiniMapController miniMapController;

    private Vector2 originalSize;
    private Vector2 originalAnchoredPosition;

    void Start()
    {
        originalSize = minimapRectTransform.sizeDelta;
        originalAnchoredPosition = minimapRectTransform.anchoredPosition;
    }

    // Updated method to work with MiniMapController
    public void ToggleMapSize()
    {
        if (miniMapController.isFullscreen)
        {
            // Use MiniMapController's restore method instead
            miniMapController.RestoreMinimap();
        }
        else
        {
            // This should not be called since clicking is handled by MiniMapController
            // But keeping for compatibility
            Debug.Log("Use MiniMapController for enlargement");
        }
    }

    // Keep these methods for backward compatibility if needed elsewhere
    private void ResetMapSizeAndPosition()
    {
        minimapRectTransform.sizeDelta = originalSize;
        minimapRectTransform.anchoredPosition = originalAnchoredPosition;
    }

    private void ResetCameraHeight()
    {
        topDownCamera.transform.position = new Vector3(0, originalCameraHeight, 0);
    }

    private void IncreaseCameraHeight()
    {
        topDownCamera.transform.position += Vector3.up * (originalCameraHeight - topDownCamera.transform.position.y);
    }
}
