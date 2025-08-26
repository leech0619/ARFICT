using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler
{
    public Camera topDownCamera;
    public Camera arCamera; // Reference to AR camera for position calculation
    public float dragSpeed = 2f;
    public float coverageIncrease = 2.0f; // Increase to 200% more coverage
    public CloseButton closeButton;
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 0.5f;
    public float minZoom = 1f;
    public float maxZoom = 5f;
    
    [Header("Movement Boundaries")]
    [Header("Map Size: X=101, Z=36 | Camera: Size=6, Y=5")]
    public float mapBoundaryX = 38.5f; // Max X movement: (101/2) - (6*1.77) = 50.5 - 10.6 ≈ 38.5
    public float mapBoundaryZ = 12f;   // Max Z movement: (36/2) - 6 = 18 - 6 = 12
    
    [Header("Map Center (World Coordinates)")]
    public Vector3 actualMapCenter = Vector3.zero; // Set this to your actual map center in world coordinates
    
    // Touch/pinch zoom variables
    private float lastTouchDistance = 0f;
    private bool isPinching = false;
    private Vector3 mapCenter; // Center position for boundary calculations
    private Vector3 logicalCameraPosition; // Logical position for movement calculations (Z can change)
    
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

    void Update()
    {
        // Handle mobile pinch-to-zoom
        if (isFullscreen && Input.touchCount == 2)
        {
            HandlePinchZoom();
        }
        else
        {
            isPinching = false;
        }
    }

    void HandlePinchZoom()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);
        
        // Get current distance between touches
        float currentTouchDistance = Vector2.Distance(touch1.position, touch2.position);
        
        if (isPinching)
        {
            // Calculate zoom based on distance change
            float deltaDistance = currentTouchDistance - lastTouchDistance;
            
            // Convert distance change to zoom
            float zoomDelta = deltaDistance * zoomSpeed * 0.01f; // Scale down for smoother zoom
            
            // Apply zoom
            ApplyZoom(-zoomDelta); // Negative because pinching in should zoom in
        }
        else
        {
            // Start pinching
            isPinching = true;
        }
        
        lastTouchDistance = currentTouchDistance;
    }

    void ApplyZoom(float zoomDelta)
    {
        if (topDownCamera == null) return;
        
        // Calculate new orthographic size
        float currentSize = topDownCamera.orthographicSize;
        float newSize = currentSize + zoomDelta;
        
        // Clamp the zoom within the specified range
        // Note: smaller orthographic size = more zoomed in
        float baseSize = originalOrthographicSize * (1 + coverageIncrease);
        float minOrthographicSize = baseSize / maxZoom; // Most zoomed in
        float maxOrthographicSize = baseSize / minZoom; // Most zoomed out
        
        newSize = Mathf.Clamp(newSize, minOrthographicSize, maxOrthographicSize);
        
        // Apply the new orthographic size
        topDownCamera.orthographicSize = newSize;
        
        // After zoom, adjust camera position to stay within map boundaries
        AdjustCameraPositionForZoom();
        
        Debug.Log($"Zoom Applied: OrthoSize={newSize}, Range=[{minOrthographicSize}, {maxOrthographicSize}]");
    }
    
    void AdjustCameraPositionForZoom()
    {
        if (topDownCamera == null) return;
        
        Vector3 currentPosition = topDownCamera.transform.position;
        
        // Calculate dynamic boundaries based on current zoom level
        float currentOrthoSize = topDownCamera.orthographicSize;
        float viewHalfWidth = currentOrthoSize * Camera.main.aspect;
        float viewHalfHeight = currentOrthoSize;
        
        // Calculate boundaries to prevent camera view from going outside map
        float dynamicBoundaryX = Mathf.Max(0, (101f / 2f) - viewHalfWidth);
        float dynamicBoundaryZ = Mathf.Max(0, (36f / 2f) - viewHalfHeight);
        
        // Clamp camera position to stay within boundaries
        Vector3 clampedPosition = currentPosition;
        clampedPosition.x = Mathf.Clamp(currentPosition.x, mapCenter.x - dynamicBoundaryX, mapCenter.x + dynamicBoundaryX);
        clampedPosition.z = Mathf.Clamp(currentPosition.z, mapCenter.z - dynamicBoundaryZ, mapCenter.z + dynamicBoundaryZ);
        
        // Update both actual camera position and logical position
        topDownCamera.transform.position = clampedPosition;
        logicalCameraPosition = clampedPosition;
        
        Debug.Log($"Camera position adjusted for zoom - X: {clampedPosition.x:F1}, Z: {clampedPosition.z:F1}");
        Debug.Log($"Zoom boundaries - X: ±{dynamicBoundaryX:F1}, Z: ±{dynamicBoundaryZ:F1}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isFullscreen)
        {
            Vector2 delta = eventData.delta;
            delta /= minimapRectTransform.rect.size;

            // Use world space directions for consistent movement (both X and Z inverted for natural feel)
            // For top-down camera: X = left/right, Z = forward/backward movement
            Vector3 movementDelta = new Vector3(-delta.x, 0, -delta.y) * dragSpeed;
            
            // Calculate what the new LOGICAL position would be (allows Z movement)
            Vector3 newLogicalPosition = logicalCameraPosition + movementDelta;
            newLogicalPosition.y = logicalCameraPosition.y; // Keep Y-axis (height) unchanged
            
            // Apply boundary constraints to prevent moving outside the map area
            // Calculate dynamic boundaries based on current zoom level
            float currentOrthoSize = topDownCamera.orthographicSize;
            float viewHalfWidth = currentOrthoSize * Camera.main.aspect; // Approximate aspect ratio
            float viewHalfHeight = currentOrthoSize;
            
            // Adjust boundaries based on current camera view size
            float dynamicBoundaryX = Mathf.Max(0, (101f / 2f) - viewHalfWidth);
            float dynamicBoundaryZ = Mathf.Max(0, (36f / 2f) - viewHalfHeight);
            
            // Clamp logical X and Z positions to prevent going outside map bounds
            newLogicalPosition.x = Mathf.Clamp(newLogicalPosition.x, mapCenter.x - dynamicBoundaryX, mapCenter.x + dynamicBoundaryX);
            newLogicalPosition.z = Mathf.Clamp(newLogicalPosition.z, mapCenter.z - dynamicBoundaryZ, mapCenter.z + dynamicBoundaryZ);
            
            // Update logical position
            logicalCameraPosition = newLogicalPosition;
            
            // Set actual camera position - use logical X and Z, keep Y at 5 for top-down view
            Vector3 actualCameraPosition = new Vector3(newLogicalPosition.x, 5f, newLogicalPosition.z);
            topDownCamera.transform.position = actualCameraPosition;
            
            Debug.Log($"Logical position: X: {newLogicalPosition.x:F1}, Z: {newLogicalPosition.z:F1}");
            Debug.Log($"Camera position: X: {actualCameraPosition.x:F1}, Y: {actualCameraPosition.y:F1}, Z: {actualCameraPosition.z:F1}");
            Debug.Log($"Boundaries - X: ±{dynamicBoundaryX:F1}, Z: ±{dynamicBoundaryZ:F1}");
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (isFullscreen && topDownCamera != null)
        {
            // Get scroll direction (positive for zoom in, negative for zoom out)
            float scrollDelta = eventData.scrollDelta.y;
            
            // Apply zoom using the shared method
            ApplyZoom(-scrollDelta * zoomSpeed);
            
            Debug.Log($"Scroll Zoom: ScrollDelta={scrollDelta}");
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
            Vector3 newPosition = new Vector3(arCameraPosition.x, 5f, -8f);
            topDownCamera.transform.position = newPosition;
            
            // Use AR camera position as starting point but set proper map center for boundaries
            // Map center should be at the center of your 101x36 map, not AR camera position
            mapCenter = new Vector3(-5.848206f, 0, -8); // Assuming your map is centered at origin
            
            // Initialize logical camera position (start at AR camera position)
            logicalCameraPosition = new Vector3(arCameraPosition.x, 5f, arCameraPosition.z);
            
            Debug.Log($"Camera positioned above AR camera: AR at {arCameraPosition}, TopDown at {newPosition}");
        }
        else
        {
            // Fallback to original behavior
            Vector3 newPosition = topDownCamera.transform.position;
            newPosition.y = 5f; // Set proper height
            topDownCamera.transform.position = newPosition;
            
            // Use proper map center for boundaries
            mapCenter = new Vector3(-5.848206f, 0, -8); // Assuming your map is centered at origin
            
            // Initialize logical camera position
            logicalCameraPosition = new Vector3(newPosition.x, 5f, newPosition.z);
            
            Debug.Log($"Camera positioned at: {topDownCamera.transform.position}");
        }

        // Map center for boundary calculations (center of your 101x36 map)
        Debug.Log($"Map center set to: {mapCenter}");

        // Significantly increase orthographic size to show much more area when enlarged
        float newOrthographicSize = originalOrthographicSize * (1 + coverageIncrease);
        topDownCamera.orthographicSize = newOrthographicSize;
        
        Debug.Log($"Camera adjusted: Original size: {originalOrthographicSize}, New size: {newOrthographicSize}");
        Debug.Log($"Map center set to: {mapCenter}");
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

