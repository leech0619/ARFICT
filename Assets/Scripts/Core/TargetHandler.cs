using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages target selection, navigation, and arrival feedback
/// </summary>
public class TargetHandler : MonoBehaviour
{
    [SerializeField]
    private NavigationController navigationController; // Path calculation controller
    
    [SerializeField]
    private TMP_Dropdown targetDataDropdown; // UI dropdown for target selection
    
    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>(); // Available navigation targets

    [SerializeField]
    private TextMeshProUGUI distanceLabel; // UI label for distance display
    
    [SerializeField]
    private ArriveTargetSound arriveTargetSound; // Audio component for arrival notifications
    
    [SerializeField]
    private ArriveDialog arriveDialog; // Dialog component for arrival notifications
    
    [SerializeField]
    private ArriveTargetVibrate arriveTargetVibrate; // Vibration component for arrival notifications
    
    [SerializeField]
    private NavigationSoundController navigationSoundController; // Sound controller for turn instructions
    
    [SerializeField]
    private float rerouteCheckInterval = 1.0f; // How often to check for closer targets (in seconds)
    
    [SerializeField]
    private float rerouteDistanceThreshold = 2.0f; // Minimum distance difference to trigger reroute
    
    // State tracking variables
    private string currentTargetName = ""; // Track current target name for rerouting
    private bool isNavigationActive = false;
    private Coroutine rerouteCoroutine;
    
    // Action feedback
    private ActionLabel actionLabel; // Reference to action label for target navigation messages
    
    // Navigation direction tracking
    private Vector3 lastUserPosition = Vector3.zero;
    private Vector3 lastDirection = Vector3.zero;
    private float lastDirectionCheckTime = 0f;
    private float directionCheckInterval = 1f; // Check direction every 1 second

    private void Start()
    {
        DeactivateAllTargets(); // Hide all targets initially
        
        // Find ActionLabel component automatically
        actionLabel = FindObjectOfType<ActionLabel>();
        
        if (actionLabel == null)
        {
            Debug.LogWarning("ActionLabel not found - Target navigation messages will not be displayed");
        }
    }

    // Helper method to display navigation message
    private void ShowNavigationToTargetMessage(string targetName)
    {
        if (actionLabel == null)
        {
            actionLabel = FindObjectOfType<ActionLabel>();
        }
        
        if (actionLabel != null)
        {
            actionLabel.ShowNavigationToTarget(targetName);
        }
    }

    public void SetSelectedTargetPositionWithDropdown(int selectedValue)
    {
        // Check if "SELECT" placeholder is selected
        if (IsSelectPlaceholder(selectedValue))
        {
            ClearAllNavigation();
            Debug.Log("SELECT placeholder chosen - all navigation cleared");
            return;
        }
        
        Vector3 targetPosition = GetCurrentlySelectedTarget(selectedValue);
        
        DeactivateAllTargets(); // Hide all target objects
        
        if (targetPosition != Vector3.zero)
        {
            navigationController.TargetPosition = targetPosition;
            
            ActivateSelectedTarget(selectedValue); // Show selected target
            
            // Store target name and start rerouting system
            if (selectedValue >= 0 && selectedValue < navigationTargetObjects.Count)
            {
                currentTargetName = navigationTargetObjects[selectedValue].Name;
                StartContinuousRerouting();
                
                // Show navigation start message via ActionLabel
                ShowNavigationToTargetMessage(currentTargetName);
            }
            
            StartCoroutine(UpdateDistanceAfterPathCalculation());
        }
        else
        {
            ClearAllNavigation();
        }
    }
    
    // Check if selected dropdown option is "Select" placeholder
    private bool IsSelectPlaceholder(int selectedValue)
    {
        if (targetDataDropdown != null && 
            selectedValue >= 0 && 
            selectedValue < targetDataDropdown.options.Count)
        {
            string optionText = targetDataDropdown.options[selectedValue].text.Trim();
            return optionText == "Select";
        }
        
        // Fallback: treat index 0 as placeholder if no dropdown reference
        return selectedValue == 0;
    }
    
