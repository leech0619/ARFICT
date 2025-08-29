using UnityEngine;

/// <summary>
/// Prevents the device screen from dimming or going to sleep during navigation
/// </summary>
public class KeepScreenAlive : MonoBehaviour {

    private void Start() {
        // Disable screen dimming and sleep to keep display always on
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}