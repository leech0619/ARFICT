using UnityEngine;
using UnityEngine.EventSystems;

public class MiniMapController : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public Camera topDownCamera;
    public float dragSpeed = 2f;
    public float coverageIncrease = 0.2f;
    public CloseButton closeButton;

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
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFullscreen)
        {
            EnlargeMinimap();
            AdjustCameraForFullscreen();
            closeButton.ShowButton();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isFullscreen)
        {
            Vector2 delta = eventData.delta;
            delta /= minimapRectTransform.rect.size;

            Vector3 forwardDirection = topDownCamera.transform.forward;
            Vector3 rightDirection = topDownCamera.transform.right;
            // Invert the horizontal (x) direction for more intuitive map dragging
            Vector3 movementDelta = forwardDirection * delta.y * dragSpeed + rightDirection * (-delta.x) * dragSpeed;

            topDownCamera.transform.position += movementDelta;
        }
    }

    void EnlargeMinimap()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float squarePercentage = 0.7f;
        float squareSize = screenHeight * squarePercentage;
        
        if (squareSize > screenWidth * 0.9f)
        {
            squareSize = screenWidth * 0.9f;
        }

        // Temporarily reparent to Canvas to avoid parent positioning issues
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            minimapRectTransform.SetParent(parentCanvas.transform, false);
        }

        // Set anchors to center
        minimapRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        minimapRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        minimapRectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Set size
        minimapRectTransform.sizeDelta = new Vector2(squareSize, squareSize);
        
        // Set position to center (0, 0) relative to center anchor
        minimapRectTransform.anchoredPosition = new Vector2(0, 0);

        isFullscreen = true;
    }

    void AdjustCameraForFullscreen()
    {
        if (topDownCamera == null) return;

        // Only adjust orthographic size, don't move position to avoid issues
        topDownCamera.orthographicSize = originalOrthographicSize * (1 + coverageIncrease);
    }

    // PUBLIC method for external scripts to restore minimap
    public void RestoreMinimap()
    {
        if (!isFullscreen) return;

        // Restore original parent first
        if (originalParent != null)
        {
            minimapRectTransform.SetParent(originalParent, false);
        }

        // Restore ALL original UI settings
        minimapRectTransform.sizeDelta = originalSize;
        minimapRectTransform.anchoredPosition = originalAnchoredPosition;
        minimapRectTransform.anchorMin = originalAnchorMin;
        minimapRectTransform.anchorMax = originalAnchorMax;
        minimapRectTransform.pivot = originalPivot;

        // Restore camera settings - but DON'T reset position, only orthographic size
        if (topDownCamera != null)
        {
            // Only restore orthographic size, keep current position to follow indicator
            topDownCamera.orthographicSize = originalOrthographicSize;
            // Do NOT restore position: topDownCamera.transform.position = originalCameraPosition;
        }

        closeButton.HideButton();
        isFullscreen = false;
    }
}

