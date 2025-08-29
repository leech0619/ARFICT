using UnityEngine;
using TMPro;

/// <summary>
/// Simple UI toggle utility for showing/hiding objects
/// </summary>
public class AdjustLineHeight : MonoBehaviour
{   
    [SerializeField]
    private GameObject toggleObject; // Object to show/hide

    // Toggle the visibility of the assigned object
    public void Toggle() {
        toggleObject.SetActive(!toggleObject.activeSelf);
    }
}
