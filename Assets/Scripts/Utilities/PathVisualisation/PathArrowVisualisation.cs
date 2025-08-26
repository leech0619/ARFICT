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
        
        // Instead of finding closest point, find the farthest point we've already passed
        // This prevents pointing backwards
        int currentPathIndex = 0;
        
        for (int i = 0; i < pathOffset.Length; i++) {
            Vector3 pathPoint = pathOffset[i];
            float distance = Vector3.Distance(currentPos, pathPoint);
            
            // If we're close to this point (within moveOnDistance), we've "reached" it
            if (distance <= moveOnDistance) {
                currentPathIndex = i;
            }
        }
        
        // Look for the next point ahead in the path sequence
        int nextIndex = currentPathIndex + 1;
        if (nextIndex < pathOffset.Length) {
            Vector3 nextPoint = pathOffset[nextIndex];
            currentDistance = Vector3.Distance(currentPos, nextPoint);
            
            // For NavMesh with slopes/stairs, accept any forward point
            // The NavMesh already handles the slope calculations
            Debug.Log($"Arrow pointing to point {nextIndex}: distance={currentDistance:F2}, height={nextPoint.y:F2}");
            return nextPoint;
        }
        
        // If we're at or near the last path point, point to final target
        Debug.Log("Near end of path, pointing to final target");
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
        
        // Check distance AFTER applying height offset to avoid false "too close" detection
        float distanceToTarget = Vector3.Distance(arrowPosition, nextNavigationPoint);
        if (distanceToTarget < 0.2f) {
            // If target is too close to the arrow position, hide the arrow
            arrow.SetActive(false);
            Debug.Log("Arrow hidden - target too close to arrow position");
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
            Debug.Log($"Arrow pointing from {arrowPosition} to {nextNavigationPoint}, distance: {distanceToTarget:F2}");
        }
    }
}