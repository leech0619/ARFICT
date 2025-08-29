using UnityEngine;

/// <summary>
/// Triggers device vibration when the user arrives at a navigation target
/// </summary>
public class ArriveTargetVibrate : MonoBehaviour
{
    [Header("Vibration Settings")]
    [SerializeField] private float triggerDistance = 1.5f; // Distance to trigger vibration
    [SerializeField] private bool vibrateOnlyOnce = true; // Vibrate only once per target
    
    [Header("Vibration Patterns")]
    [SerializeField] private VibrationType vibrationType = VibrationType.Short; // Type of vibration pattern
    [SerializeField] private int customVibrationCount = 2; // Number of vibrations for custom pattern
    [SerializeField] private float customVibrationInterval = 0.2f; // Time between vibrations in custom pattern
    
    [Header("Platform Settings")]
    [SerializeField] private bool enableOnMobile = true; // Enable vibration on mobile devices
    [SerializeField] private bool enableOnDesktop = true; // Enable vibration simulation on desktop for testing

    // Available vibration patterns
    public enum VibrationType
    {
        Short,      // Single short vibration
        Long,       // Single long vibration  
        Double,     // Two quick vibrations
        Custom      // Custom pattern with configurable count and interval
    }
    
    // State tracking variables
    private bool hasVibratedForCurrentTarget = false; // Prevents vibration spam for same target
    private string currentTargetName = ""; // Name of current navigation target
    private Vector3 lastTargetPosition = Vector3.zero; // Last known target position
    private Vector3 userPositionWhenTargetSet = Vector3.zero; // User position when target was selected
    private float minMovementRequired = 2f; // User must move at least 2m from where target was set
    private bool hasMovedAwayFromStartPosition = false; // Tracks if user has moved away from start
    
    private void Start()
    {
        Debug.Log("ArriveTargetVibrate initialized");
        
        // Check platform compatibility for vibration support
        CheckVibrationSupport();
    }
    
    /// <summary>
    /// Checks if vibration is supported and enabled on the current platform
    /// </summary>
    private void CheckVibrationSupport()
    {
        #if UNITY_ANDROID || UNITY_IOS
        if (enableOnMobile)
        {
            Debug.Log("Vibration supported on mobile platform");
        }
        else
        {
            Debug.Log("Vibration disabled for mobile platform");
        }
        #else
        if (enableOnDesktop)
        {
            Debug.Log("Vibration enabled for desktop testing");
        }
        else
        {
            Debug.Log("Vibration not supported on this platform");
        }
        #endif
    }
    
    /// <summary>
    /// Checks if user has arrived at target and triggers vibration if appropriate
    /// </summary>
    /// <param name="distanceToTarget">Current distance to target in meters</param>
    /// <param name="targetName">Name of current target</param>
    /// <param name="currentUserPosition">Current position of the user</param>
    /// <param name="targetPosition">Position of the target</param>
    public void CheckArrival(float distanceToTarget, string targetName = "", Vector3 currentUserPosition = default, Vector3 targetPosition = default)
    {
        // Handle new target selection
        if (!string.IsNullOrEmpty(targetName) && targetName != currentTargetName)
        {
            currentTargetName = targetName;
            hasVibratedForCurrentTarget = false;
            hasMovedAwayFromStartPosition = false;
            userPositionWhenTargetSet = currentUserPosition;
            lastTargetPosition = targetPosition;
            Debug.Log($"New target detected for vibration: {targetName} - User must move {minMovementRequired}m before vibration can trigger");
        }
        
        // Check if user has moved enough distance from starting position
        if (!hasMovedAwayFromStartPosition && currentUserPosition != Vector3.zero)
        {
            float distanceFromStart = Vector3.Distance(currentUserPosition, userPositionWhenTargetSet);
            if (distanceFromStart >= minMovementRequired)
            {
                hasMovedAwayFromStartPosition = true;
                Debug.Log($"User has moved {distanceFromStart:F1}m from start position - vibration now enabled");
            }
        }
        
        // Prevent vibration if user hasn't moved away from starting position
        if (!hasMovedAwayFromStartPosition)
        {
            return;
        }
        
        // Check for arrival and trigger vibration if conditions are met
        if (distanceToTarget <= triggerDistance)
        {
            // Only vibrate if we haven't vibrated for this target yet (if vibrateOnlyOnce is true)
            if (!vibrateOnlyOnce || !hasVibratedForCurrentTarget)
            {
                TriggerVibration();
                hasVibratedForCurrentTarget = true;
                
                Debug.Log($"User arrived at target! Distance: {distanceToTarget:F2}m - Vibration triggered");
            }
        }
        else if (distanceToTarget > triggerDistance * 2f) // Reset if user moves away significantly
        {
            hasVibratedForCurrentTarget = false;
        }
    }
    
