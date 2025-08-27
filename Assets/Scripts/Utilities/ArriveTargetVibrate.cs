using UnityEngine;

public class ArriveTargetVibrate : MonoBehaviour
{
    [Header("Vibration Settings")]
    [SerializeField] private float triggerDistance = 1.5f; // Distance to trigger vibration
    [SerializeField] private bool vibrateOnlyOnce = true; // Vibrate only once per target
    
    [Header("Vibration Patterns")]
    [SerializeField] private VibrationType vibrationType = VibrationType.Short;
    [SerializeField] private int customVibrationCount = 2; // For custom pattern
    [SerializeField] private float customVibrationInterval = 0.2f; // Interval between vibrations
    
    [Header("Platform Settings")]
    [SerializeField] private bool enableOnMobile = true;
    [SerializeField] private bool enableOnDesktop = true; // For testing purposes
    
    public enum VibrationType
    {
        Short,      // Single short vibration
        Long,       // Single long vibration  
        Double,     // Two quick vibrations
        Custom      // Custom pattern
    }
    
    private bool hasVibratedForCurrentTarget = false;
    private string currentTargetName = "";
    private Vector3 lastTargetPosition = Vector3.zero;
    private Vector3 userPositionWhenTargetSet = Vector3.zero;
    private float minMovementRequired = 2f; // User must move at least 2m from where target was set
    private bool hasMovedAwayFromStartPosition = false;
    
    private void Start()
    {
        Debug.Log("ArriveTargetVibrate initialized");
        
        // Check if vibration is supported on this platform
        CheckVibrationSupport();
    }
    
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
    /// Check if user has arrived at target and trigger vibration if appropriate
    /// </summary>
    /// <param name="distanceToTarget">Current distance to target in meters</param>
    /// <param name="targetName">Name of current target</param>
    /// <param name="currentUserPosition">Current position of the user</param>
    /// <param name="targetPosition">Position of the target</param>
    public void CheckArrival(float distanceToTarget, string targetName = "", Vector3 currentUserPosition = default, Vector3 targetPosition = default)
    {
        // Check if we have a new target
        if (!string.IsNullOrEmpty(targetName) && targetName != currentTargetName)
        {
            currentTargetName = targetName;
            hasVibratedForCurrentTarget = false;
            hasMovedAwayFromStartPosition = false;
            userPositionWhenTargetSet = currentUserPosition;
            lastTargetPosition = targetPosition;
            Debug.Log($"New target detected for vibration: {targetName} - User must move {minMovementRequired}m before vibration can trigger");
        }
        
        // Check if user has moved away from the starting position
        if (!hasMovedAwayFromStartPosition && currentUserPosition != Vector3.zero)
        {
            float distanceFromStart = Vector3.Distance(currentUserPosition, userPositionWhenTargetSet);
            if (distanceFromStart >= minMovementRequired)
            {
                hasMovedAwayFromStartPosition = true;
                Debug.Log($"User has moved {distanceFromStart:F1}m from start position - vibration now enabled");
            }
        }
        
        // Only check for arrival if user has moved away from starting position
        if (!hasMovedAwayFromStartPosition)
        {
            return;
        }
        
        // Check if user has arrived (within trigger distance)
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
    /// Trigger vibration based on selected pattern
    /// </summary>
    private void TriggerVibration()
    {
        // Check if vibration should be triggered on current platform
        if (!ShouldVibrateOnCurrentPlatform())
        {
            Debug.Log("Vibration not enabled for current platform");
            return;
        }
        
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
    /// Check if vibration should be triggered on current platform
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
    /// Short vibration (default mobile vibration)
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
    /// Long vibration using Android-specific functionality
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
    /// Double vibration pattern
    /// </summary>
    private void VibrateDouble()
    {
        StartCoroutine(VibratePattern(new float[] { 0f, 200f, 100f, 200f }));
    }
    
    /// <summary>
    /// Custom vibration pattern
    /// </summary>
    private void VibrateCustom()
    {
        // Create custom pattern array
        float[] pattern = new float[customVibrationCount * 2];
        for (int i = 0; i < customVibrationCount; i++)
        {
            pattern[i * 2] = i == 0 ? 0f : customVibrationInterval * 1000f; // Delay
            pattern[i * 2 + 1] = 150f; // Vibration duration
        }
        
        StartCoroutine(VibratePattern(pattern));
    }
    
    /// <summary>
    /// Execute vibration pattern
    /// </summary>
    private System.Collections.IEnumerator VibratePattern(float[] pattern)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
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
        // Fallback for iOS and desktop
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