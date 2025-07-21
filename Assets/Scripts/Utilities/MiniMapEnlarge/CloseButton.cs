using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public ResetMapButton resetMapButton; // Reference to the script handling map reset
    

    public void OnCloseButtonClick()
    {
        resetMapButton.ToggleMapSize();
    }
    public void HideButton()
    {
        gameObject.SetActive(false); // Hide the close button
    }

    public void ShowButton()
    {
        gameObject.SetActive(true); // Show the close button
    }

}
