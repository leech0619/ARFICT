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

    public void SetSelectedTargetPositionWithDropdown(int selectedValue)
    {
        Vector3 targetPosition = GetCurrentlySelectedTarget(selectedValue);
        
        if (targetPosition != Vector3.zero)
        {
            navigationController.TargetPosition = targetPosition;
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
}