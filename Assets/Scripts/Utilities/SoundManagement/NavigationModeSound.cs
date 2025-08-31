using UnityEngine;

/// <summary>
/// Handles sound playback for navigation mode switching (line/arrow)
/// Provides audio feedback when switching between line and arrow navigation modes
/// </summary>
public class NavigationModeSound : MonoBehaviour
{
    [Header("Navigation Mode Sound Settings")]
    [SerializeField] private AudioSource audioSource; // Audio source for playing mode sounds
    [SerializeField] private AudioClip lineModeSound; // Sound clip for when switching to LINE mode
    [SerializeField] private AudioClip arrowModeSound; // Sound clip for when switching to ARROW mode
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float lineModeVolume = 1.0f; // Volume for line mode sound
    [Range(0f, 1f)]
    [SerializeField] private float arrowModeVolume = 1.0f; // Volume for arrow mode sound

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
                Debug.Log("NavigationModeSound: AudioSource component created automatically");
            }
        }
        
        // Validate AudioSource
        if (audioSource == null)
        {
            Debug.LogWarning("NavigationModeSound: No AudioSource assigned - Navigation mode sounds will not play");
            return;
        }
        
        // Configure AudioSource settings
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // Validate audio clips
        if (lineModeSound == null)
        {
            Debug.LogWarning("NavigationModeSound: No line mode sound clip assigned");
        }
        
        if (arrowModeSound == null)
        {
            Debug.LogWarning("NavigationModeSound: No arrow mode sound clip assigned");
        }
        
        Debug.Log("NavigationModeSound: Sound system initialized");
    }
    
    /// <summary>
    /// Play sound when switching to LINE navigation mode
    /// Call this method when navigation mode changes to line visualization
    /// </summary>
    public void PlayLineModeSound()
    {
        if (!soundEnabled || audioSource == null || lineModeSound == null)
        {
            return;
        }
        
        // Use sound queue system for coordinated playback
        if (SoundController.Instance != null)
        {
            SoundController.Instance.RequestPlaySound(() => {
                if (audioSource != null && lineModeSound != null)
                {
                    // Stop any currently playing sound
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                    
                    // Play line mode sound
                    audioSource.clip = lineModeSound;
                    audioSource.volume = lineModeVolume;
                    audioSource.Play();
                    
                    Debug.Log("Line mode sound played through queue system");
                }
            }, lineModeSound.length);
        }
        else
        {
            // Fallback if SoundController not available
            Debug.LogWarning("SoundController not found, playing line mode sound directly");
            
            // Stop any currently playing sound
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Play line mode sound
            audioSource.clip = lineModeSound;
            audioSource.volume = lineModeVolume;
            audioSource.Play();
            
            Debug.Log("Line mode sound played (fallback)");
        }
    }
    
    /// <summary>
    /// Play sound when switching to ARROW navigation mode
    /// Call this method when navigation mode changes to arrow visualization
    /// </summary>
    public void PlayArrowModeSound()
    {
        if (!soundEnabled || audioSource == null || arrowModeSound == null)
        {
            return;
        }
        
        // Use sound queue system for coordinated playback
        if (SoundController.Instance != null)
        {
            SoundController.Instance.RequestPlaySound(() => {
                if (audioSource != null && arrowModeSound != null)
                {
                    // Stop any currently playing sound
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                    
                    // Play arrow mode sound
                    audioSource.clip = arrowModeSound;
                    audioSource.volume = arrowModeVolume;
                    audioSource.Play();
                    
                    Debug.Log("Arrow mode sound played through queue system");
                }
            }, arrowModeSound.length);
        }
        else
        {
            // Fallback if SoundController not available
            Debug.LogWarning("SoundController not found, playing arrow mode sound directly");
            
            // Stop any currently playing sound
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Play arrow mode sound
            audioSource.clip = arrowModeSound;
            audioSource.volume = arrowModeVolume;
            audioSource.Play();
            
            Debug.Log("Arrow mode sound played (fallback)");
        }
    }
    
    /// <summary>
    /// Play appropriate sound based on navigation mode
    /// </summary>
    /// <param name="isArrowMode">True if switching to arrow mode, false if switching to line mode</param>
    public void PlayModeSound(bool isArrowMode)
    {
        if (isArrowMode)
        {
            PlayArrowModeSound();
        }
        else
        {
            PlayLineModeSound();
        }
    }
    
    /// <summary>
    /// Enable or disable navigation mode sounds
    /// </summary>
    /// <param name="enabled">True to enable sounds, false to disable</param>
    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        Debug.Log($"Navigation mode sounds {(enabled ? "enabled" : "disabled")}");
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
    /// Set the volume for line mode sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetLineModeVolume(float volume)
    {
        lineModeVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Set the volume for arrow mode sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetArrowModeVolume(float volume)
    {
        arrowModeVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Stop any currently playing navigation mode sound
    /// </summary>
    public void StopModeSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Navigation mode sound stopped");
        }
    }
}