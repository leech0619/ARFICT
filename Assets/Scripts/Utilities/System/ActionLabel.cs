using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays temporary action feedback messages for 3 seconds
/// Works with a single TextMeshPro Text UI component attached to this GameObject
/// Shows status messages for QR scanning, navigation toggles, and mode changes
/// </summary>
public class ActionLabel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI actionText; // TextMeshPro text component (this GameObject)
    
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 3f; // How long to show the label (seconds)
    
    // Component references for checking states
    private QrCodeRecenter qrCodeRecenter; // Reference to QR code scanner
    private SwitchPathVisualisation switchPathVis; // Reference to path visualization switcher
    private AdjustLineHeight adjustLineHeight; // Reference to line height adjuster
    
    // State tracking
    private Coroutine hideCoroutine; // Coroutine for auto-hiding the label
    
    private void Start()
    {
        // Get TextMeshPro component if not assigned
        if (actionText == null)
        {
            actionText = GetComponent<TextMeshProUGUI>();
        }
        
        // Find component references automatically
        FindComponentReferences();
        
        // Initialize text as hidden but keep GameObject active so other scripts can find it
        if (actionText != null)
        {
            actionText.text = ""; // Clear text instead of deactivating GameObject
            actionText.enabled = false; // Hide the text component
        }
        
        Debug.Log("ActionLabel initialized");
    }
    
    /// <summary>
    /// Automatically finds required component references in the scene
    /// </summary>
    private void FindComponentReferences()
    {
        if (qrCodeRecenter == null)
        {
            qrCodeRecenter = FindObjectOfType<QrCodeRecenter>();
        }
        
        if (switchPathVis == null)
        {
            switchPathVis = FindObjectOfType<SwitchPathVisualisation>();
        }
        
        if (adjustLineHeight == null)
        {
            adjustLineHeight = FindObjectOfType<AdjustLineHeight>();
        }
        
        Debug.Log($"ActionLabel references found - QR: {qrCodeRecenter != null}, Switch: {switchPathVis != null}, Adjust: {adjustLineHeight != null}");
    }
    
    /// <summary>
    /// Shows the action label with specified message for the display duration
    /// </summary>
    /// <param name="message">Message to display</param>
    public void ShowActionLabel(string message)
    {
        if (actionText == null)
        {
            Debug.LogWarning("ActionText component not assigned!");
            return;
        }
        
        // Set the message text
        actionText.text = message;
        
        // Show the text component
        actionText.enabled = true;
        
        // Cancel any existing hide coroutine
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        // Start new hide coroutine
        hideCoroutine = StartCoroutine(HideAfterDelay());
        
        Debug.Log($"ActionLabel shown: {message}");
    }
    
    /// <summary>
    /// Coroutine to automatically hide the label after the display duration
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        
        if (actionText != null)
        {
            actionText.enabled = false;
            actionText.text = "";
        }
        
        hideCoroutine = null;
        Debug.Log("ActionLabel hidden after timeout");
    }
    
    /// <summary>
    /// Immediately hides the action label
    /// </summary>
    public void HideActionLabel()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        
        if (actionText != null)
        {
            actionText.enabled = false;
            actionText.text = "";
        }
        
        Debug.Log("ActionLabel hidden manually");
    }
    
    /// <summary>
    /// Shows QR code scan success message
    /// Call this when QR code is successfully scanned and relocation completes
    /// </summary>
    public void ShowQRScanSuccess()
    {
        ShowActionLabel("QR Code scanned success");
    }
    
    /// <summary>
    /// Shows QR code invalid message
    /// Call this when QR code is scanned but target is not found or QR is invalid
    /// </summary>
    public void ShowQRScanInvalid()
    {
        ShowActionLabel("QR code is invalid");
    }
    
    /// <summary>
    /// Shows navigation toggle status message
    /// Call this when navigation visibility is toggled on/off
    /// </summary>
    /// <param name="isNavigationOn">Whether navigation is now active</param>
    public void ShowNavigationToggle(bool isNavigationOn)
    {
        string message = isNavigationOn ? "Navigation On" : "Navigation Off";
        ShowActionLabel(message);
    }
    
    /// <summary>
    /// Shows navigation mode change message
    /// Call this when navigation visualization mode is switched
    /// </summary>
    /// <param name="isArrowMode">True for arrow mode, false for line mode</param>
    public void ShowNavigationModeChange(bool isArrowMode)
    {
        string message = isArrowMode ? "Navigation Mode : Arrow" : "Navigation Mode : Line";
        ShowActionLabel(message);
    }
    
    /// <summary>
    /// Shows line height slider status message
    /// Call this when line height slider is toggled on/off
    /// </summary>
    /// <param name="isSliderOn">Whether the slider is now active</param>
    public void ShowLineHeightSliderToggle(bool isSliderOn)
    {
        string message = isSliderOn ? "Line Height Slider : On" : "Line Height Slider : Off";
        ShowActionLabel(message);
    }
    
    /// <summary>
    /// Shows target navigation start message
    /// Call this when a navigation target is selected
    /// </summary>
    /// <param name="targetName">Name of the selected target</param>
    public void ShowNavigationToTarget(string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            ShowActionLabel("Navigating to target");
        }
        else
        {
            ShowActionLabel($"Navigating to {targetName}");
        }
    }
    
    /// <summary>
    /// Shows reroute message when navigation switches to a closer target
    /// Call this when the system automatically reroutes to a closer target
    /// </summary>
    public void ShowRerouteMessage()
    {
        ShowActionLabel("Reroute to Closer Target");
    }
    
    /// <summary>
    /// Shows reroute message with target name when navigation switches to a closer target
    /// Call this when the system automatically reroutes to a specific closer target
    /// </summary>
    /// <param name="targetName">Name of the closer target being rerouted to</param>
    public void ShowRerouteMessage(string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            ShowRerouteMessage();
        }
        else
        {
            ShowActionLabel($"Reroute to Closer Target: {targetName}");
        }
    }
    
    /// <summary>
    /// Sets the display duration for action labels
    /// </summary>
    /// <param name="duration">Duration in seconds</param>
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
        Debug.Log($"ActionLabel display duration set to: {duration} seconds");
    }
    
    /// <summary>
    /// Manually set component references if automatic finding doesn't work
    /// </summary>
    public void SetComponentReferences(QrCodeRecenter qr, SwitchPathVisualisation switchVis, AdjustLineHeight adjustHeight)
    {
        qrCodeRecenter = qr;
        switchPathVis = switchVis;
        adjustLineHeight = adjustHeight;
        Debug.Log("ActionLabel component references manually set");
    }
    
    /// <summary>
    /// Returns whether the action label is currently visible
    /// </summary>
    public bool IsActionLabelVisible()
    {
        return actionText != null && actionText.enabled;
    }
    
    /// <summary>
    /// Test method to manually trigger ActionLabel (for debugging)
    /// Call this from inspector or other scripts to test functionality
    /// </summary>
    [ContextMenu("Test ActionLabel")]
    public void TestActionLabel()
    {
        ShowActionLabel("Test Message - ActionLabel Working!");
    }
}
