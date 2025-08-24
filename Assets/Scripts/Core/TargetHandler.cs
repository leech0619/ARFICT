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

    private void Start()
    {
        DeactivateAllTargets();
    }

    public void SetSelectedTargetPositionWithDropdown(int selectedValue)
    {
        Vector3 targetPosition = GetCurrentlySelectedTarget(selectedValue);
        
        DeactivateAllTargets();
        
        if (targetPosition != Vector3.zero)
        {
            navigationController.TargetPosition = targetPosition;
            
            ActivateSelectedTarget(selectedValue);
            
            StartCoroutine(UpdateDistanceAfterPathCalculation());
        }
        else
        {
            UpdateDistanceLabel(0f);
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
        navigationController.TargetPosition = Vector3.zero;
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
}