    // Method to completely clear all navigation state
    private void ClearAllNavigation()
    {
        DeactivateAllTargets();
        navigationController.TargetPosition = Vector3.zero;
        
        // Explicitly clear the calculated path to ensure line renderer disappears
        navigationController.ClearCalculatedPath();
        
        StopContinuousRerouting();
        UpdateDistanceLabel(0f);
        currentTargetName = "";
        
        // Reset arrival sound state
        if (arriveTargetSound != null)
        {
            arriveTargetSound.ResetSoundState();
        }
        
        // Reset arrival dialog state
        if (arriveDialog != null)
        {
            arriveDialog.ResetDialogState();
        }
        
        // Reset arrival vibration state
        if (arriveTargetVibrate != null)
        {
            arriveTargetVibrate.ResetVibrateState();
        }
        
        // Reset navigation sound state
        if (navigationSoundController != null)
        {
            navigationSoundController.ResetNavigationSoundState();
        }
    }

    private IEnumerator UpdateDistanceAfterPathCalculation()
    {
        yield return new WaitForEndOfFrame();
        
        float distance = CalculateCurrentPathDistance();
        UpdateDistanceLabel(distance);
    }

    private float CalculateCurrentPathDistance()
    {
        if (navigationController == null || navigationController.CalculatedPath == null)
        {
            return 0f;
        }

        var path = navigationController.CalculatedPath;
        if (path.corners.Length < 2)
        {
            return 0f;
        }

        // Sum distances between all path corners
        float distance = 0f;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return distance;
    }

    // Calculate NavMesh path distance to a specific target for rerouting
    private float CalculateNavMeshDistanceToTarget(Vector3 targetPosition)
    {
        if (navigationController == null) return float.MaxValue;

        Vector3 currentPosition = navigationController.transform.position;
        UnityEngine.AI.NavMeshPath tempPath = new UnityEngine.AI.NavMeshPath();
        
        if (UnityEngine.AI.NavMesh.CalculatePath(currentPosition, targetPosition, UnityEngine.AI.NavMesh.AllAreas, tempPath))
        {
            float totalDistance = 0f;
            
            if (tempPath.corners.Length >= 2)
            {
                // Sum all corner distances
                for (int i = 0; i < tempPath.corners.Length - 1; i++)
                {
                    totalDistance += Vector3.Distance(tempPath.corners[i], tempPath.corners[i + 1]);
                }
            }
            else if (tempPath.corners.Length == 1)
            {
                totalDistance = Vector3.Distance(currentPosition, targetPosition);
            }
            
            return totalDistance;
        }
        
        return float.MaxValue;
    }

    private void UpdateDistanceLabel(float distance)
    {
        if (distanceLabel != null)
        {
            if (distance > 0f)
            {
                distanceLabel.text = $"Distance: {distance:F2} m";
                
                // Check for arrival sound trigger
                if (arriveTargetSound != null && !string.IsNullOrEmpty(currentTargetName) && navigationController != null)
                {
                    Vector3 userPosition = navigationController.transform.position;
                    Vector3 targetPosition = navigationController.TargetPosition;
                    arriveTargetSound.CheckArrival(distance, currentTargetName, userPosition, targetPosition);
                }
                
                // Check for arrival dialog trigger
                if (arriveDialog != null && !string.IsNullOrEmpty(currentTargetName))
                {
                    arriveDialog.CheckArrival(distance, currentTargetName);
                }
                
                // Check for arrival vibration trigger
                if (arriveTargetVibrate != null && !string.IsNullOrEmpty(currentTargetName) && navigationController != null)
                {
                    Vector3 userPosition = navigationController.transform.position;
                    Vector3 targetPosition = navigationController.TargetPosition;
                    arriveTargetVibrate.CheckArrival(distance, currentTargetName, userPosition, targetPosition);
                }
            }
            else
            {
                distanceLabel.text = "Distance: 0.00 m";
            }
        }
    }

