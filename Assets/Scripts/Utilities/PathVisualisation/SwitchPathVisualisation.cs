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

    private void Start() {
        // Initialize with line visualization as default
        activeVisualisation = pathLineVis.gameObject;
        activeDistanceLabel = distanceLabel.gameObject;
    }

    /// <summary>
    /// Switches to the next available path visualization mode
    /// </summary>
    public void NextLineVisualisation() {
        visualisationCounter++;

        DisableAllPathVisuals(); // Turn off all visualizations
        EnablePathVisualsByIndex(visualisationCounter); // Enable the selected one
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
        activeVisualisation.SetActive(!activeVisualisation.activeSelf);
        activeDistanceLabel.SetActive(!activeDistanceLabel.activeSelf);
    }
}