using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public Camera topDownCamera;
    public Camera arCamera; // Reference to AR camera for position calculation
    public float dragSpeed = 2f;
    public float coverageIncrease = 2.0f; // Increase to 200% more coverage
    public CloseButton closeButton;
    
    [Header("UI References")]
    public GameObject border; // Border GameObject (active by default)
    public GameObject borderEnlarged; // BorderEnlarged GameObject (inactive by default)
    public RectTransform minimapRawImage; // MiniMapRawImage RectTransform

    public bool isFullscreen = false;
    private RectTransform minimapRectTransform;
    
    // Store ALL original settings for proper restoration
    private Vector2 originalSize;
    private Vector2 originalAnchoredPosition;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector3 originalCameraPosition;
    private float originalOrthographicSize;
    private Transform originalParent; // Store original parent
    
    // Store original MiniMapRawImage settings
    private Vector2 originalRawImageAnchorMin;
    private Vector2 originalRawImageAnchorMax;
    private Vector2 originalRawImageOffsetMin;
    private Vector2 originalRawImageOffsetMax;
    private Vector2 originalRawImageAnchoredPosition;
    private Vector2 originalRawImageSizeDelta; // Add missing sizeDelta storage

    void Start()
    {
        minimapRectTransform = GetComponent<RectTransform>();
        
        // Store ALL original UI settings
        originalSize = minimapRectTransform.sizeDelta;
        originalAnchoredPosition = minimapRectTransform.anchoredPosition;
        originalAnchorMin = minimapRectTransform.anchorMin;
        originalAnchorMax = minimapRectTransform.anchorMax;
        originalPivot = minimapRectTransform.pivot;
        originalParent = minimapRectTransform.parent; // Store original parent
        
        // Store original camera settings
        if (topDownCamera != null)
        {
            originalCameraPosition = topDownCamera.transform.position;
            originalOrthographicSize = topDownCamera.orthographicSize;
        }
        
        // Store original MiniMapRawImage settings
        if (minimapRawImage != null)
        {
            originalRawImageAnchorMin = minimapRawImage.anchorMin;
            originalRawImageAnchorMax = minimapRawImage.anchorMax;
            originalRawImageOffsetMin = minimapRawImage.offsetMin;
            originalRawImageOffsetMax = minimapRawImage.offsetMax;
            originalRawImageAnchoredPosition = minimapRawImage.anchoredPosition;
            originalRawImageSizeDelta = minimapRawImage.sizeDelta; // Store original size
        }
        
        // Verify assignments
        Debug.Log($"MiniMapController Start - Verifying assignments:");
        Debug.Log($"border: {(border != null ? border.name : "NULL")}");
        Debug.Log($"borderEnlarged: {(borderEnlarged != null ? borderEnlarged.name : "NULL")}");
        Debug.Log($"minimapRawImage: {(minimapRawImage != null ? minimapRawImage.name : "NULL")}");
        
        if (minimapRawImage != null)
        {
            Debug.Log($"minimapRawImage current parent: {minimapRawImage.parent.name}");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFullscreen)
        {
            Debug.Log("Minimap clicked - starting enlargement");
            EnlargeMinimap();
            AdjustCameraForFullscreen();
            
            if (closeButton != null)
            {
                closeButton.ShowButton();
            }
            
            Debug.Log("Minimap enlargement completed");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isFullscreen)
        {
            Vector2 delta = eventData.delta;
            delta /= minimapRectTransform.rect.size;

            // Only allow X-axis movement, keep Z-axis fixed at -8
            Vector3 rightDirection = topDownCamera.transform.right;
            
            // Calculate only horizontal movement (X-axis)
            Vector3 movementDelta = rightDirection * (-delta.x) * dragSpeed;
            
            // Apply movement but constrain Z-axis to -8
            Vector3 newPosition = topDownCamera.transform.position + movementDelta;
            newPosition.z = -8f; // Keep Z-axis fixed at -8
            
            topDownCamera.transform.position = newPosition;
        }
    }

    void EnlargeMinimap()
    {
        // Debug current state
        Debug.Log($"EnlargeMinimap called - minimapRawImage: {minimapRawImage}, borderEnlarged: {borderEnlarged}");
        if (minimapRawImage != null)
        {
            Debug.Log($"MiniMapRawImage current parent: {minimapRawImage.parent.name}");
        }
        
        // Move MiniMapRawImage to BorderEnlarged parent
        if (minimapRawImage != null && borderEnlarged != null)
        {
            Transform originalParentOfRawImage = minimapRawImage.parent;
            minimapRawImage.SetParent(borderEnlarged.transform, false);
            Debug.Log($"Moved MiniMapRawImage from {originalParentOfRawImage.name} to {borderEnlarged.name}");
            Debug.Log($"MiniMapRawImage new parent: {minimapRawImage.parent.name}");
            
            Debug.Log($"MiniMapRawImage moved to BorderEnlarged parent - will be resized later");
        }
        else
        {
            if (minimapRawImage == null) Debug.LogError("minimapRawImage is null! Please assign it in the inspector.");
            if (borderEnlarged == null) Debug.LogError("borderEnlarged is null! Please assign it in the inspector.");
        }
        
        // Activate BorderEnlarged and deactivate Border
        if (borderEnlarged != null)
        {
            borderEnlarged.SetActive(true);
            Debug.Log("BorderEnlarged activated");
        }
        
        if (border != null)
        {
            border.SetActive(false);
            Debug.Log("Border deactivated");
        }

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // For circular minimap, use the smaller dimension to ensure it fits properly
        float minScreenDimension = Mathf.Min(screenWidth, screenHeight);
        
        // Use 85% of the smaller screen dimension for optimal circular display
        float circlePercentage = 0.85f;
        float circleSize = minScreenDimension * circlePercentage;

        Debug.Log($"Enlarging minimap: Screen {screenWidth}x{screenHeight}, Circle size: {circleSize}");

        // Don't modify BorderEnlarged size, just activate it
        if (borderEnlarged != null)
        {
            RectTransform borderEnlargedRect = borderEnlarged.GetComponent<RectTransform>();
            Debug.Log($"BorderEnlarged kept at original size - sizeDelta: {borderEnlargedRect.sizeDelta}");
            
            // Center the BorderEnlarged on screen
            borderEnlargedRect.anchorMin = new Vector2(0.5f, 0.5f);
            borderEnlargedRect.anchorMax = new Vector2(0.5f, 0.5f);
            borderEnlargedRect.pivot = new Vector2(0.5f, 0.5f);
            borderEnlargedRect.anchoredPosition = new Vector2(0, 0);
        }

        // Resize MiniMapRawImage to the calculated square size
        if (minimapRawImage != null)
        {
            Debug.Log($"MiniMapRawImage before resizing - sizeDelta: {minimapRawImage.sizeDelta}");
            
            // Set anchors to center within BorderEnlarged
            minimapRawImage.anchorMin = new Vector2(0.5f, 0.5f);
            minimapRawImage.anchorMax = new Vector2(0.5f, 0.5f);
            minimapRawImage.pivot = new Vector2(0.5f, 0.5f);
            
            // Set the calculated size directly for circular display
            minimapRawImage.sizeDelta = new Vector2(circleSize, circleSize);
            
            // Center it within BorderEnlarged
            minimapRawImage.anchoredPosition = new Vector2(0, 0);
            
            Debug.Log($"MiniMapRawImage after resizing - sizeDelta: {minimapRawImage.sizeDelta}, rect.size: {minimapRawImage.rect.size}");
        }

        Debug.Log($"Minimap UI configured for enlargement");

        isFullscreen = true;
    }

    void AdjustCameraForFullscreen()
    {
        if (topDownCamera == null) return;

        // Update camera position to follow AR camera if available
        if (arCamera != null)
        {
            Vector3 arCameraPosition = arCamera.transform.position;
            Vector3 newPosition = new Vector3(arCameraPosition.x, arCameraPosition.y + 5f, -8f);
            topDownCamera.transform.position = newPosition;
            Debug.Log($"Camera positioned above AR camera: AR at {arCameraPosition}, TopDown at {newPosition}");
        }
        else
        {
            // Fallback to original behavior: keep X and Y from original position, set Z to -8
            Vector3 newPosition = topDownCamera.transform.position;
            newPosition.z = -8f;
            topDownCamera.transform.position = newPosition;
            Debug.Log($"Camera Z-axis set to -8, position: {topDownCamera.transform.position}");
        }

        // Significantly increase orthographic size to show much more area when enlarged
        float newOrthographicSize = originalOrthographicSize * (1 + coverageIncrease);
        topDownCamera.orthographicSize = newOrthographicSize;
        
        Debug.Log($"Camera adjusted: Original size: {originalOrthographicSize}, New size: {newOrthographicSize}");
    }

    // PUBLIC method for external scripts to restore minimap
    public void RestoreMinimap()
    {
        if (!isFullscreen) return;

        // Move MiniMapRawImage back to Border parent
        if (minimapRawImage != null && border != null)
        {
            Debug.Log($"RestoreMinimap - MiniMapRawImage current parent: {minimapRawImage.parent.name}");
            Transform currentParentOfRawImage = minimapRawImage.parent;
            minimapRawImage.SetParent(border.transform, false);
            Debug.Log($"Moved MiniMapRawImage from {currentParentOfRawImage.name} back to {border.name}");
            Debug.Log($"MiniMapRawImage restored parent: {minimapRawImage.parent.name}");
            
            // Restore original MiniMapRawImage settings
            minimapRawImage.anchorMin = originalRawImageAnchorMin;
            minimapRawImage.anchorMax = originalRawImageAnchorMax;
            minimapRawImage.offsetMin = originalRawImageOffsetMin;
            minimapRawImage.offsetMax = originalRawImageOffsetMax;
            minimapRawImage.anchoredPosition = originalRawImageAnchoredPosition;
            minimapRawImage.sizeDelta = originalRawImageSizeDelta; // Restore original size
            
            Debug.Log($"MiniMapRawImage settings restored - sizeDelta: {minimapRawImage.sizeDelta}");
        }
        else
        {
            if (minimapRawImage == null) Debug.LogError("minimapRawImage is null during restore!");
            if (border == null) Debug.LogError("border is null during restore!");
        }
        
        // Restore Border activation and deactivate BorderEnlarged
        if (border != null)
        {
            border.SetActive(true);
            Debug.Log("Border reactivated");
        }
        
        if (borderEnlarged != null)
        {
            borderEnlarged.SetActive(false);
            Debug.Log("BorderEnlarged deactivated");
        }

        // Note: Update minimap position to follow AR camera location
        if (arCamera != null)
        {
            // Update the stored original camera position to current AR camera location + offset
            Vector3 arCameraPosition = arCamera.transform.position;
            originalCameraPosition = new Vector3(arCameraPosition.x, arCameraPosition.y + 5f, arCameraPosition.z);
            Debug.Log($"Updated original camera position to follow AR camera: {originalCameraPosition}");
        }

        // Restore camera settings - position based on updated original position, orthographic size to original
        if (topDownCamera != null)
        {
            // Restore both position and orthographic size (originalCameraPosition is now updated to follow AR camera)
            topDownCamera.transform.position = originalCameraPosition;
            topDownCamera.orthographicSize = originalOrthographicSize;
            Debug.Log($"Camera restored to position: {originalCameraPosition}");
        }

        closeButton.HideButton();
        isFullscreen = false;
    }
}

