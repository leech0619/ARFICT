using UnityEngine;
using TMPro;

/// <summary>
/// Allows switching between different path visualization modes (line vs arrow)
/// </summary>
public class SwitchPathVisualisation : MonoBehaviour {

    // Path visualization components
    [SerializeField]
    private PathLineVisualisation pathLineVis; // Line-based path display
    [SerializeField]
    private PathArrowVisualisation arrowLineVis; // Arrow-based path display
    [SerializeField]
    private TextMeshProUGUI distanceLabel; // Distance text display

    // State tracking variables
    private int visualisationCounter = 0; // Current visualization mode index
    private GameObject activeVisualisation; // Currently active visualization object
    private GameObject activeDistanceLabel; // Distance label reference
    
    // Action feedback
    private ActionLabel actionLabel; // Reference to action label for mode change messages
    
    // Sound feedback
    private ToggleNavigationSound toggleNavigationSound; // Reference to toggle navigation sound controller
    private NavigationModeSound navigationModeSound; // Reference to navigation mode sound controller

    private void Start() {
        // Initialize with line visualization as default
        activeVisualisation = pathLineVis.gameObject;
        activeDistanceLabel = distanceLabel.gameObject;
        
        // Find ActionLabel component automatically
        actionLabel = FindObjectOfType<ActionLabel>();
        
        if (actionLabel == null)
        {
            Debug.LogWarning("ActionLabel not found - Navigation mode change messages will not be displayed");
        }
        
        // Find ToggleNavigationSound component automatically
        toggleNavigationSound = FindObjectOfType<ToggleNavigationSound>();
        
        if (toggleNavigationSound == null)
        {
            Debug.LogWarning("ToggleNavigationSound not found - Navigation toggle sounds will not play");
        }
        
        // Find NavigationModeSound component automatically
        navigationModeSound = FindObjectOfType<NavigationModeSound>();
        
        if (navigationModeSound == null)
        {
            Debug.LogWarning("NavigationModeSound not found - Navigation mode switch sounds will not play");
        }
    }

    /// <summary>
    /// Switches to the next available path visualization mode
    /// </summary>
    public void NextLineVisualisation() {
        visualisationCounter++;

        DisableAllPathVisuals(); // Turn off all visualizations
        EnablePathVisualsByIndex(visualisationCounter); // Enable the selected one
        
        // Find ActionLabel if not already found (fixes timing issue)
        if (actionLabel == null)
        {
            actionLabel = FindObjectOfType<ActionLabel>();
        }
        
        // Show mode change message via ActionLabel
        if (actionLabel != null)
        {
            bool isArrowMode = (visualisationCounter == 1);
            actionLabel.ShowNavigationModeChange(isArrowMode);
        }
        else
        {
            Debug.LogWarning("ActionLabel still not found when trying to show mode change message");
        }
        
        // Find NavigationModeSound if not already found (fixes timing issue)
        if (navigationModeSound == null)
        {
            navigationModeSound = FindObjectOfType<NavigationModeSound>();
        }
        
        // Play appropriate mode sound
        if (navigationModeSound != null)
        {
            bool isArrowMode = (visualisationCounter == 1);
            navigationModeSound.PlayModeSound(isArrowMode);
        }
        else
        {
            Debug.LogWarning("NavigationModeSound still not found when trying to play mode sound");
        }
    }

    /// <summary>
    /// Turns off all path visualization components
    /// </summary>
    private void DisableAllPathVisuals() {
        pathLineVis.gameObject.SetActive(false);
        arrowLineVis.gameObject.SetActive(false);
    }

    /// <summary>
    /// Enables a specific visualization based on the index
    /// </summary>
    private void EnablePathVisualsByIndex(int visIndex) {
        switch (visIndex) {
            case 1:
                // Switch to arrow visualization
                activeVisualisation = arrowLineVis.gameObject;
                break;
            default:
                // Default to line visualization and reset counter
                activeVisualisation = pathLineVis.gameObject;
                visualisationCounter = 0;
                break;
        }

        activeVisualisation.SetActive(true);
    }

    /// <summary>
    /// Toggles the visibility of the current visualization and distance label
    /// </summary>
    public void ToggleVisualVisibility()
    {
        bool isBecomingActive = !activeVisualisation.activeSelf;
        
        activeVisualisation.SetActive(isBecomingActive);
        activeDistanceLabel.SetActive(isBecomingActive);
        
        // Find ActionLabel if not already found (fixes timing issue)
        if (actionLabel == null)
        {
            actionLabel = FindObjectOfType<ActionLabel>();
        }
        
        // Show navigation toggle message via ActionLabel
        if (actionLabel != null)
        {
            actionLabel.ShowNavigationToggle(isBecomingActive);
        }
        else
        {
            Debug.LogWarning("ActionLabel still not found when trying to show navigation toggle message");
        }
        
        // Find ToggleNavigationSound if not already found (fixes timing issue)
        if (toggleNavigationSound == null)
        {
            toggleNavigationSound = FindObjectOfType<ToggleNavigationSound>();
        }
        
        // Play appropriate toggle sound
        if (toggleNavigationSound != null)
        {
            toggleNavigationSound.PlayToggleSound(isBecomingActive);
        }
        else
        {
            Debug.LogWarning("ToggleNavigationSound still not found when trying to play toggle sound");
        }
    }
    
    /// <summary>
    /// Returns whether navigation visualization is currently active
    /// </summary>
    public bool IsNavigationActive()
    {
        return activeVisualisation != null && activeVisualisation.activeSelf;
    }
    
    /// <summary>
    /// Returns whether currently in arrow mode (true) or line mode (false)
    /// </summary>
    public bool IsArrowMode()
    {
        return visualisationCounter == 1;
    }
    
    /// <summary>
    /// Manually set the ActionLabel reference if automatic finding doesn't work
    /// </summary>
    public void SetActionLabel(ActionLabel label)
    {
        actionLabel = label;
        Debug.Log("SwitchPathVisualisation ActionLabel reference manually set");
    }
}