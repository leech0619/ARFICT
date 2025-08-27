using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    [Header("Sound Button References")]
    public GameObject soundOnButton;  // Button shown when sound is ON (not muted)
    public GameObject soundOffButton; // Button shown when sound is OFF (muted)
    
    [Header("Sound Settings")]
    public bool isSoundMuted = false; // Current sound state
    
    // Store original volume to restore when unmuting
    private float originalVolume = 1f;
    
    void Start()
    {
        // Store the original volume
        originalVolume = AudioListener.volume;
        
        // Initialize button states based on current sound state
        UpdateButtonStates();
        
        // Add button click listeners
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
    /// Mute the sound (called when SoundOnButton is clicked)
    /// </summary>
    public void MuteSound()
    {
        isSoundMuted = true;
        AudioListener.volume = 0f;
        UpdateButtonStates();
        Debug.Log("Sound muted");
    }
    
    /// <summary>
    /// Unmute the sound (called when SoundOffButton is clicked)
    /// </summary>
    public void UnmuteSound()
    {
        isSoundMuted = false;
        AudioListener.volume = originalVolume;
        UpdateButtonStates();
        Debug.Log("Sound unmuted");
    }
    
    /// <summary>
    /// Toggle sound state (alternative method for external calls)
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
    /// Update button visibility based on current sound state
    /// </summary>
    private void UpdateButtonStates()
    {
        if (soundOnButton != null)
        {
            soundOnButton.SetActive(!isSoundMuted); // Show when sound is ON (not muted)
        }
        
        if (soundOffButton != null)
        {
            soundOffButton.SetActive(isSoundMuted); // Show when sound is OFF (muted)
        }
        
        Debug.Log($"Button states updated - SoundOn visible: {!isSoundMuted}, SoundOff visible: {isSoundMuted}");
    }
    
    /// <summary>
    /// Get current sound state
    /// </summary>
    public bool IsSoundMuted()
    {
        return isSoundMuted;
    }
    
    /// <summary>
    /// Set sound state directly (useful for loading saved preferences)
    /// </summary>
    public void SetSoundState(bool muted)
    {
        isSoundMuted = muted;
        
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
