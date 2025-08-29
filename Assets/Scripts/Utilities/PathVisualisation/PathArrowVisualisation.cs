using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Displays a directional arrow that points toward the next navigation waypoint
/// </summary>
public class PathArrowVisualisation : MonoBehaviour {

    [SerializeField]
    private NavigationController navigationController; // Source of navigation path
    [SerializeField]
    private GameObject arrow; // Arrow object to display
    [SerializeField]
    private Slider navigationYOffset; // UI control for arrow height
    [SerializeField]
    private float moveOnDistance; // Distance to consider a waypoint "reached"

    [Header("Stabilization Settings")]
    [SerializeField]
    private float positionSmoothSpeed = 5f; // Arrow position smoothing speed
    [SerializeField]
    private float rotationSmoothSpeed = 8f; // Arrow rotation smoothing speed
    [SerializeField]
    private float updateInterval = 0.1f; // How often to recalculate target
    [SerializeField]
    private float minimumMovementThreshold = 0.02f; // Minimum movement to trigger update

    // Path calculation variables
    private NavMeshPath path;
    private float currentDistance;
    private Vector3[] pathOffset;
    private Vector3 nextNavigationPoint = Vector3.zero;
    
    // Smoothing and stabilization variables
    private Vector3 targetArrowPosition;
    private Quaternion targetArrowRotation;
    private Vector3 lastPlayerPosition;
    private float lastUpdateTime;
    private bool hasValidTarget = false;

    private void Update() {
        if (navigationController == null) return;
        
        // Get current navigation path
        path = navigationController.CalculatedPath;
        if (path == null || path.corners == null || path.corners.Length == 0) {
            // Hide arrow when no path available
            if (arrow != null) {
                arrow.SetActive(false);
            }
            hasValidTarget = false;
            return;
        }
        
        // Only recalculate when needed to reduce jitter
        bool shouldUpdate = Time.time - lastUpdateTime > updateInterval ||
                           Vector3.Distance(transform.position, lastPlayerPosition) > minimumMovementThreshold;
        
        if (shouldUpdate) {
            AddOffsetToPath(); // Copy path points for manipulation
            SelectNextNavigationPoint(); // Find next waypoint to point toward
            CalculateTargetArrowTransform(); // Calculate arrow position and rotation
            
            lastUpdateTime = Time.time;
            lastPlayerPosition = transform.position;
        }
        
        // Smoothly move arrow to target position/rotation
        SmoothArrowMovement();
    }

