using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles navigation path calculation using Unity's NavMesh system
/// Tracks AR camera position and calculates optimal routes to targets
/// </summary>
public class NavigationController : MonoBehaviour {

    // Current navigation target position in world space
    public Vector3 TargetPosition { get; set; } = Vector3.zero;

    // The calculated navigation path from current position to target
    public NavMeshPath CalculatedPath { get; private set; }
    
    [SerializeField]
    private Camera arCamera; // Reference to AR camera for position tracking

    private void Start() {
        // Initialize path calculation system
        CalculatedPath = new NavMeshPath();
        
        // Auto-assign main camera if not set in inspector
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    private void Update() {
        // Keep navigation controller at camera position for accurate pathfinding
        if (arCamera != null)
        {
            transform.position = arCamera.transform.position;
        }
        
        // Calculate path to target if one is set
        if (TargetPosition != Vector3.zero) {
            NavMesh.CalculatePath(transform.position, TargetPosition, NavMesh.AllAreas, CalculatedPath);
        } else {
            // Clear the calculated path when no target is set
            ClearCalculatedPath();
        }
    }
    
    /// <summary>
    /// Completely clears the current navigation path
    /// Used when navigation is cancelled or completed
    /// </summary>
    public void ClearCalculatedPath() {
        if (CalculatedPath != null) {
            CalculatedPath.ClearCorners();
        }
    }
}