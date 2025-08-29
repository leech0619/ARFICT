using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls global sound muting/unmuting with UI button management
/// </summary>
public class SoundController : MonoBehaviour
{
    [Header("Sound Button References")]
    public GameObject soundOnButton;  // Button displayed when sound is enabled (clickable to mute)
    public GameObject soundOffButton; // Button displayed when sound is muted (clickable to unmute)
    
    [Header("Sound Settings")]
    public bool isSoundMuted = false; // Current global sound state
    
    // Volume management
    private float originalVolume = 1f; // Stores original volume level for restoration when unmuting
    
    void Start()
    {
        // Store the original volume level for restoration
        originalVolume = AudioListener.volume;
        
        // Set initial button visibility based on current sound state
        UpdateButtonStates();
        
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
}
