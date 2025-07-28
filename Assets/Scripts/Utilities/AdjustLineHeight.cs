using UnityEngine;
using TMPro;

public class AdjustLineHeight : MonoBehaviour
{   
    [SerializeField]
    private GameObject toggleObject;

    public void Toggle() {
        toggleObject.SetActive(!toggleObject.activeSelf);
    }
}
