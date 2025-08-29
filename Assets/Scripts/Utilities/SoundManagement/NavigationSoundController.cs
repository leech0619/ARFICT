using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages directional audio instructions for navigation (turn left, right, etc.)
/// Automatically blocks navigation sounds when arrival dialog is active to prevent interference
/// </summary>
public class NavigationSoundController : MonoBehaviour
{
    [Header("Navigation Audio Clips")]
    public AudioClip turnLeftClip; // Sound for left turn instructions
    public AudioClip turnRightClip; // Sound for right turn instructions
    public AudioClip continueStraightClip; // Sound for straight/forward instructions
    public AudioClip uTurnClip; // Sound for U-turn instructions
    
    [Header("Audio Settings")]
    public AudioSource audioSource; // Audio component for playing instruction sounds
    public float volume = 1f; // Volume level for navigation sounds
    public bool enableNavigationSounds = true; // Toggle to enable/disable navigation sounds
    
    [Header("Sound Timing")]
    public float instructionCooldown = 2f; // Minimum time between ANY instructions
    public float sameInstructionCooldown = 5f; // Extra cooldown for repeating the same instruction
    public float minimumMovementDistance = 2f; // Minimum distance user must move before next instruction
    
    // State tracking variables
    private float lastInstructionTime = 0f; // Time when last instruction was played
    private string lastInstruction = ""; // Last instruction that was played
    private Vector3 lastInstructionPosition = Vector3.zero; // Position where last instruction was given
    
    // External component references
    private SoundController soundController; // Reference to check global mute state
    private ArriveDialog arriveDialog; // Reference to check if arrival dialog is blocking navigation sounds
    
    void Start()
    {
        // Initialize AudioSource component if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure AudioSource for navigation instructions
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for UI feedback
        
        // Find SoundController to check global mute state
        soundController = FindObjectOfType<SoundController>();
        
        // Find ArriveDialog to check if arrival dialog is blocking navigation sounds
        arriveDialog = FindObjectOfType<ArriveDialog>();
        
        Debug.Log("NavigationSoundController initialized");
    }
    
    /// <summary>
    /// Play turn left instruction sound
    /// </summary>
    public void PlayTurnLeft()
    {
        PlayNavigationSound(turnLeftClip, "Turn Left");
    }
    
    /// <summary>
    /// Play turn right instruction sound
    /// </summary>
    public void PlayTurnRight()
    {
        PlayNavigationSound(turnRightClip, "Turn Right");
    }
    
    /// <summary>
    /// Play continue straight instruction sound
    /// </summary>
    public void PlayContinueStraight()
    {
        PlayNavigationSound(continueStraightClip, "Continue Straight");
    }
    
    /// <summary>
    /// Play U-turn instruction sound
    /// </summary>
    public void PlayUTurn()
    {
        PlayNavigationSound(uTurnClip, "Make U-Turn");
    }
    
    /// <summary>
    /// Plays navigation instruction based on angle direction
    /// </summary>
    /// <param name="angle">Angle in degrees (-180 to 180, negative = left, positive = right)</param>
    public void PlayDirectionInstruction(float angle)
    {
        float absAngle = Mathf.Abs(angle);
        
        // Determine appropriate instruction based on angle thresholds
        if (absAngle > 150f) // U-turn detection (150°+ turn)
        {
            PlayUTurn();
        }
        else if (angle < -40f) // Major left turns
        {
            PlayTurnLeft();
        }
        else if (angle > 40f) // Major right turns
        {
            PlayTurnRight();
        }
        else // Continue straight (angle between -40 and 40 degrees)
        {
            PlayContinueStraight();
        }
    }
    
    /// <summary>
    /// Plays navigation instruction based on direction string
    /// </summary>
    /// <param name="direction">Direction string: "left", "right", "straight", "uturn"</param>
    public void PlayDirectionInstruction(string direction)
    {
        direction = direction.ToLower().Trim();
        
        // Match direction strings to appropriate instruction sounds
        switch (direction)
        {
            case "left":
            case "turn left":
                PlayTurnLeft();
                break;
            case "right":
            case "turn right":
                PlayTurnRight();
                break;
            case "straight":
            case "continue straight":
            case "forward":
                PlayContinueStraight();
                break;
            case "uturn":
            case "u-turn":
            case "u turn":
            case "make u-turn":
                PlayUTurn();
                break;
            default:
                Debug.LogWarning($"Unknown direction instruction: {direction}");
                break;
        }
    }
    
