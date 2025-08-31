using UnityEngine;

/// <summary>
/// Handles sound playback for line height adjustment toggle functionality
/// Provides audio feedback when line height slider is turned on/off
/// </summary>
public class ToggleLineAdjustSound : MonoBehaviour
{
    [Header("Line Adjust Toggle Sound Settings")]
    [SerializeField] private AudioSource audioSource; // Audio source for playing toggle sounds
    [SerializeField] private AudioClip sliderOnClip; // Sound clip for when slider is turned ON
    [SerializeField] private AudioClip sliderOffClip; // Sound clip for when slider is turned OFF
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float onVolume = 1.0f; // Volume for slider ON sound
    [Range(0f, 1f)]
    [SerializeField] private float offVolume = 1.0f; // Volume for slider OFF sound
    
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
                Debug.Log("ToggleLineAdjustSound: AudioSource component created automatically");
            }
        }
        
        // Validate AudioSource
        if (audioSource == null)
        {
            Debug.LogWarning("ToggleLineAdjustSound: No AudioSource assigned - Line height slider sounds will not play");
            return;
        }
        
        // Configure AudioSource settings
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // Validate audio clips
        if (sliderOnClip == null)
        {
            Debug.LogWarning("ToggleLineAdjustSound: No slider ON sound clip assigned");
        }
        
        if (sliderOffClip == null)
        {
            Debug.LogWarning("ToggleLineAdjustSound: No slider OFF sound clip assigned");
        }
        
        Debug.Log("ToggleLineAdjustSound: Sound system initialized");
    }
    
    /// <summary>
    /// Play sound when line height slider is turned ON
    /// Call this method when slider becomes active/visible
    /// </summary>
    public void PlaySliderOnSound()
    {
        if (!soundEnabled || audioSource == null || sliderOnClip == null)
        {
            return;
        }
        
        // Use sound queue system for coordinated playback
        if (SoundController.Instance != null)
        {
            SoundController.Instance.RequestPlaySound(() => {
                if (audioSource != null && sliderOnClip != null)
                {
                    // Stop any currently playing sound
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                    
                    // Play slider ON sound
                    audioSource.clip = sliderOnClip;
                    audioSource.volume = onVolume;
                    audioSource.Play();
                    
                    Debug.Log("Line height slider ON sound played through queue system");
                }
            }, sliderOnClip.length);
        }
        else
        {
            // Fallback if SoundController not available
            Debug.LogWarning("SoundController not found, playing slider ON sound directly");
            
            // Stop any currently playing sound
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Play slider ON sound
            audioSource.clip = sliderOnClip;
            audioSource.volume = onVolume;
            audioSource.Play();
            
            Debug.Log("Line height slider ON sound played (fallback)");
        }
    }
    
    /// <summary>
    /// Play sound when line height slider is turned OFF
    /// Call this method when slider becomes inactive/hidden
    /// </summary>
    public void PlaySliderOffSound()
    {
        if (!soundEnabled || audioSource == null || sliderOffClip == null)
        {
            return;
        }
        
        // Use sound queue system for coordinated playback
        if (SoundController.Instance != null)
        {
            SoundController.Instance.RequestPlaySound(() => {
                if (audioSource != null && sliderOffClip != null)
                {
                    // Stop any currently playing sound
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                    
                    // Play slider OFF sound
                    audioSource.clip = sliderOffClip;
                    audioSource.volume = offVolume;
                    audioSource.Play();
                    
                    Debug.Log("Line height slider OFF sound played through queue system");
                }
            }, sliderOffClip.length);
        }
        else
        {
            // Fallback if SoundController not available
            Debug.LogWarning("SoundController not found, playing slider OFF sound directly");
            
            // Stop any currently playing sound
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Play slider OFF sound
            audioSource.clip = sliderOffClip;
            audioSource.volume = offVolume;
            audioSource.Play();
            
            Debug.Log("Line height slider OFF sound played (fallback)");
        }
    }
    
    /// <summary>
    /// Play appropriate sound based on slider state
    /// </summary>
    /// <param name="isSliderOn">True if slider is turning ON, false if turning OFF</param>
    public void PlayToggleSound(bool isSliderOn)
    {
        if (isSliderOn)
        {
            PlaySliderOnSound();
        }
        else
        {
            PlaySliderOffSound();
        }
    }
    
    /// <summary>
    /// Enable or disable line height slider toggle sounds
    /// </summary>
    /// <param name="enabled">True to enable sounds, false to disable</param>
    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        Debug.Log($"Line height slider toggle sounds {(enabled ? "enabled" : "disabled")}");
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
    /// Set the volume for slider ON sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetOnVolume(float volume)
    {
        onVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Set the volume for slider OFF sounds
    /// </summary>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetOffVolume(float volume)
    {
        offVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Stop any currently playing line height slider toggle sound
    /// </summary>
    public void StopToggleSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Line height slider toggle sound stopped");
        }
    }
}