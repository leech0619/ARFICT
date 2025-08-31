using UnityEngine;

/// <summary>
/// Handles sound playback for navigation toggle functionality
/// Provides audio feedback when navigation visibility is turned on/off
/// </summary>
public class ToggleNavigationSound : MonoBehaviour
{
    [Header("Navigation Toggle Sound Settings")]
    [SerializeField] private AudioSource audioSource; // Audio source for playing toggle sounds
    [SerializeField] private AudioClip navigationOnClip; // Sound clip for when navigation is turned ON
    [SerializeField] private AudioClip navigationOffClip; // Sound clip for when navigation is turned OFF
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float onVolume = 1.0f; // Volume for navigation ON sound
    [Range(0f, 1f)]
    [SerializeField] private float offVolume = 1.0f; // Volume for navigation OFF sound
    
    [Header("Auto Setup")]
    [SerializeField] private bool findAudioSourceAutomatically = true; // Auto-find AudioSource if not assigned
    
    // Sound state tracking
    private bool soundEnabled = true;
    
    private void Start()
    {
        InitializeSoundSystem();
    }
    
    /// <summary>
    /// Initialize the sound system and validate components
    /// </summary>
    private void InitializeSoundSystem()
    {
        // Auto-find AudioSource if not assigned
        if (audioSource == null && findAudioSourceAutomatically)
        {
            audioSource = GetComponent<AudioSource>();
            
            // If still not found, create one
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("ToggleNavigationSound: AudioSource component created automatically");
            }
        }
        
        // Validate AudioSource
        if (audioSource == null)
        {
            Debug.LogWarning("ToggleNavigationSound: No AudioSource assigned - Navigation toggle sounds will not play");
            return;
        }
        
        // Configure AudioSource settings
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // Validate audio clips
        if (navigationOnClip == null)
        {
            Debug.LogWarning("ToggleNavigationSound: No navigation ON sound clip assigned");
        }
        
        if (navigationOffClip == null)
        {
            Debug.LogWarning("ToggleNavigationSound: No navigation OFF sound clip assigned");
        }
        
        Debug.Log("ToggleNavigationSound: Sound system initialized");
    }
    
    /// <summary>
    /// Play sound when navigation is turned ON
    /// Call this method when navigation visibility becomes active
    /// </summary>
    public void PlayNavigationOnSound()
    {
        if (!soundEnabled || audioSource == null || navigationOnClip == null)
        {
            return;
        }
        
        // Stop any currently playing sound
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Play navigation ON sound
        audioSource.clip = navigationOnClip;
        audioSource.volume = onVolume;
        audioSource.Play();
        
        Debug.Log("Navigation ON sound played");
    }
    
    /// <summary>
    /// Play sound when navigation is turned OFF
    /// Call this method when navigation visibility becomes inactive
    /// </summary>
    public void PlayNavigationOffSound()
    {
        if (!soundEnabled || audioSource == null || navigationOffClip == null)
        {
            return;
        }
        
        // Stop any currently playing sound
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Play navigation OFF sound
        audioSource.clip = navigationOffClip;
        audioSource.volume = offVolume;
        audioSource.Play();
        
        Debug.Log("Navigation OFF sound played");
    }
    
    /// <summary>
    /// Play appropriate sound based on navigation state
    /// </summary>
    /// <param name="isNavigationOn">True if navigation is turning ON, false if turning OFF</param>
    public void PlayToggleSound(bool isNavigationOn)
    {
        if (isNavigationOn)
        {
            PlayNavigationOnSound();
        }
        else
        {
            PlayNavigationOffSound();
        }
    }
    
    /// <summary>
    /// Enable or disable navigation toggle sounds
    /// </summary>
    /// <param name="enabled">True to enable sounds, false to disable</param>
    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        Debug.Log($"Navigation toggle sounds {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Check if sounds are currently enabled
    /// </summary>
    /// <returns>True if sounds are enabled</returns>
    public bool IsSoundEnabled()
    {
        return soundEnabled;
    }
    
    /// <summary>
    /// Set the volume for navigation ON sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetOnVolume(float volume)
    {
        onVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Set the volume for navigation OFF sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetOffVolume(float volume)
    {
        offVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Stop any currently playing navigation toggle sound
    /// </summary>
    public void StopToggleSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Navigation toggle sound stopped");
        }
    }
}