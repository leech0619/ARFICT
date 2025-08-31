using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls global sound muting/unmuting with UI button management
/// Also manages global sound queue to prevent multiple simultaneous sounds
/// </summary>
public class SoundController : MonoBehaviour
{
    [Header("Sound Button References")]
    public GameObject soundOnButton;  // Button displayed when sound is enabled (clickable to mute)
    public GameObject soundOffButton; // Button displayed when sound is muted (clickable to unmute)
    
    [Header("Sound Settings")]
    public bool isSoundMuted = false; // Current global sound state
    
    [Header("Sound Queue Settings")]
    [SerializeField] private float soundCooldown = 0.3f; // Minimum time between sounds
    [SerializeField] private bool enableSoundQueue = true; // Enable/disable sound queue management
    
    // Volume management
    private float originalVolume = 1f; // Stores original volume level for restoration when unmuting
    
    // Sound queue management
    private bool isSoundPlaying = false; // Tracks if any sound is currently being managed
    private float lastSoundTime = 0f; // Time when last sound was played
    private Queue<System.Action> soundQueue = new Queue<System.Action>(); // Queue for pending sounds
    private Coroutine soundQueueCoroutine; // Coroutine for processing sound queue
    
    // Singleton pattern for global access
    private static SoundController instance;
    public static SoundController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SoundController>();
                if (instance == null)
                {
                    Debug.LogWarning("SoundController not found in scene!");
                }
            }
            return instance;
        }
    }
    
    void Start()
    {
        // Store the original volume level for restoration
        originalVolume = AudioListener.volume;
        
        // Set initial button visibility based on current sound state
        UpdateButtonStates();
        
        // Initialize singleton instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else if (instance != this)
        {
            // Destroy duplicate instances
            Destroy(gameObject);
            return;
        }
        
        // Set up button click event listeners
        if (soundOnButton != null)
        {
            Button onButton = soundOnButton.GetComponent<Button>();
            if (onButton != null)
            {
                onButton.onClick.AddListener(MuteSound);
            }
        }
        
        if (soundOffButton != null)
        {
            Button offButton = soundOffButton.GetComponent<Button>();
            if (offButton != null)
            {
                offButton.onClick.AddListener(UnmuteSound);
            }
        }
        
        Debug.Log($"SoundController initialized - Sound muted: {isSoundMuted}");
    }
    
    /// <summary>
    /// Mutes all game audio (called when sound-on button is clicked)
    /// </summary>
    public void MuteSound()
    {
        isSoundMuted = true;
        AudioListener.volume = 0f;
        UpdateButtonStates();
        Debug.Log("Sound muted");
    }
    
    /// <summary>
    /// Unmutes all game audio (called when sound-off button is clicked)
    /// </summary>
    public void UnmuteSound()
    {
        isSoundMuted = false;
        AudioListener.volume = originalVolume;
        UpdateButtonStates();
        Debug.Log("Sound unmuted");
    }
    
    /// <summary>
    /// Toggles between muted and unmuted state (alternative method for external calls)
    /// </summary>
    public void ToggleSound()
    {
        if (isSoundMuted)
        {
            UnmuteSound();
        }
        else
        {
            MuteSound();
        }
    }
    
    /// <summary>
    /// Updates button visibility based on current sound state
    /// </summary>
    private void UpdateButtonStates()
    {
        if (soundOnButton != null)
        {
            soundOnButton.SetActive(!isSoundMuted); // Show when sound is enabled (not muted)
        }
        
        if (soundOffButton != null)
        {
            soundOffButton.SetActive(isSoundMuted); // Show when sound is muted
        }
        
        Debug.Log($"Button states updated - SoundOn visible: {!isSoundMuted}, SoundOff visible: {isSoundMuted}");
    }
    
    /// <summary>
    /// Returns the current global sound mute state
    /// </summary>
    public bool IsSoundMuted()
    {
        return isSoundMuted;
    }
    
    /// <summary>
    /// Sets sound state directly (useful for loading saved preferences)
    /// </summary>
    public void SetSoundState(bool muted)
    {
        isSoundMuted = muted;
        
        // Apply the sound state immediately
        if (isSoundMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = originalVolume;
        }
        
        UpdateButtonStates();
        Debug.Log($"Sound state set to: {(muted ? "Muted" : "Unmuted")}");
    }
    
    // ==================== SOUND QUEUE MANAGEMENT ====================
    
    /// <summary>
    /// Requests to play a sound through the global sound manager
    /// Ensures only one sound plays at a time
    /// </summary>
    public void RequestPlaySound(System.Action playAction, float duration = 1f)
    {
        if (!enableSoundQueue || IsSoundMuted())
        {
            return; // Sound is disabled globally or queue is disabled
        }
        
        // Check if enough time has passed since last sound
        if (Time.time - lastSoundTime < soundCooldown)
        {
            // Queue the sound for later if cooldown hasn't passed
            soundQueue.Enqueue(() => InternalPlaySound(playAction, duration));
            
            // Start processing queue if not already running
            if (soundQueueCoroutine == null)
            {
                soundQueueCoroutine = StartCoroutine(ProcessSoundQueue());
            }
            return;
        }
        
        // Play immediately if no conflicts
        InternalPlaySound(playAction, duration);
    }
    
    /// <summary>
    /// Internal method to play sound with tracking
    /// </summary>
    private void InternalPlaySound(System.Action playAction, float duration)
    {
        if (IsSoundMuted()) return;
        
        isSoundPlaying = true;
        lastSoundTime = Time.time;
        
        try
        {
            playAction?.Invoke(); // Execute the sound playing action
            Debug.Log($"[SoundController] Sound played at {Time.time}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SoundController] Error playing sound: {e.Message}");
        }
        
        // Schedule sound completion
        StartCoroutine(MarkSoundComplete(duration));
    }
    
    /// <summary>
    /// Marks sound as complete after duration
    /// </summary>
    private IEnumerator MarkSoundComplete(float duration)
    {
        yield return new WaitForSeconds(duration);
        isSoundPlaying = false;
        Debug.Log($"[SoundController] Sound marked as complete at {Time.time}");
    }
    
    /// <summary>
    /// Processes queued sounds one by one
    /// </summary>
    private IEnumerator ProcessSoundQueue()
    {
        while (soundQueue.Count > 0)
        {
            // Wait for cooldown period
            yield return new WaitForSeconds(soundCooldown);
            
            // Skip if sound is muted
            if (IsSoundMuted())
            {
                soundQueue.Clear(); // Clear queue when muted
                break;
            }
            
            // Play next sound in queue
            if (soundQueue.Count > 0)
            {
                var nextSound = soundQueue.Dequeue();
                nextSound?.Invoke();
            }
        }
        
        soundQueueCoroutine = null; // Reset coroutine reference
        Debug.Log("[SoundController] Sound queue processing completed");
    }
    
    /// <summary>
    /// Checks if sound system is currently busy
    /// </summary>
    public bool IsSoundSystemBusy()
    {
        return isSoundPlaying || soundQueue.Count > 0;
    }
    
    /// <summary>
    /// Clears all pending sounds from queue
    /// </summary>
    public void ClearSoundQueue()
    {
        soundQueue.Clear();
        if (soundQueueCoroutine != null)
        {
            StopCoroutine(soundQueueCoroutine);
            soundQueueCoroutine = null;
        }
        Debug.Log("[SoundController] Sound queue cleared");
    }
    
    /// <summary>
    /// Sets the cooldown time between sounds
    /// </summary>
    public void SetSoundCooldown(float cooldown)
    {
        soundCooldown = Mathf.Max(0.1f, cooldown); // Minimum 0.1 seconds
        Debug.Log($"[SoundController] Sound cooldown set to {soundCooldown} seconds");
    }
    
    /// <summary>
    /// Enables or disables the sound queue system
    /// </summary>
    public void SetSoundQueueEnabled(bool enabled)
    {
        enableSoundQueue = enabled;
        if (!enabled)
        {
            ClearSoundQueue(); // Clear queue when disabling
        }
        Debug.Log($"[SoundController] Sound queue {(enabled ? "enabled" : "disabled")}");
    }
}
