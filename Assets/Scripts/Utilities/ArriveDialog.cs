using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ArriveDialog : MonoBehaviour
{
    [Header("Dialog Components")]
    [SerializeField] private GameObject dialogCanvas; // The entire dialog canvas
    [SerializeField] private GameObject blockingPanel; // Panel to block background interactions
    [SerializeField] private Button continueButton;
    [SerializeField] private Button endButton;
    [SerializeField] private TextMeshProUGUI messageText; // DialogMessage text object
    
    [Header("Navigation References")]
    [SerializeField] private TargetHandler targetHandler; // Reference to clear navigation
    [SerializeField] private TMP_Dropdown navigationDropdown; // Reference to reset dropdown
    
    [Header("Dialog Settings")]
    [SerializeField] private float triggerDistance = 1.5f; // Distance to trigger dialog
    [SerializeField] private bool showOnlyOnce = true; // Show dialog only once per target
    
    [Header("Modal Settings")]
    [SerializeField] private bool enableModalMode = true; // Enable modal behavior
    [SerializeField] private Color blockingPanelColor = new Color(0, 0, 0, 0.5f); // Semi-transparent black
    
    private bool hasShownForCurrentTarget = false;
    private string currentTargetName = "";
    private bool isDialogActive = false;
    
    // Modal interaction blocking
    private static ArriveDialog activeModalDialog = null;
    
    private void Start()
    {
        // Ensure dialog is hidden at start
        if (dialogCanvas != null)
        {
            dialogCanvas.SetActive(false);
        }
        
        // Setup blocking panel if modal mode is enabled
        SetupBlockingPanel();
        
        // Setup button listeners
        SetupButtons();
        
        Debug.Log("ArriveDialog initialized");
    }
    
    /// <summary>
    /// Setup blocking panel for modal behavior
    /// </summary>
    private void SetupBlockingPanel()
    {
        if (!enableModalMode) return;
        
        if (blockingPanel == null)
        {
            Debug.LogWarning("Blocking panel not assigned - modal functionality will be limited");
            return;
        }
        
        // Ensure blocking panel is initially hidden
        blockingPanel.SetActive(false);
        
        // Setup blocking panel properties if it has an Image component
        var panelImage = blockingPanel.GetComponent<UnityEngine.UI.Image>();
        if (panelImage != null)
        {
            panelImage.color = blockingPanelColor;
            panelImage.raycastTarget = true; // Block raycast events
        }
        
        Debug.Log("Blocking panel setup complete");
    }
    
    private void SetupButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            Debug.Log("Continue button listener added");
        }
        else
        {
            Debug.LogWarning("Continue button is not assigned!");
        }
        
        if (endButton != null)
        {
            endButton.onClick.AddListener(OnEndButtonClicked);
            Debug.Log("End button listener added");
        }
        else
        {
            Debug.LogWarning("End button is not assigned!");
        }
    }
    
    /// <summary>
    /// Check if user has arrived and should show dialog
    /// </summary>
    /// <param name="distanceToTarget">Current distance to target</param>
    /// <param name="targetName">Name of current target</param>
    public void CheckArrival(float distanceToTarget, string targetName = "")
    {
        // Check if we have a new target
        if (!string.IsNullOrEmpty(targetName) && targetName != currentTargetName)
        {
            currentTargetName = targetName;
            // Only reset if we're NOT already close to the new target
            // This prevents immediate dialog showing when selecting nearby targets
            if (distanceToTarget > triggerDistance)
            {
                hasShownForCurrentTarget = false;
            }
            else
            {
                // We're already close to this new target, mark as already shown
                hasShownForCurrentTarget = true;
            }
            Debug.Log($"New target detected for dialog: {targetName} (distance: {distanceToTarget:F2}m)");
        }
        
        // Check if user has arrived and dialog should be shown
        if (distanceToTarget <= triggerDistance && !isDialogActive)
        {
            // Only show if we haven't shown for this target yet (if showOnlyOnce is true)
            if (!showOnlyOnce || !hasShownForCurrentTarget)
            {
                ShowArrivalDialog(targetName);
                hasShownForCurrentTarget = true;
            }
        }
        else if (distanceToTarget > triggerDistance * 2f) // Reset if user moves away significantly
        {
            hasShownForCurrentTarget = false;
        }
    }
    
    /// <summary>
    /// Show the arrival dialog
    /// </summary>
    private void ShowArrivalDialog(string targetName = "")
    {
        if (dialogCanvas != null)
        {
            dialogCanvas.SetActive(true);
            isDialogActive = true;
            
            // Enable modal blocking if configured
            EnableModalBlocking();
            
            // Update message text if available
            if (messageText != null)
            {
                if (!string.IsNullOrEmpty(targetName))
                {
                    messageText.text = $"You have reached {targetName}.";
                }
                else
                {
                    messageText.text = "You have reached your destination.";
                }
            }
            
            Debug.Log($"Arrival dialog shown for target: {targetName}");
        }
        else
        {
            Debug.LogWarning("Dialog canvas is not assigned!");
        }
    }
    
    /// <summary>
    /// Hide the arrival dialog
    /// </summary>
    private void HideArrivalDialog()
    {
        if (dialogCanvas != null)
        {
            dialogCanvas.SetActive(false);
            isDialogActive = false;
            
            // Disable modal blocking
            DisableModalBlocking();
            
            Debug.Log("Arrival dialog hidden");
        }
    }
    
    /// <summary>
    /// Continue button clicked - close dialog and reset target state
    /// </summary>
    private void OnContinueButtonClicked()
    {
        Debug.Log("Continue button clicked - closing dialog and resetting target state");
        
        // Close the dialog first
        HideArrivalDialog();
        
        // Reset target state so dialog can work properly for new targets
        hasShownForCurrentTarget = false;
        currentTargetName = "";
    }
    
    /// <summary>
    /// End button clicked - clear navigation and close dialog
    /// </summary>
    private void OnEndButtonClicked()
    {
        Debug.Log("End button clicked - clearing navigation and closing dialog");
        
        // Clear the navigation target
        if (targetHandler != null)
        {
            targetHandler.ClearAllTargets();
            Debug.Log("Navigation targets cleared");
        }
        else
        {
            Debug.LogWarning("TargetHandler is not assigned!");
        }
        
        // Reset dropdown to "Select" placeholder
        if (navigationDropdown != null)
        {
            navigationDropdown.value = 0; // Assuming index 0 is "Select"
            Debug.Log("Navigation dropdown reset to 'Select'");
        }
        else
        {
            Debug.LogWarning("Navigation dropdown is not assigned!");
        }
        
        // Close the dialog
        HideArrivalDialog();
        
        // Reset state
        ResetDialogState();
    }
    
    /// <summary>
    /// Reset dialog state for new navigation session
    /// </summary>
    public void ResetDialogState()
    {
        hasShownForCurrentTarget = false;
        currentTargetName = "";
        isDialogActive = false;
        Debug.Log("Dialog state reset");
    }
    
    /// <summary>
    /// Set the trigger distance dynamically
    /// </summary>
    public void SetTriggerDistance(float distance)
    {
        triggerDistance = distance;
        Debug.Log($"Dialog trigger distance set to: {distance}m");
    }
    
    /// <summary>
    /// Check if dialog is currently active
    /// </summary>
    public bool IsDialogActive()
    {
        return isDialogActive;
    }
    
    /// <summary>
    /// Manually show dialog (for testing)
    /// </summary>
    public void ShowDialog(string targetName = "Test Target")
    {
        ShowArrivalDialog(targetName);
    }
    
    /// <summary>
    /// Manually hide dialog (for testing)
    /// </summary>
    public void HideDialog()
    {
        HideArrivalDialog();
    }
    
    /// <summary>
    /// Enable modal blocking to prevent interactions with other objects
    /// </summary>
    private void EnableModalBlocking()
    {
        if (!enableModalMode) return;
        
        // Set this as the active modal dialog
        activeModalDialog = this;
        
        // Show blocking panel
        if (blockingPanel != null)
        {
            blockingPanel.SetActive(true);
            Debug.Log("Modal blocking enabled - background interactions blocked");
        }
        
        // Block input events (you can extend this based on your input system)
        Time.timeScale = 1f; // Keep time running but block other interactions
    }
    
    /// <summary>
    /// Disable modal blocking to restore normal interactions
    /// </summary>
    private void DisableModalBlocking()
    {
        if (!enableModalMode) return;
        
        // Clear active modal dialog
        if (activeModalDialog == this)
        {
            activeModalDialog = null;
        }
        
        // Hide blocking panel
        if (blockingPanel != null)
        {
            blockingPanel.SetActive(false);
            Debug.Log("Modal blocking disabled - background interactions restored");
        }
    }
    
    /// <summary>
    /// Check if any modal dialog is currently blocking interactions
    /// </summary>
    public static bool IsAnyDialogModalActive()
    {
        return activeModalDialog != null && activeModalDialog.isDialogActive;
    }
    
    /// <summary>
    /// Check if a specific interaction should be blocked by modal dialog
    /// </summary>
    public static bool ShouldBlockInteraction()
    {
        return IsAnyDialogModalActive();
    }
}