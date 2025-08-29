using UnityEngine;

/// <summary>
/// Plays a sound when the user arrives at a navigation target
/// Only respects global mute settings, can play even when arrival dialog is active
/// </summary>
public class ArriveTargetSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Audio component for playing sounds
    [SerializeField] private AudioClip arrivalSound; // Sound clip to play on arrival
    [SerializeField] private float triggerDistance = 1.5f; // Distance to trigger arrival sound
    
    [Header("Sound Control")]
    [SerializeField] private float volume = 1.0f; // Audio volume level
    [SerializeField] private bool playOnlyOnce = true; // Play sound only once per target
    
    // State tracking variables
    private bool hasPlayedForCurrentTarget = false; // Prevents sound spam for same target
    private string currentTargetName = ""; // Name of current navigation target
    private Vector3 lastTargetPosition = Vector3.zero; // Last known target position
    private Vector3 userPositionWhenTargetSet = Vector3.zero; // User position when target was selected
    private float minMovementRequired = 2f; // User must move at least 2m from where target was set
    private bool hasMovedAwayFromStartPosition = false; // Tracks if user has moved away from start
    
    // External component references
    private SoundController soundController; // Reference to check global mute state
    
    private void Start()
    {
        // Initialize AudioSource component if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // Create AudioSource if it doesn't exist
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure AudioSource for UI feedback
        if (audioSource != null)
        {
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound for UI feedback
        }
        
        // Find SoundController to check global mute state
        soundController = FindObjectOfType<SoundController>();
        
        Debug.Log("ArriveTargetSound initialized");
    }
    
    /// <summary>
    /// Checks if user has arrived at target and plays sound if appropriate
    /// </summary>
    /// <param name="distanceToTarget">Current distance to target in meters</param>
    /// <param name="targetName">Name of current target (optional, for tracking)</param>
    /// <param name="currentUserPosition">Current position of the user</param>
    /// <param name="targetPosition">Position of the target</param>
    public void CheckArrival(float distanceToTarget, string targetName = "", Vector3 currentUserPosition = default, Vector3 targetPosition = default)
    {
        // Handle new target selection
        if (!string.IsNullOrEmpty(targetName) && targetName != currentTargetName)
        {
            currentTargetName = targetName;
            hasPlayedForCurrentTarget = false;
            hasMovedAwayFromStartPosition = false;
            userPositionWhenTargetSet = currentUserPosition;
            lastTargetPosition = targetPosition;
            Debug.Log($"New target detected: {targetName} - User must move {minMovementRequired}m before arrival sound can play");
        }
        
        // Check if user has moved enough distance from starting position
        if (!hasMovedAwayFromStartPosition && currentUserPosition != Vector3.zero)
        {
            float distanceFromStart = Vector3.Distance(currentUserPosition, userPositionWhenTargetSet);
            if (distanceFromStart >= minMovementRequired)
            {
                hasMovedAwayFromStartPosition = true;
                Debug.Log($"User has moved {distanceFromStart:F1}m from start position - arrival sound now enabled");
            }
        }
        
        // Prevent sound if user hasn't moved away from starting position
        if (!hasMovedAwayFromStartPosition)
        {
            // Debug.Log($"Sound blocked - user hasn't moved {minMovementRequired}m from start position yet");
            return;
        }
        
        // Check for arrival and play sound if conditions are met
        if (distanceToTarget <= triggerDistance)
        {
            // Only play if we haven't played for this target yet (if playOnlyOnce is true)
            if (!playOnlyOnce || !hasPlayedForCurrentTarget)
            {
                PlayArrivalSound();
                hasPlayedForCurrentTarget = true;
                
                Debug.Log($"User arrived at target! Distance: {distanceToTarget:F2}m");
            }
        }
        else if (distanceToTarget > triggerDistance * 2f) // Reset if user moves away significantly
        {
            hasPlayedForCurrentTarget = false;
        }
    }
    
    /// <summary>
    /// Plays the arrival sound clip
    /// </summary>
    private void PlayArrivalSound()
    {
        // Check if sounds are globally muted (respect global mute but allow during dialog)
        if (soundController != null && soundController.IsSoundMuted())
        {
            Debug.Log("Arrival sound blocked - sounds are globally muted");
            return;
        }
        
        if (audioSource != null && arrivalSound != null)
        {
            audioSource.clip = arrivalSound;
            audioSource.volume = volume;
            audioSource.Play();
            
            Debug.Log("Arrival sound played!");
        }
        else
        {
            // Log warnings for missing components
            if (audioSource == null)
                Debug.LogWarning("AudioSource is not assigned!");
            if (arrivalSound == null)
                Debug.LogWarning("Arrival sound clip is not assigned!");
        }
    }
    
    /// <summary>
    /// Manually trigger arrival sound (for testing or special cases)
    /// </summary>
    public void TriggerArrivalSound()
    {
        PlayArrivalSound();
    }
    
    /// <summary>
    /// Reset the sound state for a new navigation session
    /// </summary>
    public void ResetSoundState()
    {
        hasPlayedForCurrentTarget = false;
        currentTargetName = "";
        hasMovedAwayFromStartPosition = false;
        userPositionWhenTargetSet = Vector3.zero;
        lastTargetPosition = Vector3.zero;
        Debug.Log("Arrival sound state reset");
    }
    
    /// <summary>
    /// Set the trigger distance dynamically
    /// </summary>
    public void SetTriggerDistance(float distance)
    {
        triggerDistance = distance;
        Debug.Log($"Trigger distance set to: {distance}m");
    }
    
    /// <summary>
    /// Set the volume dynamically
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        Debug.Log($"Volume set to: {volume}");
    }
    
    /// <summary>
    /// Set the minimum movement required before arrival sound can play
    /// </summary>
    public void SetMinMovementRequired(float movement)
    {
        minMovementRequired = movement;
        Debug.Log($"Min movement required set to: {movement}m");
    }
    
    /// <summary>
    /// Manually set the sound controller reference (if automatic finding doesn't work)
    /// </summary>
    public void SetSoundController(SoundController controller)
    {
        soundController = controller;
        Debug.Log("SoundController reference manually set");
    }
}