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

    private NavMeshPath path;
    private float currentDistance;
    private Vector3[] pathOffset;
    private Vector3 nextNavigationPoint = Vector3.zero;

    private void Update() {
        if (navigationController == null) return;
        
        path = navigationController.CalculatedPath;
        if (path == null || path.corners == null || path.corners.Length == 0) {
            if (arrow != null) {
                arrow.SetActive(false);
            }
            return;
        }
        
        if (arrow != null && !arrow.activeInHierarchy) {
            arrow.SetActive(true);
        }

        AddOffsetToPath();
        SelectNextNavigationPoint();
        UpdateArrowForSlope();
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

    private void UpdateArrowForSlope() {
        if (arrow == null || nextNavigationPoint == Vector3.zero) return;
        
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
            arrow.SetActive(false);
            Debug.Log($"Arrow hidden - target too close (player: {distanceFromPlayer:F2}m, arrow: {distanceFromArrow:F2}m)");
            return;
        }
        
        // Additional check: ensure we're not pointing backwards relative to movement
        Vector3 directionToTarget = (nextNavigationPoint - transform.position).normalized;
        Vector3 playerForward = transform.forward;
        float dotProduct = Vector3.Dot(playerForward, directionToTarget);
        
        // If target is significantly behind us, hide arrow
        if (dotProduct < -0.3f) {
            arrow.SetActive(false);
            Debug.Log($"Arrow hidden - target is behind player (dot: {dotProduct:F2})");
            return;
        }
        
        // Ensure arrow is active
        if (!arrow.activeInHierarchy) {
            arrow.SetActive(true);
        }
        
        arrow.transform.position = arrowPosition;
        
        // Simple slope-aware rotation
        Vector3 direction = nextNavigationPoint - arrowPosition;
        if (direction != Vector3.zero) {
            arrow.transform.LookAt(nextNavigationPoint);
            Debug.Log($"Arrow pointing from {arrowPosition} to {nextNavigationPoint}, distance: {distanceFromArrow:F2}");
        }
    }
}