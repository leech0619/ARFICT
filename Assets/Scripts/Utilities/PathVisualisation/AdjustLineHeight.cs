using UnityEngine;
using TMPro;

/// <summary>
/// Simple UI toggle utility for showing/hiding objects
/// Now with ActionLabel integration for slider status feedback
/// </summary>
public class AdjustLineHeight : MonoBehaviour
{   
    [SerializeField]
    private GameObject toggleObject; // Object to show/hide
    
    // Action feedback
    private ActionLabel actionLabel; // Reference to action label for slider status messages
    
    // Sound feedback
    private ToggleLineAdjustSound toggleLineAdjustSound; // Reference to line adjust toggle sound controller
    
    private void Start()
    {
        // Find ActionLabel component automatically
        actionLabel = FindObjectOfType<ActionLabel>();
        
        if (actionLabel == null)
        {
            Debug.LogWarning("ActionLabel not found - Line height slider status messages will not be displayed");
        }
        
        // Find ToggleLineAdjustSound component automatically
        toggleLineAdjustSound = FindObjectOfType<ToggleLineAdjustSound>();
        
        if (toggleLineAdjustSound == null)
        {
            Debug.LogWarning("ToggleLineAdjustSound not found - Line height slider toggle sounds will not play");
        }
    }

    // Toggle the visibility of the assigned object
    public void Toggle() {
        bool isBecomingActive = !toggleObject.activeSelf;
        toggleObject.SetActive(isBecomingActive);
        
        // Find ActionLabel if not already found (fixes timing issue)
        if (actionLabel == null)
        {
            actionLabel = FindObjectOfType<ActionLabel>();
        }
        
        // Show slider status message via ActionLabel
        if (actionLabel != null)
        {
            actionLabel.ShowLineHeightSliderToggle(isBecomingActive);
        }
        else
        {
            Debug.LogWarning("ActionLabel still not found when trying to show slider message");
        }
        
        // Find ToggleLineAdjustSound if not already found (fixes timing issue)
        if (toggleLineAdjustSound == null)
        {
            toggleLineAdjustSound = FindObjectOfType<ToggleLineAdjustSound>();
        }
        
        // Play appropriate toggle sound
        if (toggleLineAdjustSound != null)
        {
            toggleLineAdjustSound.PlayToggleSound(isBecomingActive);
        }
        else
        {
            Debug.LogWarning("ToggleLineAdjustSound still not found when trying to play toggle sound");
        }
    }
    
    /// <summary>
    /// Returns whether the line height slider is currently active
    /// </summary>
    public bool IsSliderActive()
    {
        return toggleObject != null && toggleObject.activeSelf;
    }
    
    /// <summary>
    /// Manually set the ActionLabel reference if automatic finding doesn't work
    /// </summary>
    public void SetActionLabel(ActionLabel label)
    {
        actionLabel = label;
        Debug.Log("AdjustLineHeight ActionLabel reference manually set");
    }
}
