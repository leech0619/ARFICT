using UnityEngine;
using UnityEngine.EventSystems;

public class ViewMiniMap : MonoBehaviour, IDragHandler
{
    public Camera topDownCamera; // Reference to the top-down camera
    public float dragSpeed = 2f; // Speed of dragging
    public float coverageIncrease = 0.2f; // Percentage increase in coverage when clicked

    private bool isFullscreen = false;
    private RectTransform minimapRectTransform;
    private Vector2 originalSize;
    private Vector2 originalPosition;
    private Vector2 dragStartPosition;

    void Start()
    {
        // Get the RectTransform component of the minimap
        minimapRectTransform = GetComponent<RectTransform>();
        // Store the original size and position of the minimap
        originalSize = minimapRectTransform.sizeDelta;
        originalPosition = minimapRectTransform.anchoredPosition;
    }

    public void ViewMap()
    {
        if (!isFullscreen)
        {
            // Enlarge the minimap
            EnlargeMinimap();
            isFullscreen = true;
        }
    }

    public void CloseMap()
    {
        if (isFullscreen)
        {
            // Return to original size and position
            minimapRectTransform.sizeDelta = originalSize;
            minimapRectTransform.anchoredPosition = originalPosition;
            isFullscreen = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isFullscreen)
        {
            // If dragging, perform panning
            Vector2 delta = eventData.delta;
            delta /= minimapRectTransform.rect.size; // Normalize delta

            // Calculate the forward and right directions of the camera
            Vector3 forwardDirection = topDownCamera.transform.forward;
            Vector3 rightDirection = topDownCamera.transform.right;

            // Calculate the movement delta in world space
            Vector3 movementDelta = forwardDirection * delta.y * dragSpeed + rightDirection * delta.x * dragSpeed;

            // Move the camera accordingly
            topDownCamera.transform.position += movementDelta;
        }
    }

    void EnlargeMinimap()
    {
        // If not fullscreen, enlarge to match screen size while maintaining aspect ratio
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

        // Move the top-down camera up
        Vector3 forwardDirection = topDownCamera.transform.forward;
        float forwardOffset = 25.0f; // Adjust this value as needed
        topDownCamera.transform.position += forwardDirection * forwardOffset;
    }
}