    /// <summary>
    /// Triggers vibration based on the selected pattern type
    /// </summary>
    private void TriggerVibration()
    {
        // Check if vibration should be triggered on current platform
        if (!ShouldVibrateOnCurrentPlatform())
        {
            Debug.Log("Vibration not enabled for current platform");
            return;
        }
        
        // Execute the appropriate vibration pattern
        switch (vibrationType)
        {
            case VibrationType.Short:
                VibrateShort();
                break;
            case VibrationType.Long:
                VibrateLong();
                break;
            case VibrationType.Double:
                VibrateDouble();
                break;
            case VibrationType.Custom:
                VibrateCustom();
                break;
        }
        
        Debug.Log($"Vibration triggered: {vibrationType}");
    }
    
    /// <summary>
    /// Checks if vibration should be triggered based on platform settings
    /// </summary>
    private bool ShouldVibrateOnCurrentPlatform()
    {
        #if UNITY_ANDROID || UNITY_IOS
        return enableOnMobile;
        #else
        return enableOnDesktop;
        #endif
    }
    
    /// <summary>
    /// Executes a short vibration using the default mobile vibration
    /// </summary>
    private void VibrateShort()
    {
        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #else
        if (enableOnDesktop)
        {
            Debug.Log("VIBRATE: Short vibration (simulated on desktop)");
        }
        #endif
    }
    
    /// <summary>
    /// Executes a long vibration with custom duration (Android-specific)
    /// </summary>
    private void VibrateLong()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        vibrator.Call("vibrate", 800); // 800ms vibration
        #elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate(); // iOS doesn't support custom duration, fallback to default
        #else
        if (enableOnDesktop)
        {
            Debug.Log("VIBRATE: Long vibration (simulated on desktop)");
        }
        #endif
    }
    
    /// <summary>
    /// Executes a double vibration pattern (two quick vibrations)
    /// </summary>
    private void VibrateDouble()
    {
        StartCoroutine(VibratePattern(new float[] { 0f, 200f, 100f, 200f }));
    }
    
    /// <summary>
    /// Executes a custom vibration pattern based on user settings
    /// </summary>
    private void VibrateCustom()
    {
        // Create custom pattern array with alternating delays and vibrations
        float[] pattern = new float[customVibrationCount * 2];
        for (int i = 0; i < customVibrationCount; i++)
        {
            pattern[i * 2] = i == 0 ? 0f : customVibrationInterval * 1000f; // Delay
            pattern[i * 2 + 1] = 150f; // Vibration duration
        }
        
        StartCoroutine(VibratePattern(pattern));
    }
    
    /// <summary>
    /// Executes a vibration pattern with specified timing
    /// </summary>
    private System.Collections.IEnumerator VibratePattern(float[] pattern)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Use Android-specific vibration pattern API
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        
        long[] androidPattern = new long[pattern.Length];
        for (int i = 0; i < pattern.Length; i++)
        {
            androidPattern[i] = (long)pattern[i];
        }
        
        vibrator.Call("vibrate", androidPattern, -1);
        
        // Calculate total pattern duration and wait for it to complete
        float totalDuration = 0f;
        for (int i = 0; i < pattern.Length; i++)
        {
            totalDuration += pattern[i];
        }
        yield return new WaitForSeconds(totalDuration / 1000f);
        #else
        // Fallback pattern execution for iOS and desktop
        for (int i = 0; i < pattern.Length; i += 2)
        {
            if (i > 0) yield return new WaitForSeconds(pattern[i] / 1000f); // Delay
            if (i + 1 < pattern.Length)
            {
                #if UNITY_IOS && !UNITY_EDITOR
                Handheld.Vibrate();
                #else
                if (enableOnDesktop)
                {
                    Debug.Log($"VIBRATE: Pattern step {i/2 + 1} (simulated on desktop)");
                }
                #endif
            }
        }
        #endif
    }
    
    /// <summary>
    /// Manually trigger vibration (for testing)
    /// </summary>
    public void TriggerTestVibration()
    {
        TriggerVibration();
    }
    
    /// <summary>
    /// Reset vibration state for new navigation session
    /// </summary>
    public void ResetVibrateState()
    {
        hasVibratedForCurrentTarget = false;
        currentTargetName = "";
        hasMovedAwayFromStartPosition = false;
        userPositionWhenTargetSet = Vector3.zero;
        lastTargetPosition = Vector3.zero;
        Debug.Log("Vibration state reset");
    }
    
    /// <summary>
    /// Set the trigger distance dynamically
    /// </summary>
    public void SetTriggerDistance(float distance)
    {
        triggerDistance = distance;
        Debug.Log($"Vibration trigger distance set to: {distance}m");
    }
    
    /// <summary>
    /// Set the minimum movement required before vibration can trigger
    /// </summary>
    public void SetMinMovementRequired(float movement)
    {
        minMovementRequired = movement;
        Debug.Log($"Min movement required for vibration set to: {movement}m");
    }
    
    /// <summary>
    /// Set vibration type dynamically
    /// </summary>
    public void SetVibrationType(VibrationType type)
    {
        vibrationType = type;
        Debug.Log($"Vibration type set to: {type}");
    }
}