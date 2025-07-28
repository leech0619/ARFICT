using UnityEngine;

public class ArrowFollowRotation : MonoBehaviour
{
    [Header("References")]
    public RectTransform arrowIndicator;
    
    [Header("Settings")]
    public bool useCompass = true; // Use device compass
    public bool useGyroscope = false; // Use device gyroscope
    public float rotationOffset = 0f; // Adjust arrow orientation
    
    void Start()
    {
        Debug.Log("ArrowFollowRotation: Script initialized");
        
        // Enable compass if available
        if (useCompass && SystemInfo.supportsLocationService)
        {
            Input.compass.enabled = true;
            Debug.Log("Compass enabled");
        }
        
        // Enable gyroscope if available and selected
        if (useGyroscope && SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("Gyroscope enabled");
        }
    }
    
    void Update()
    {
        RotateArrow();
    }
    
    void RotateArrow()
    {
        if (arrowIndicator == null)
        {
            Debug.LogWarning("ArrowFollowRotation: Arrow indicator not assigned!");
            return;
        }
        
        float deviceRotation = 0f;
        
        if (useCompass && Input.compass.enabled)
        {
            // Get compass heading (0-360 degrees, where 0 is North)
            deviceRotation = Input.compass.magneticHeading;
        }
        else if (useGyroscope && Input.gyro.enabled)
        {
            // Get gyroscope rotation (Y-axis for horizontal rotation)
            deviceRotation = Input.gyro.attitude.eulerAngles.y;
        }
        else
        {
            // Fallback: use camera transform rotation
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                deviceRotation = mainCamera.transform.eulerAngles.y;
            }
        }
        
        // Apply rotation with offset to arrow
        float finalRotation = deviceRotation + rotationOffset;
        arrowIndicator.rotation = Quaternion.Euler(0, 0, finalRotation);
    }
}
