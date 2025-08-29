using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Displays a line that shows the navigation path from player to target
/// </summary>
public class PathLineVisualisation : MonoBehaviour {

    // Component references
    [SerializeField]
    private NavigationController navigationController; // Source of navigation path
    [SerializeField]
    private LineRenderer line; // Line renderer to display the path
    [SerializeField]
    private Slider navigationYOffset; // Y position adjustment for line height

    // Path calculation variables
    private NavMeshPath path; // Current navigation path
    private Vector3[] calculatedPathAndOffset; // Path points with applied offsets

    private void Update() {
        if (navigationController == null) return;
        
        // Get current navigation path
        path = navigationController.CalculatedPath;
        if (path == null || path.corners == null || path.corners.Length == 0) {
            // Hide line renderer when no path available
            if (line != null) {
                line.positionCount = 0;
            }
            return;
        }
        
        // Process and display the path
        AddOffsetToPath(); // Copy path points for manipulation
        AddLineOffset(); // Apply Y height offset if needed
        SetLineRendererPositions(); // Update line renderer with new positions
    }

    /// <summary>
    /// Creates a copy of the navigation path for manipulation
    /// </summary>
    private void AddOffsetToPath() {
        calculatedPathAndOffset = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++) {
            calculatedPathAndOffset[i] = new Vector3(path.corners[i].x, path.corners[i].y, path.corners[i].z);
        }
    }

    /// <summary>
    /// Applies Y-axis height offset to all path points if specified
    /// </summary>
    private void AddLineOffset() {
        if (navigationYOffset.value != 0) {
            for (int i = 0; i < calculatedPathAndOffset.Length; i++) {
                calculatedPathAndOffset[i] += new Vector3(0, navigationYOffset.value, 0);
            }
        }
    }

    /// <summary>
    /// Updates the line renderer with the calculated path positions
    /// </summary>
    private void SetLineRendererPositions() {
        line.positionCount = calculatedPathAndOffset.Length;
        line.SetPositions(calculatedPathAndOffset);
    }
}