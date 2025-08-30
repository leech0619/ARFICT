using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls tutorial canvas navigation with numbered tutorial panels
/// Manages sequential tutorial flow with next/back button functionality
/// </summary>
public class TutorialController : MonoBehaviour
{
    [System.Serializable]
    public class TutorialPanel
    {
        [Header("Tutorial Panel Settings")]
        public GameObject tutorialObject;     // The tutorial canvas GameObject
        public int tutorialNumber;           // Order number for this tutorial (1, 2, 3, 4...)
        public Button nextButton;            // Next button for this tutorial
        public Button backButton;            // Back button for this tutorial
    }

    [Header("Tutorial Configuration")]
    [SerializeField] private List<TutorialPanel> tutorialPanels = new List<TutorialPanel>();

    [Header("Tutorial Control")]
    [SerializeField] private int currentTutorialIndex; // Current active tutorial number
    [SerializeField] private bool startTutorialOnAwake = true;  // Auto-start first tutorial

    // State tracking
    private int totalTutorials;
    private Dictionary<int, TutorialPanel> tutorialMap = new Dictionary<int, TutorialPanel>();

    void Awake()
    {
        InitializeTutorialSystem();

        if (startTutorialOnAwake)
        {
            LoadTutorial(1); // Start with first tutorial
        }
    }

    void Start()
    {
        SetupButtonListeners();
    }

    /// <summary>
    /// Initialize the tutorial system and create tutorial mapping
    /// </summary>
    private void InitializeTutorialSystem()
    {
        // Clear existing mapping
        tutorialMap.Clear();

        // Create mapping of tutorial numbers to panels
        foreach (TutorialPanel panel in tutorialPanels)
        {
            if (panel.tutorialObject != null)
            {
                tutorialMap[panel.tutorialNumber] = panel;

                // Initially deactivate all tutorials
                panel.tutorialObject.SetActive(false);
            }
        }

        totalTutorials = tutorialMap.Count;
        Debug.Log($"Tutorial System initialized with {totalTutorials} tutorials");
    }

    /// <summary>
    /// Setup button click listeners for all tutorial panels
    /// </summary>
    private void SetupButtonListeners()
    {
        foreach (TutorialPanel panel in tutorialPanels)
        {
            // Setup Next button
            if (panel.nextButton != null)
            {
                int tutorialNum = panel.tutorialNumber; // Capture for closure
                panel.nextButton.onClick.RemoveAllListeners();
                panel.nextButton.onClick.AddListener(() => LoadNextTutorial(tutorialNum));
            }

            // Setup Back button
            if (panel.backButton != null)
            {
                int tutorialNum = panel.tutorialNumber; // Capture for closure
                panel.backButton.onClick.RemoveAllListeners();
                panel.backButton.onClick.AddListener(() => LoadPreviousTutorial(tutorialNum));
            }
        }

        Debug.Log("Tutorial button listeners configured");
    }

    /// <summary>
    /// Load specific tutorial by number
    /// </summary>
    /// <param name="tutorialNumber">Tutorial number to load (1-based)</param>
    public void LoadTutorial(int tutorialNumber)
    {
        // Validate tutorial number
        if (!tutorialMap.ContainsKey(tutorialNumber))
        {
            Debug.LogWarning($"Tutorial {tutorialNumber} not found!");
            return;
        }

        // Deactivate current tutorial
        DeactivateAllTutorials();

        // Activate target tutorial
        TutorialPanel targetPanel = tutorialMap[tutorialNumber];
        targetPanel.tutorialObject.SetActive(true);

        // Update current index
        currentTutorialIndex = tutorialNumber;

        Debug.Log($"Loaded Tutorial {tutorialNumber}");
    }

    /// <summary>
    /// Load next tutorial from current tutorial
    /// </summary>
    /// <param name="currentTutorial">Current tutorial number</param>
    public void LoadNextTutorial(int currentTutorial)
    {
        int nextTutorial = currentTutorial + 1;

        if (tutorialMap.ContainsKey(nextTutorial))
        {
            LoadTutorial(nextTutorial);
            Debug.Log($"Next: {currentTutorial} -> {nextTutorial}");
        }
        else
        {
            Debug.Log($"No next tutorial available from Tutorial {currentTutorial}");
            // Optionally handle end of tutorials (e.g., exit tutorial mode)
            OnTutorialComplete();
        }
    }

    /// <summary>
    /// Load previous tutorial from current tutorial
    /// </summary>
    /// <param name="currentTutorial">Current tutorial number</param>
    public void LoadPreviousTutorial(int currentTutorial)
    {
        int previousTutorial = currentTutorial - 1;

        if (tutorialMap.ContainsKey(previousTutorial))
        {
            LoadTutorial(previousTutorial);
            Debug.Log($"Back: {currentTutorial} -> {previousTutorial}");
        }
        else
        {
            Debug.Log($"No previous tutorial available from Tutorial {currentTutorial}");
            // Optionally handle start of tutorials (e.g., exit tutorial mode)
            OnTutorialStart();
        }
    }

    /// <summary>
    /// Deactivate all tutorial panels
    /// </summary>
    private void DeactivateAllTutorials()
    {
        foreach (TutorialPanel panel in tutorialPanels)
        {
            if (panel.tutorialObject != null)
            {
                panel.tutorialObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Get current active tutorial number
    /// </summary>
    /// <returns>Current tutorial number</returns>
    public int GetCurrentTutorial()
    {
        return currentTutorialIndex;
    }

    /// <summary>
    /// Check if tutorial number exists
    /// </summary>
    /// <param name="tutorialNumber">Tutorial number to check</param>
    /// <returns>True if tutorial exists</returns>
    public bool HasTutorial(int tutorialNumber)
    {
        return tutorialMap.ContainsKey(tutorialNumber);
    }

    /// <summary>
    /// Get total number of tutorials
    /// </summary>
    /// <returns>Total tutorial count</returns>
    public int GetTotalTutorials()
    {
        return totalTutorials;
    }

    /// <summary>
    /// Called when tutorial sequence completes
    /// Override this method to customize end-of-tutorial behavior
    /// </summary>
    protected virtual void OnTutorialComplete()
    {
        Debug.Log("Tutorial sequence completed!");
        // Add custom completion logic here (e.g., return to main menu, unlock features)
    }

    /// <summary>
    /// Called when trying to go back from first tutorial
    /// Override this method to customize start-of-tutorial behavior
    /// </summary>
    protected virtual void OnTutorialStart()
    {
        Debug.Log("At tutorial start!");
        // Add custom start logic here (e.g., exit tutorial mode)
    }

    // Public methods for external control

    /// <summary>
    /// Jump to first tutorial
    /// </summary>
    public void GoToFirstTutorial()
    {
        LoadTutorial(1);
    }

    /// <summary>
    /// Jump to last tutorial
    /// </summary>
    public void GoToLastTutorial()
    {
        if (totalTutorials > 0)
        {
            LoadTutorial(totalTutorials);
        }
    }

    /// <summary>
    /// Exit tutorial mode (deactivate all tutorials)
    /// </summary>
    public void ExitTutorial()
    {
        DeactivateAllTutorials();
        Debug.Log("Tutorial mode exited");
    }
}
