using UnityEngine;
using System.Collections;

/// <summary>
/// Handles sound playback when QR code is scanned successfully
/// Provides audio feedback for successful QR code scanning events
/// </summary>
public class QrCodeScannedSound : MonoBehaviour
{
    [Header("QR Code Scan Sound Settings")]
    [SerializeField] private AudioSource audioSource; // Audio source for playing scan sound
    [SerializeField] private AudioClip qrScanSuccessClip; // Sound clip for successful QR scan
    [SerializeField] private AudioClip qrScanInvalidClip; // Sound clip for invalid QR scan
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float successVolume = 1.0f; // Volume for success sound
    [Range(0f, 1f)]
    [SerializeField] private float invalidVolume = 1.0f; // Volume for invalid sound
    
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
                Debug.Log("QrCodeScannedSound: AudioSource component created automatically");
            }
        }
        
        // Validate AudioSource
        if (audioSource == null)
        {
            Debug.LogWarning("QrCodeScannedSound: No AudioSource assigned - QR scan sounds will not play");
            return;
        }
        
        // Configure AudioSource settings
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // Validate audio clips
        if (qrScanSuccessClip == null)
        {
            Debug.LogWarning("QrCodeScannedSound: No success sound clip assigned");
        }
        
        if (qrScanInvalidClip == null)
        {
            Debug.LogWarning("QrCodeScannedSound: No invalid sound clip assigned");
        }
        
        Debug.Log("QrCodeScannedSound: Sound system initialized");
    }
    
    /// <summary>
    /// Play sound when QR code is scanned successfully
    /// Call this method when QR scanning succeeds
    /// </summary>
    public void PlayQRScanSuccessSound()
    {
        if (!soundEnabled || audioSource == null || qrScanSuccessClip == null)
        {
            return;
        }
        
        // Stop any currently playing sound
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Play success sound
        audioSource.clip = qrScanSuccessClip;
        audioSource.volume = successVolume;
        audioSource.Play();
        
        Debug.Log("QR Code scan success sound played");
    }
    
    /// <summary>
    /// Play a simple beep sound for QR scanning (if no specific clips assigned)
    /// Generates a procedural beep sound
    /// </summary>
    public void PlayQRScanBeep()
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
        audioSource.volume = successVolume;
        audioSource.pitch = 2.0f; // Higher pitch for beep
        audioSource.Play();
        
        yield return new WaitForSeconds(0.1f); // Short beep duration
        
        audioSource.Stop();
        audioSource.pitch = 1.0f; // Reset pitch
    }
    
    /// <summary>
    /// Play sound when QR code is invalid or target not found
    /// Call this method when QR scanning detects an invalid QR code
    /// </summary>
    public void PlayQRScanInvalidSound()
    {
        if (!soundEnabled || audioSource == null || qrScanInvalidClip == null)
        {
            return;
        }
        
        // Stop any currently playing sound
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Play invalid sound
        audioSource.clip = qrScanInvalidClip;
        audioSource.volume = invalidVolume;
        audioSource.Play();
        
        Debug.Log("QR Code scan invalid sound played");
    }
    
    /// <summary>
    /// Enable or disable QR scan sounds
    /// </summary>
    /// <param name="enabled">True to enable sounds, false to disable</param>
    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        Debug.Log($"QR Code scan sounds {(enabled ? "enabled" : "disabled")}");
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
    /// Set the volume for success sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetSuccessVolume(float volume)
    {
        successVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Set the volume for invalid sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetInvalidVolume(float volume)
    {
        invalidVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Stop any currently playing QR scan sound
    /// </summary>
    public void StopQRScanSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("QR Code scan sound stopped");
        }
    }
}