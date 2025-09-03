using UnityEngine;
using System.Collections;

/// <summary>
/// Handles sound playback when navigation system reroutes to a closer target
/// Provides audio feedback for dynamic rerouting events
/// </summary>
public class RerouteSound : MonoBehaviour
{
    [Header("Reroute Sound Settings")]
    [SerializeField] private AudioSource audioSource; // Audio source for playing reroute sound
    [SerializeField] private AudioClip rerouteClip; // Sound clip for rerouting notification
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float rerouteVolume = 1.0f; // Volume for reroute sound
    
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
                Debug.Log("RerouteSound: AudioSource component created automatically");
            }
        }
        
        // Validate AudioSource
        if (audioSource == null)
        {
            Debug.LogWarning("RerouteSound: No AudioSource assigned - Reroute sounds will not play");
            return;
        }
        
        // Configure AudioSource settings
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // Validate audio clips
        if (rerouteClip == null)
        {
            Debug.LogWarning("RerouteSound: No reroute sound clip assigned");
        }
        
        Debug.Log("RerouteSound: Sound system initialized");
    }
    
    /// <summary>
    /// Play sound when navigation reroutes to a closer target
    /// Call this method when rerouting occurs
    /// </summary>
    public void PlayRerouteSound()
    {
        if (!soundEnabled || audioSource == null || rerouteClip == null)
        {
            return;
        }
        
        // Stop any currently playing sound
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Play reroute sound
        audioSource.clip = rerouteClip;
        audioSource.volume = rerouteVolume;
        audioSource.Play();
        
        Debug.Log("Reroute sound played");
    }
    
    /// <summary>
    /// Play a simple beep sound for rerouting (if no specific clips assigned)
    /// Generates a procedural beep sound
    /// </summary>
    public void PlayRerouteBeep()
    {
        if (!soundEnabled || audioSource == null)
        {
            return;
        }
        
        // Simple beep using AudioSource without clip (generates tone)
        StartCoroutine(PlayBeepCoroutine());
    }
    
    /// <summary>
    /// Coroutine to play a simple beep sound
    /// </summary>
    private IEnumerator PlayBeepCoroutine()
    {
        // Create a simple beep by playing a short tone
        audioSource.volume = rerouteVolume;
        audioSource.pitch = 1.5f; // Medium pitch for reroute beep
        audioSource.Play();
        
        yield return new WaitForSeconds(0.15f); // Short beep duration
        
        audioSource.Stop();
        audioSource.pitch = 1.0f; // Reset pitch
    }
    
    /// <summary>
    /// Enable or disable reroute sounds
    /// </summary>
    /// <param name="enabled">True to enable sounds, false to disable</param>
    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        Debug.Log($"Reroute sounds {(enabled ? "enabled" : "disabled")}");
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
    /// Set the volume for reroute sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetRerouteVolume(float volume)
    {
        rerouteVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Stop any currently playing reroute sound
    /// </summary>
    public void StopRerouteSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Reroute sound stopped");
        }
    }
}
