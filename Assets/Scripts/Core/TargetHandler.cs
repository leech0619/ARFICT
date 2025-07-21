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

    public void SetSelectedTargetPositionWithDropdown(int selectedValue)
    {
        Vector3 targetPosition = GetCurrentlySelectedTarget(selectedValue);
        
        if (targetPosition != Vector3.zero)
        {
            navigationController.TargetPosition = targetPosition;
            
            // Update distance after path calculation
            StartCoroutine(UpdateDistanceAfterPathCalculation());
        }
        else
        {
            UpdateDistanceLabel(0f);
        }
    }

    private IEnumerator UpdateDistanceAfterPathCalculation()
    {
        // Wait for NavigationController to calculate new path
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
        
        // Calculate total distance by summing distances between consecutive path points
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return distance;
    }

    private void UpdateDistanceLabel(float distance)
    {
        if (distanceLabel != null)
        {
            if (distance > 0f)
            {
                // Always show distance in meters with two decimal places
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
        
        if (selectedTarget != null && selectedTarget.PositionObject != null)
        {
            return selectedTarget.PositionObject.transform.position;
        }

        return Vector3.zero;
    }

    public Target GetCurrentTargetByTargetText(string targetText)
    {
        return navigationTargetObjects.Find(x => 
            x.Name.ToLower().Equals(targetText.ToLower()));
    }

    // Optional: Real-time distance updates as user moves
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