    /// <summary>
    /// Core method that handles playing navigation sounds with spam prevention
    /// </summary>
    private void PlayNavigationSound(AudioClip clip, string instruction)
    {
        // Check if navigation sounds are globally enabled
        if (!enableNavigationSounds)
        {
            Debug.Log($"Navigation sounds disabled - skipping: {instruction}");
            return;
        }
        
        // Check if sounds are globally muted via SoundController
        if (soundController != null && soundController.IsSoundMuted())
        {
            Debug.Log($"Sounds are muted - skipping: {instruction}");
            return;
        }
        
        // Check if arrival dialog is active and blocking navigation sounds
        if (arriveDialog != null && arriveDialog.IsDialogActive())
        {
            Debug.Log($"Arrival dialog is active - blocking navigation sound: {instruction}");
            return;
        }
        
        // Validate audio clip is assigned
        if (clip == null)
        {
            Debug.LogWarning($"Audio clip not assigned for: {instruction}");
            return;
        }
        
        // Calculate time and distance since last instruction for spam prevention
        float currentTime = Time.time;
        float timeSinceLastInstruction = currentTime - lastInstructionTime;
        Vector3 currentPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
        float distanceSinceLastInstruction = Vector3.Distance(currentPosition, lastInstructionPosition);
        
        // Prevent instruction spam with general cooldown
        if (timeSinceLastInstruction < instructionCooldown)
        {
            Debug.Log($"General instruction cooldown active - skipping: {instruction} (last played {timeSinceLastInstruction:F1}s ago)");
            return;
        }
        
        // Extra cooldown for repeating the same instruction
        if (lastInstruction == instruction && timeSinceLastInstruction < sameInstructionCooldown)
        {
            Debug.Log($"Same instruction cooldown active - skipping repeated: {instruction} (last played {timeSinceLastInstruction:F1}s ago)");
            return;
        }
        
        // Require minimum movement to prevent spam when standing still
        if (lastInstructionPosition != Vector3.zero && distanceSinceLastInstruction < minimumMovementDistance)
        {
            Debug.Log($"Insufficient movement - skipping: {instruction} (moved {distanceSinceLastInstruction:F1}m, need {minimumMovementDistance}m)");
            return;
        }
        
        // Play the instruction sound and update tracking variables
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
            
            lastInstructionTime = currentTime;
            lastInstruction = instruction;
            lastInstructionPosition = currentPosition;
            
            Debug.Log($"Playing navigation instruction: {instruction} at position {currentPosition}");
        }
        else
        {
            Debug.LogError("AudioSource component is missing!");
        }
    }
    
    /// <summary>
    /// Toggles navigation sounds on or off
    /// </summary>
    public void SetNavigationSoundsEnabled(bool enabled)
    {
        enableNavigationSounds = enabled;
        Debug.Log($"Navigation sounds {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Sets the volume level for navigation sounds
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
        Debug.Log($"Navigation sound volume set to: {volume}");
    }
    
    /// <summary>
    /// Returns whether navigation sounds are currently enabled
    /// </summary>
    public bool AreNavigationSoundsEnabled()
    {
        return enableNavigationSounds;
    }
    
    /// <summary>
    /// Stops any currently playing navigation instruction sound
    /// </summary>
    public void StopNavigationSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Navigation sound stopped");
        }
    }
    
    /// <summary>
    /// Resets instruction cooldown timers (useful when starting new navigation)
    /// </summary>
    public void ResetInstructionCooldown()
    {
        lastInstructionTime = 0f;
        lastInstruction = "";
        lastInstructionPosition = Vector3.zero;
        Debug.Log("Navigation instruction cooldown reset");
    }
    
    /// <summary>
    /// Completely resets navigation sound state (stops sounds and clears cooldowns)
    /// </summary>
    public void ResetNavigationSoundState()
    {
        StopNavigationSound();
        ResetInstructionCooldown();
        Debug.Log("Navigation sound state completely reset");
    }
    
    /// <summary>
    /// Manually set the arrival dialog reference (if automatic finding doesn't work)
    /// </summary>
    public void SetArriveDialog(ArriveDialog dialog)
    {
        arriveDialog = dialog;
        Debug.Log("ArriveDialog reference manually set");
    }
}
