using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility class for loading scenes and exiting the application
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by its build index number
    /// </summary>
    /// <param name="sceneIndex">Index of the scene in Build Settings</param>
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Exits the application (quits the game)
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
