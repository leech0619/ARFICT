using UnityEngine;

public class SpinTarget : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 90f; // Degrees per second
    
    void Update()
    {
        // Rotate the object around its Y-axis
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }
    
    /// <summary>
    /// Set new spin speed
    /// </summary>
    /// <param name="newSpeed">New rotation speed in degrees per second</param>
    public void SetSpinSpeed(float newSpeed)
    {
        spinSpeed = newSpeed;
    }
}