    private Vector3 GetCurrentlySelectedTarget(int selectedValue)
    {
        if (selectedValue < 0 || selectedValue >= navigationTargetObjects.Count)
        {
            return Vector3.zero;
        }

        Target selectedTarget = navigationTargetObjects[selectedValue];
        
        if (selectedTarget?.PositionObject != null)
        {
            // Check if multiple targets exist with same name
            List<Target> matchingTargets = navigationTargetObjects.FindAll(x => 
                x.Name.ToLower().Equals(selectedTarget.Name.ToLower()));
            
            if (matchingTargets.Count > 1)
            {
                // Find closest target by NavMesh distance
                Target closestTarget = GetClosestTargetByTargetText(selectedTarget.Name);
                if (closestTarget != null)
                {
                    return closestTarget.PositionObject.transform.position;
                }
            }
            
            return selectedTarget.PositionObject.transform.position;
        }

        return Vector3.zero;
    }

    public Target GetCurrentTargetByTargetText(string targetText)
    {
        return navigationTargetObjects.Find(x => 
            x.Name.ToLower().Equals(targetText.ToLower()));
    }

    // Find closest target by name using NavMesh path distance
    public Target GetClosestTargetByTargetText(string targetText)
    {
        List<Target> matchingTargets = navigationTargetObjects.FindAll(x => 
            x.Name.ToLower().Equals(targetText.ToLower()));
        
        if (matchingTargets.Count == 0) return null;
        if (matchingTargets.Count == 1) return matchingTargets[0];
        
        Target closestTarget = null;
        float closestDistance = float.MaxValue;
        
        foreach (Target target in matchingTargets)
        {
            if (target?.PositionObject != null)
            {
                float distance = CalculateNavMeshDistanceToTarget(target.PositionObject.transform.position);
                
                if (distance < closestDistance && distance != float.MaxValue)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
        }
        
        return closestTarget;
    }

    private void DeactivateAllTargets()
    {
        foreach (Target target in navigationTargetObjects)
        {
            if (target != null && target.PositionObject != null)
            {
                target.PositionObject.SetActive(false);
            }
        }
    }

    private void ActivateSelectedTarget(int targetIndex)
    {
        if (targetIndex >= 0 && targetIndex < navigationTargetObjects.Count)
        {
            Target selectedTarget = navigationTargetObjects[targetIndex];
            if (selectedTarget?.PositionObject != null)
            {
                // Check if multiple targets exist with same name
                List<Target> matchingTargets = navigationTargetObjects.FindAll(x => 
                    x.Name.ToLower().Equals(selectedTarget.Name.ToLower()));
                
                if (matchingTargets.Count > 1)
                {
                    // Activate closest target
                    Target closestTarget = GetClosestTargetByTargetText(selectedTarget.Name);
                    if (closestTarget?.PositionObject != null)
                    {
                        closestTarget.PositionObject.SetActive(true);
                        return;
                    }
                }
                
                selectedTarget.PositionObject.SetActive(true);
            }
        }
    }

    public void ActivateTargetByName(string targetName)
    {
        DeactivateAllTargets();
        
        Target targetToActivate = GetClosestTargetByTargetText(targetName);
        if (targetToActivate?.PositionObject != null)
        {
            targetToActivate.PositionObject.SetActive(true);
            navigationController.TargetPosition = targetToActivate.PositionObject.transform.position;
            
            // Store current target name for rerouting
            currentTargetName = targetName;
            StartContinuousRerouting();
            
            // Show navigation message
            ShowNavigationToTargetMessage(targetName);
            
            StartCoroutine(UpdateDistanceAfterPathCalculation());
        }
    }

    public Target GetActiveTarget()
    {
        foreach (Target target in navigationTargetObjects)
        {
            if (target != null && target.PositionObject != null && target.PositionObject.activeSelf)
            {
                return target;
            }
        }
        return null;
    }

    public void ClearAllTargets()
    {
        // Reset all navigation state
        DeactivateAllTargets();
        StopContinuousRerouting();
        navigationController.TargetPosition = Vector3.zero;
        currentTargetName = "";
        UpdateDistanceLabel(0f);
    }

    public int GetActiveTargetIndex()
    {
        // Find which target is currently active
        for (int i = 0; i < navigationTargetObjects.Count; i++)
        {
            Target target = navigationTargetObjects[i];
            if (target != null && target.PositionObject != null && target.PositionObject.activeSelf)
            {
                return i;
            }
        }
        return -1;
    }

    private void Update()
    {
        // Main navigation update loop
        if (navigationController != null && navigationController.TargetPosition != Vector3.zero)
        {
            float currentDistance = CalculateCurrentPathDistance();
            if (currentDistance > 0f)
            {
                UpdateDistanceLabel(currentDistance);
            }
            
            // Check arrival sound using direct distance for accuracy
            if (arriveTargetSound != null && !string.IsNullOrEmpty(currentTargetName))
            {
                Vector3 userPosition = navigationController.transform.position;
                Vector3 targetPosition = navigationController.TargetPosition;
                float directDistance = Vector3.Distance(userPosition, targetPosition);
                
                arriveTargetSound.CheckArrival(directDistance, currentTargetName, userPosition, targetPosition);
            }
            
            // Check arrival dialog using direct distance for accuracy
            if (arriveDialog != null && !string.IsNullOrEmpty(currentTargetName))
            {
                float directDistance = Vector3.Distance(
                    navigationController.transform.position, 
                    navigationController.TargetPosition
                );
                
                arriveDialog.CheckArrival(directDistance, currentTargetName);
            }
            
            // Check direct distance for arrival vibration (more accurate for close proximity)
            if (arriveTargetVibrate != null && !string.IsNullOrEmpty(currentTargetName))
            {
                Vector3 userPosition = navigationController.transform.position;
                Vector3 targetPosition = navigationController.TargetPosition;
                float directDistance = Vector3.Distance(userPosition, targetPosition);
                
                // Use direct distance for vibration detection
                arriveTargetVibrate.CheckArrival(directDistance, currentTargetName, userPosition, targetPosition);
            }
            
            // Check for navigation direction changes and play turn instructions
            CheckNavigationDirection();
        }
    }
    
    // Start continuous rerouting for targets with multiple instances
    private void StartContinuousRerouting()
    {
        StopContinuousRerouting(); // Stop any existing rerouting
        
        if (!string.IsNullOrEmpty(currentTargetName))
        {
            // Check if there are multiple targets with the same name
            List<Target> matchingTargets = navigationTargetObjects.FindAll(x => 
                x.Name.ToLower().Equals(currentTargetName.ToLower()));
            
            if (matchingTargets.Count > 1)
            {
                isNavigationActive = true;
                rerouteCoroutine = StartCoroutine(ContinuousReroutingCoroutine());
            }
        }
    }
    
    // Stop continuous rerouting
    private void StopContinuousRerouting()
    {
        isNavigationActive = false;
        if (rerouteCoroutine != null)
        {
            StopCoroutine(rerouteCoroutine);
            rerouteCoroutine = null;
        }
    }
    
    // Coroutine that continuously checks for closer targets
    private IEnumerator ContinuousReroutingCoroutine()
    {
        while (isNavigationActive && !string.IsNullOrEmpty(currentTargetName))
        {
            yield return new WaitForSeconds(rerouteCheckInterval);
            
            // Check if we should reroute to a closer target
            CheckAndRerouteToCloserTarget();
        }
    }
    
    // Check if there's a closer target and reroute if necessary
    private void CheckAndRerouteToCloserTarget()
    {
        if (string.IsNullOrEmpty(currentTargetName) || navigationController == null)
            return;
        
        // Get current active target
        Target currentActiveTarget = GetActiveTarget();
        if (currentActiveTarget == null)
            return;
        
        // Find the closest target by name
        Target closestTarget = GetClosestTargetByTargetText(currentTargetName);
        if (closestTarget == null || closestTarget == currentActiveTarget)
            return;
        
        // Calculate distances to compare
        float currentTargetDistance = CalculateNavMeshDistanceToTarget(currentActiveTarget.PositionObject.transform.position);
        float closestTargetDistance = CalculateNavMeshDistanceToTarget(closestTarget.PositionObject.transform.position);
        
        // Only reroute if the new target is significantly closer
        if (closestTargetDistance < currentTargetDistance && 
            (currentTargetDistance - closestTargetDistance) > rerouteDistanceThreshold)
        {
            // Reroute to the closer target
            Debug.Log($"Rerouting to closer {currentTargetName}: {closestTargetDistance:F2}m vs {currentTargetDistance:F2}m");
            
            DeactivateAllTargets();
            closestTarget.PositionObject.SetActive(true);
            navigationController.TargetPosition = closestTarget.PositionObject.transform.position;
            
            StartCoroutine(UpdateDistanceAfterPathCalculation());
        }
    }
    
    // Called when navigation is complete or cancelled
    public void OnNavigationComplete()
    {
        StopContinuousRerouting();
        currentTargetName = "";
    }
    
    /// <summary>
    /// Check navigation direction and play turn instructions
    /// </summary>
    private void CheckNavigationDirection()
    {
        if (navigationSoundController == null || navigationController == null)
            return;
            
        // Only check direction periodically to avoid spam
        if (Time.time - lastDirectionCheckTime < directionCheckInterval)
            return;
            
        Vector3 currentUserPosition = navigationController.transform.position;
        
        // Skip if user hasn't moved significantly
        if (Vector3.Distance(currentUserPosition, lastUserPosition) < 0.5f)
            return;
            
        // Use the NavigationController's CalculatedPath instead of NavMeshAgent
        var calculatedPath = navigationController.CalculatedPath;
        if (calculatedPath == null || calculatedPath.corners.Length < 2)
        {
            Debug.Log("NavigationSoundController: No valid path found in NavigationController.");
            return;
        }
            
        // Find the next waypoint in the path that's ahead of current position
        Vector3 nextWaypoint = Vector3.zero;
        bool foundNextWaypoint = false;
        
        for (int i = 0; i < calculatedPath.corners.Length - 1; i++)
        {
            float distanceToCorner = Vector3.Distance(currentUserPosition, calculatedPath.corners[i]);
            if (distanceToCorner > 1.0f) // Look for waypoints that are at least 1 meter away
            {
                nextWaypoint = calculatedPath.corners[i];
                foundNextWaypoint = true;
                break;
            }
        }
        
        // If no distant waypoint found, use the final destination
        if (!foundNextWaypoint && calculatedPath.corners.Length > 0)
        {
            nextWaypoint = calculatedPath.corners[calculatedPath.corners.Length - 1];
            foundNextWaypoint = true;
        }
        
        if (!foundNextWaypoint)
        {
            return;
        }
        
        // Calculate direction to next waypoint
        Vector3 directionToWaypoint = (nextWaypoint - currentUserPosition).normalized;
        
        // Remove Y component for 2D direction calculation
        directionToWaypoint.y = 0;
        
        // Calculate user's forward direction (assuming user faces movement direction)
        Vector3 userMovementDirection = (currentUserPosition - lastUserPosition).normalized;
        userMovementDirection.y = 0;
        
        // Only calculate turn if we have a valid movement direction
        if (userMovementDirection.magnitude > 0.1f)
        {
            // Calculate angle between movement direction and required direction
            float angle = Vector3.SignedAngle(userMovementDirection, directionToWaypoint, Vector3.up);
            
            // Only give instructions for significant direction changes
            if (Mathf.Abs(angle) > 15f) // 15 degree threshold
            {
                navigationSoundController.PlayDirectionInstruction(angle);
                Debug.Log($"Navigation instruction: angle = {angle:F1}°");
            }
        }
        
        // Update tracking variables
        lastUserPosition = currentUserPosition;
        lastDirection = directionToWaypoint;
        lastDirectionCheckTime = Time.time;
    }
}