    /// <summary>
    /// Creates a copy of the navigation path for manipulation
    /// </summary>
    private void AddOffsetToPath() {
        if (path == null || path.corners == null) return;
        
        // Create a copy of path points for offset calculations
        pathOffset = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++) {
            pathOffset[i] = new Vector3(path.corners[i].x, path.corners[i].y, path.corners[i].z);
        }
    }

    /// <summary>
    /// Finds the next navigation point to point the arrow toward
    /// </summary>
    private void SelectNextNavigationPoint() {
        nextNavigationPoint = SelectNextNavigationPointWithinDistance();
    }

    /// <summary>
    /// Selects the optimal waypoint ahead of the player's current position
    /// </summary>
    private Vector3 SelectNextNavigationPointWithinDistance() {
        if (pathOffset == null || pathOffset.Length == 0) {
            return navigationController != null ? navigationController.TargetPosition : Vector3.zero;
        }
        
        Vector3 currentPos = transform.position;
        
        // Find the farthest waypoint we've passed
        int currentPathIndex = 0;
        
        for (int i = 0; i < pathOffset.Length; i++) {
            Vector3 pathPoint = pathOffset[i];
            float distance = Vector3.Distance(currentPos, pathPoint);
            
            // Mark waypoint as "reached" if within moveOnDistance
            if (distance <= moveOnDistance) {
                currentPathIndex = i;
            }
        }
        
        // Find next suitable waypoint ahead in sequence
        // Start from the next point but look further ahead if needed
        for (int i = currentPathIndex + 1; i < pathOffset.Length; i++) {
            Vector3 pathPoint = pathOffset[i];
            float distance = Vector3.Distance(currentPos, pathPoint);
            
            // Check for good distance or stairs (height difference)
            float heightDifference = Mathf.Abs(pathPoint.y - currentPos.y);
            bool isGoodDistance = distance > moveOnDistance * 0.8f; // Slightly more lenient
            bool isStairPoint = heightDifference > 0.4f; // Height difference for stairs
            
            if (isGoodDistance || isStairPoint) {
                currentDistance = distance;
                Debug.Log($"Arrow pointing to point {i}: distance={distance:F2}, height diff={heightDifference:F2}, isStair={isStairPoint}");
                return pathPoint;
            }
        }
        
        // Default to final target if no good intermediate point found
        Debug.Log("No suitable intermediate point found, pointing to final target");
        return navigationController != null ? navigationController.TargetPosition : Vector3.zero;
    }

    /// <summary>
    /// Calculates where the arrow should be positioned and what direction it should point
    /// </summary>
    private void CalculateTargetArrowTransform() {
        if (arrow == null || nextNavigationPoint == Vector3.zero) {
            hasValidTarget = false;
            return;
        }
        
        // Position arrow in front of the camera/player so it's visible
        Vector3 arrowPosition = transform.position + transform.forward * 1f; // 1 meter in front

        // Apply Y offset if specified, otherwise keep same Y as camera
        if (navigationYOffset != null && navigationYOffset.value != 0) {
            arrowPosition.y = navigationYOffset.value;
        }
        
        // Multiple distance checks to prevent pointing at player
        float distanceFromPlayer = Vector3.Distance(transform.position, nextNavigationPoint);
        float distanceFromArrow = Vector3.Distance(arrowPosition, nextNavigationPoint);
        
        // Hide arrow if target is too close to either player or arrow position
        if (distanceFromPlayer < 0.5f || distanceFromArrow < 0.3f) {
            hasValidTarget = false;
            Debug.Log($"Arrow hidden - target too close (player: {distanceFromPlayer:F2}m, arrow: {distanceFromArrow:F2}m)");
            return;
        }
        
        // Hide arrow if target is behind the player
        Vector3 directionToTarget = (nextNavigationPoint - transform.position).normalized;
        Vector3 playerForward = transform.forward;
        float dotProduct = Vector3.Dot(playerForward, directionToTarget);
        
        // If target is significantly behind us, hide arrow
        if (dotProduct < -0.3f) {
            hasValidTarget = false;
            Debug.Log($"Arrow hidden - target is behind player (dot: {dotProduct:F2})");
            return;
        }
        
        // Calculate target transform
        targetArrowPosition = arrowPosition;
        
        // Calculate target rotation to point toward waypoint
        Vector3 direction = nextNavigationPoint - arrowPosition;
        if (direction != Vector3.zero) {
            targetArrowRotation = Quaternion.LookRotation(direction);
            hasValidTarget = true;
            Debug.Log($"Arrow target calculated - pointing from {arrowPosition} to {nextNavigationPoint}, distance: {distanceFromArrow:F2}");
        } else {
            hasValidTarget = false;
        }
    }
    
    /// <summary>
    /// Smoothly moves and rotates the arrow to its target position
    /// </summary>
    private void SmoothArrowMovement() {
        if (arrow == null) return;
        
        // Hide arrow if no valid target
        if (!hasValidTarget) {
            arrow.SetActive(false);
            return;
        }
        
        // Show arrow and set initial position when becoming active
        if (!arrow.activeInHierarchy) {
            arrow.SetActive(true);
            // Initialize smooth values when arrow becomes active
            arrow.transform.position = targetArrowPosition;
            arrow.transform.rotation = targetArrowRotation;
            return;
        }
        
        // Smoothly interpolate position and rotation over time
        arrow.transform.position = Vector3.Lerp(
            arrow.transform.position, 
            targetArrowPosition, 
            positionSmoothSpeed * Time.deltaTime
        );
        
        arrow.transform.rotation = Quaternion.Slerp(
            arrow.transform.rotation, 
            targetArrowRotation, 
            rotationSmoothSpeed * Time.deltaTime
        );
    }
}