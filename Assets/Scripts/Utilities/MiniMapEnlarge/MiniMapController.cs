//using UnityEngine;
//using UnityEngine.EventSystems;

//public class MinimapController : MonoBehaviour, IPointerClickHandler, IDragHandler
//{
//    public Camera topDownCamera; // Reference to the top-down camera
//    public float dragSpeed = 2f; // Speed of dragging
//    public float coverageIncrease = 0.2f; // Percentage increase in coverage when clicked

//    private bool isFullscreen = false;
//    private RectTransform minimapRectTransform;
//    private Vector2 originalSize;
//    private Vector2 dragStartPosition;

//    void Start()
//    {
//        // Get the RectTransform component of the minimap
//        minimapRectTransform = GetComponent<RectTransform>();
//        // Store the original size of the minimap
//        originalSize = minimapRectTransform.sizeDelta;
//    }

//    public void OnPointerClick(PointerEventData eventData)
//    {
//        if (!isFullscreen)
//        {
//            // Enlarge the minimap
//            EnlargeMinimap();
//            // Move the top-down camera up
//            MoveCameraUp();
//        }
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        if (isFullscreen)
//        {
//            // If dragging, perform panning
//            Vector2 delta = eventData.delta;
//            delta /= minimapRectTransform.rect.size; // Normalize delta

//            // Calculate the forward and right directions of the camera
//            Vector3 forwardDirection = topDownCamera.transform.forward;
//            Vector3 rightDirection = topDownCamera.transform.right;

//            // Calculate the movement delta in world space
//            Vector3 movementDelta = forwardDirection * delta.y * dragSpeed + rightDirection * delta.x * dragSpeed;

//            // Move the camera accordingly
//            topDownCamera.transform.position += movementDelta;
//        }
//    }

//    void EnlargeMinimap()
//    {
//        // If not fullscreen, enlarge to match screen size while maintaining aspect ratio
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

//        // Set fullscreen flag
//        isFullscreen = true;
//    }

//    void MoveCameraUp()
//    {
//        // Calculate the amount to move the camera up
//        float originalOrthographicSize = topDownCamera.orthographicSize;
//        float newOrthographicSize = originalOrthographicSize * (1 + coverageIncrease);
//        float deltaY = (newOrthographicSize - originalOrthographicSize) * 0.5f;

//        // Move the camera up
//        topDownCamera.transform.position += Vector3.up * deltaY;
//    }
//}



using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public Camera topDownCamera; // Reference to the top-down camera
    public float dragSpeed = 2f; // Speed of dragging
    public float coverageIncrease = 0.2f; // Percentage increase in coverage when clicked

    public CloseButton closeButton;

    private bool isFullscreen = false;
    private RectTransform minimapRectTransform;
    private Vector2 originalSize;
    private Vector2 dragStartPosition;

    void Start()
    {
        // Get the RectTransform component of the minimap
        minimapRectTransform = GetComponent<RectTransform>();
        // Store the original size of the minimap
        originalSize = minimapRectTransform.sizeDelta;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFullscreen)
        {
            // Enlarge the minimap
            EnlargeMinimap();
            // Move the top-down camera up
            MoveCameraUp();
            closeButton.ShowButton();
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

        // Set fullscreen flag
        isFullscreen = true;

        Vector3 forwardDirection = topDownCamera.transform.forward;
        float forwardOffset = 25.0f; // Adjust this value as needed
        topDownCamera.transform.position += forwardDirection * forwardOffset;
    }

    void MoveCameraUp()
    {
        // Calculate the amount to move the camera up
        float originalOrthographicSize = topDownCamera.orthographicSize;
        float newOrthographicSize = originalOrthographicSize * (1 + coverageIncrease);
        float deltaY = (newOrthographicSize - originalOrthographicSize) * 0.5f;

        // Move the camera up
        topDownCamera.transform.position += Vector3.up * deltaY;
    }

    //void ShrinkMinimap()
    //{
    //    // Shrink back to original size
    //    minimapRectTransform.sizeDelta = originalSize;
    //    // Reset anchoring and position
    //    minimapRectTransform.anchorMin = new Vector2(0, 1);
    //    minimapRectTransform.anchorMax = new Vector2(0, 1);
    //    minimapRectTransform.pivot = new Vector2(0, 1);
    //    minimapRectTransform.anchoredPosition = new Vector2(0, -originalSize.y); // Move to bottom left corner

    //    // Reset fullscreen flag
    //    isFullscreen = false;
    //}
}

