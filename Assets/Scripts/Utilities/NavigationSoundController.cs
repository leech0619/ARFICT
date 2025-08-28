using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSoundController : MonoBehaviour
{
    [Header("Navigation Audio Clips")]
    public AudioClip turnLeftClip;
    public AudioClip turnRightClip;
    public AudioClip continueStraightClip;
    public AudioClip uTurnClip; // New U-turn audio clip
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float volume = 1f;
    public bool enableNavigationSounds = true;
    
    [Header("Sound Timing")]
    public float instructionCooldown = 3f; // Minimum time between ANY instructions (increased from 2f)
    public float sameInstructionCooldown = 5f; // Extra cooldown for repeating the same instruction
    public float minimumMovementDistance = 2f; // Minimum distance user must move before next instruction
    
    // Private variables
    private float lastInstructionTime = 0f;
    private string lastInstruction = "";
    private Vector3 lastInstructionPosition = Vector3.zero;
    
    // Sound controller reference for mute checking
    private SoundController soundController;
    
    void Start()
    {
        // Get or create AudioSource component
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure AudioSource
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for UI feedback
        
        // Find SoundController to check if sounds are muted
        soundController = FindObjectOfType<SoundController>();
        
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
    /// Play navigation instruction based on angle
    /// </summary>
    /// <param name="angle">Angle in degrees (-180 to 180, negative = left, positive = right)</param>
    public void PlayDirectionInstruction(float angle)
    {
        float absAngle = Mathf.Abs(angle);
        
        // Determine direction based on angle thresholds
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
    /// Play navigation instruction based on direction string
    /// </summary>
    /// <param name="direction">Direction string: "left", "right", "straight", "uturn"</param>
    public void PlayDirectionInstruction(string direction)
    {
        direction = direction.ToLower().Trim();
        
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
    /// Core method to play navigation sounds with cooldown and mute checking
    /// </summary>
    private void PlayNavigationSound(AudioClip clip, string instruction)
    {
        // Check if navigation sounds are enabled
        if (!enableNavigationSounds)
        {
            Debug.Log($"Navigation sounds disabled - skipping: {instruction}");
            return;
        }
        
        // Check if sounds are globally muted
        if (soundController != null && soundController.IsSoundMuted())
        {
            Debug.Log($"Sounds are muted - skipping: {instruction}");
            return;
        }
        
        // Check if audio clip is assigned
        if (clip == null)
        {
            Debug.LogWarning($"Audio clip not assigned for: {instruction}");
            return;
        }
        
        // Check cooldown to prevent spam
        float currentTime = Time.time;
        float timeSinceLastInstruction = currentTime - lastInstructionTime;
        Vector3 currentPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
        float distanceSinceLastInstruction = Vector3.Distance(currentPosition, lastInstructionPosition);
        
        // Check if we're in the general cooldown period (blocks ALL instructions)
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
        
        // Check if user has moved enough since last instruction (prevents spam when standing still)
        if (lastInstructionPosition != Vector3.zero && distanceSinceLastInstruction < minimumMovementDistance)
        {
            Debug.Log($"Insufficient movement - skipping: {instruction} (moved {distanceSinceLastInstruction:F1}m, need {minimumMovementDistance}m)");
            return;
        }
        
        // Play the sound
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
    /// Enable or disable navigation sounds
    /// </summary>
    public void SetNavigationSoundsEnabled(bool enabled)
    {
        enableNavigationSounds = enabled;
        Debug.Log($"Navigation sounds {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Set the volume for navigation sounds
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
    /// Check if navigation sounds are currently enabled
    /// </summary>
    public bool AreNavigationSoundsEnabled()
    {
        return enableNavigationSounds;
    }
    
    /// <summary>
    /// Stop any currently playing navigation sound
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
    /// Reset instruction cooldown (useful when starting new navigation)
    /// </summary>
    public void ResetInstructionCooldown()
    {
        lastInstructionTime = 0f;
        lastInstruction = "";
        lastInstructionPosition = Vector3.zero;
        Debug.Log("Navigation instruction cooldown reset");
    }
    
    /// <summary>
    /// Reset all navigation sound state (stop sounds and reset cooldown)
    /// </summary>
    public void ResetNavigationSoundState()
    {
        StopNavigationSound();
        ResetInstructionCooldown();
        Debug.Log("Navigation sound state completely reset");
    }
}
