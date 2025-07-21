//using UnityEngine;
//using UnityEngine.UI;

//public class ResetMapButton : MonoBehaviour
//{
//    public RectTransform minimapRectTransform; // Reference to the minimap's RectTransform
//    public Camera topDownCamera; // Reference to the top-down camera
//    public float originalCameraHeight = 11.45f; // Original height of the top-down camera

//    private Vector2 originalSize; // Store the original size of the minimap
//    private Vector2 originalAnchoredPosition; // Store the original anchored position of the minimap
//    private bool isEnlarged = false; // Flag to track if the map is enlarged

//    void Start()
//    {
//        // Store the original size of the minimap
//        originalSize = new Vector2(400, 400);
//        // Store the original anchored position of the minimap
//        originalAnchoredPosition = new Vector2(220, 539);
//    }

//    // Method to toggle between enlarging and resetting the map
//    public void ToggleMapSize()
//    {
//        if (isEnlarged)
//        {
//            ResetMapSizeAndPosition();
//            ResetCameraHeight();
//            isEnlarged = false;
//        }
//        else
//        {
//            EnlargeMinimap();
//            IncreaseCameraHeight();
//            isEnlarged = true;
//        }
//    }

//    // Method to reset the map to its original size and position
//    private void ResetMapSizeAndPosition()
//    {
//        minimapRectTransform.sizeDelta = originalSize;
//        // Reset anchored position
//        minimapRectTransform.anchoredPosition = originalAnchoredPosition;
//    }

//    // Method to enlarge the map
//    private void EnlargeMinimap()
//    {
//        // Enlarge to match screen size while maintaining aspect ratio
//        float aspectRatio = originalSize.x / originalSize.y;
//        RectTransform canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
//        float newSizeY = canvasRectTransform.sizeDelta.y;
//        float newSizeX = newSizeY * aspectRatio;
//        minimapRectTransform.sizeDelta = new Vector2(newSizeX, newSizeY);
//        // Set anchoring and position to center on the screen
//        minimapRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
//        minimapRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
//        minimapRectTransform.pivot = new Vector2(0.5f, 0.5f);
//        minimapRectTransform.anchoredPosition = Vector2.zero;
//    }

//    // Method to reset the top-down camera height
//    private void ResetCameraHeight()
//    {
//        topDownCamera.transform.position = new Vector3(0, originalCameraHeight, 0);
//    }

//    // Method to increase the top-down camera height
//    private void IncreaseCameraHeight()
//    {
//        topDownCamera.transform.position += Vector3.up * (originalCameraHeight - topDownCamera.transform.position.y);
//    }
//}




using UnityEngine;
using UnityEngine.UI;

public class ResetMapButton : MonoBehaviour
{
    public RectTransform minimapRectTransform; // Reference to the minimap's RectTransform
    public Camera topDownCamera; // Reference to the top-down camera
    public float originalCameraHeight = 11.45f; // Original height of the top-down camera
    public CloseButton closeButton; // Reference to the close button

    private Vector2 originalSize; // Store the original size of the minimap
    private Vector2 originalAnchoredPosition; // Store the original anchored position of the minimap
    private bool isEnlarged = false; // Flag to track if the map is enlarged

    void Start()
    {
        // Store the original size of the minimap
        originalSize = minimapRectTransform.sizeDelta;
        // Store the original anchored position of the minimap
        originalAnchoredPosition = minimapRectTransform.anchoredPosition;
    }

    // Method to toggle between enlarging and resetting the map
    public void ToggleMapSize()
    {
        if (isEnlarged)
        {
            ResetMapSizeAndPosition();
            ResetCameraHeight();
            isEnlarged = false;
        }
        else
        {
            EnlargeMinimap();
            IncreaseCameraHeight();
            isEnlarged = true;   
        }
    }

    // Method to reset the map to its original size and position
    private void ResetMapSizeAndPosition()
    {
        minimapRectTransform.sizeDelta = originalSize;
        // Reset anchored position
        minimapRectTransform.anchoredPosition = originalAnchoredPosition;
    }

    // Method to enlarge the map
    private void EnlargeMinimap()
    {
        // Enlarge to match screen size while maintaining aspect ratio
        float aspectRatio = originalSize.x / originalSize.y;
        RectTransform canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float newSizeY = canvasRectTransform.sizeDelta.y;
        float newSizeX = newSizeY * aspectRatio;
        minimapRectTransform.sizeDelta = new Vector2(newSizeX, newSizeY);
        // Set anchoring and position to center on the screen
        minimapRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        minimapRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        minimapRectTransform.pivot = new Vector2(0.5f, 0.5f);
        minimapRectTransform.anchoredPosition = Vector2.zero;
    }

    // Method to reset the top-down camera height
    private void ResetCameraHeight()
    {
        topDownCamera.transform.position = new Vector3(0, originalCameraHeight, 0);
    }

    // Method to increase the top-down camera height
    private void IncreaseCameraHeight()
    {
        topDownCamera.transform.position += Vector3.up * (originalCameraHeight - topDownCamera.transform.position.y);
    }
}
