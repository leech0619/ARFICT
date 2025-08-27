using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PathArrowVisualisation : MonoBehaviour {

    [SerializeField]
    private NavigationController navigationController;
    [SerializeField]
    private GameObject arrow;
    [SerializeField]
    private Slider navigationYOffset;
    [SerializeField]
    private float moveOnDistance;

    [Header("Stabilization Settings")]
    [SerializeField]
    private float positionSmoothSpeed = 5f; // How fast position changes are smoothed
    [SerializeField]
    private float rotationSmoothSpeed = 8f; // How fast rotation changes are smoothed
    [SerializeField]
    private float updateInterval = 0.1f; // How often to recalculate target (in seconds)
    [SerializeField]
    private float minimumMovementThreshold = 0.02f; // Minimum movement to trigger update

    private NavMeshPath path;
    private float currentDistance;
    private Vector3[] pathOffset;
    private Vector3 nextNavigationPoint = Vector3.zero;
    
    // Stabilization variables
    private Vector3 targetArrowPosition;
    private Quaternion targetArrowRotation;
    private Vector3 lastPlayerPosition;
    private float lastUpdateTime;
    private bool hasValidTarget = false;

    private void Update() {
        if (navigationController == null) return;
        
        path = navigationController.CalculatedPath;
        if (path == null || path.corners == null || path.corners.Length == 0) {
            if (arrow != null) {
                arrow.SetActive(false);
            }
            hasValidTarget = false;
            return;
        }
        
        // Only recalculate target periodically or when player moves significantly
        bool shouldUpdate = Time.time - lastUpdateTime > updateInterval ||
                           Vector3.Distance(transform.position, lastPlayerPosition) > minimumMovementThreshold;
        
        if (shouldUpdate) {
            AddOffsetToPath();
            SelectNextNavigationPoint();
            CalculateTargetArrowTransform();
            
            lastUpdateTime = Time.time;
            lastPlayerPosition = transform.position;
        }
        
        // Always smooth the arrow movement
        SmoothArrowMovement();
    }

    private void AddOffsetToPath() {
        if (path == null || path.corners == null) return;
        
        pathOffset = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++) {
            pathOffset[i] = new Vector3(path.corners[i].x, path.corners[i].y, path.corners[i].z);
        }
    }

    private void SelectNextNavigationPoint() {
        nextNavigationPoint = SelectNextNavigationPointWithinDistance();
    }

    private Vector3 SelectNextNavigationPointWithinDistance() {
        if (pathOffset == null || pathOffset.Length == 0) {
            return navigationController != null ? navigationController.TargetPosition : Vector3.zero;
        }
        
        Vector3 currentPos = transform.position;
        
        // Find the farthest point we've already passed
        int currentPathIndex = 0;
        
        for (int i = 0; i < pathOffset.Length; i++) {
            Vector3 pathPoint = pathOffset[i];
            float distance = Vector3.Distance(currentPos, pathPoint);
            
            // If we're close to this point (within moveOnDistance), we've "reached" it
            if (distance <= moveOnDistance) {
                currentPathIndex = i;
            }
        }
        
        // Look for the next suitable point ahead in the path sequence
        // Start from the next point but look further ahead if needed
        for (int i = currentPathIndex + 1; i < pathOffset.Length; i++) {
            Vector3 pathPoint = pathOffset[i];
            float distance = Vector3.Distance(currentPos, pathPoint);
            
            // For stairs: ensure the point is far enough OR has significant height difference
            float heightDifference = Mathf.Abs(pathPoint.y - currentPos.y);
            bool isGoodDistance = distance > moveOnDistance * 0.8f; // Slightly more lenient
            bool isStairPoint = heightDifference > 0.4f; // Height difference for stairs
            
            if (isGoodDistance || isStairPoint) {
                currentDistance = distance;
                Debug.Log($"Arrow pointing to point {i}: distance={distance:F2}, height diff={heightDifference:F2}, isStair={isStairPoint}");
                return pathPoint;
            }
        }
        
        // If no good intermediate point found, use final target
        Debug.Log("No suitable intermediate point found, pointing to final target");
        return navigationController != null ? navigationController.TargetPosition : Vector3.zero;
    }

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
        
        // If target is too close to either player or arrow position, hide the arrow
        if (distanceFromPlayer < 0.5f || distanceFromArrow < 0.3f) {
            hasValidTarget = false;
            Debug.Log($"Arrow hidden - target too close (player: {distanceFromPlayer:F2}m, arrow: {distanceFromArrow:F2}m)");
            return;
        }
        
        // Additional check: ensure we're not pointing backwards relative to movement
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
        
        // Calculate target rotation
        Vector3 direction = nextNavigationPoint - arrowPosition;
        if (direction != Vector3.zero) {
            targetArrowRotation = Quaternion.LookRotation(direction);
            hasValidTarget = true;
            Debug.Log($"Arrow target calculated - pointing from {arrowPosition} to {nextNavigationPoint}, distance: {distanceFromArrow:F2}");
        } else {
            hasValidTarget = false;
        }
    }
    
    private void SmoothArrowMovement() {
        if (arrow == null) return;
        
        if (!hasValidTarget) {
            arrow.SetActive(false);
            return;
        }
        
        if (!arrow.activeInHierarchy) {
            arrow.SetActive(true);
            // Initialize smooth values when arrow becomes active
            arrow.transform.position = targetArrowPosition;
            arrow.transform.rotation = targetArrowRotation;
            return;
        }
        
        // Smoothly interpolate position and rotation
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