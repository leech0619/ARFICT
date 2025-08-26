using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetHandler : MonoBehaviour
{
    [SerializeField]
    private NavigationController navigationController;
    
    [SerializeField]
    private TMP_Dropdown targetDataDropdown;
    
    [SerializeField]
    private List<Target> navigationTargetObjects = new List<Target>();

    [SerializeField]
    private TextMeshProUGUI distanceLabel; // UI label for distance display
    
    [SerializeField]
    private float rerouteCheckInterval = 2.0f; // How often to check for closer targets (in seconds)
    
    [SerializeField]
    private float rerouteDistanceThreshold = 5.0f; // Minimum distance difference to trigger reroute
    
    private string currentTargetName = ""; // Track current target name for rerouting
    private bool isNavigationActive = false;
    private Coroutine rerouteCoroutine;

    private void Start()
    {
        DeactivateAllTargets();
    }

    public void SetSelectedTargetPositionWithDropdown(int selectedValue)
    {
        // Check if "SELECT" placeholder is selected (typically index 0)
        if (IsSelectPlaceholder(selectedValue))
        {
            // "SELECT" placeholder selected - clear all navigation
            ClearAllNavigation();
            Debug.Log("SELECT placeholder chosen - all navigation cleared");
            return;
        }
        
        Vector3 targetPosition = GetCurrentlySelectedTarget(selectedValue);
        
        DeactivateAllTargets();
        
        if (targetPosition != Vector3.zero)
        {
            navigationController.TargetPosition = targetPosition;
            
            ActivateSelectedTarget(selectedValue);
            
            // Store current target name for rerouting
            if (selectedValue >= 0 && selectedValue < navigationTargetObjects.Count)
            {
                currentTargetName = navigationTargetObjects[selectedValue].Name;
                StartContinuousRerouting();
            }
            
            StartCoroutine(UpdateDistanceAfterPathCalculation());
        }
        else
        {
            ClearAllNavigation();
        }
    }
    
    // Helper method to check if the selected value corresponds to "SELECT" placeholder
    private bool IsSelectPlaceholder(int selectedValue)
    {
        // Check if dropdown has options and the selected option is "SELECT"
        if (targetDataDropdown != null && 
            selectedValue >= 0 && 
            selectedValue < targetDataDropdown.options.Count)
        {
            string optionText = targetDataDropdown.options[selectedValue].text.ToUpper().Trim();
            return optionText == "SELECT" || 
                   optionText == "CHOOSE TARGET" || 
                   optionText == "SELECT TARGET" ||
                   optionText == "---SELECT---";
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

        float distance = 0f;
        
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return distance;
    }

    // Calculate actual NavMesh path distance to a specific target
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
        DeactivateAllTargets();
        StopContinuousRerouting();
        navigationController.TargetPosition = Vector3.zero;
        currentTargetName = "";
        UpdateDistanceLabel(0f);
    }

    public int GetActiveTargetIndex()
    {
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
        if (navigationController != null && navigationController.TargetPosition != Vector3.zero)
        {
            float currentDistance = CalculateCurrentPathDistance();
            if (currentDistance > 0f)
            {
                UpdateDistanceLabel(currentDistance);
            }
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
}