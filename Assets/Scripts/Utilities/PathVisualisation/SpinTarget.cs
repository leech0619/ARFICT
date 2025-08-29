using UnityEngine;

/// <summary>
/// Makes an object continuously rotate around its Y-axis
/// </summary>
public class SpinTarget : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 90f; // Rotation speed in degrees per second
    
    void Update()
    {
        // Continuously rotate around Y-axis
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }
    
    /// <summary>
    /// Changes the rotation speed of the spinning object
    /// </summary>
    /// <param name="newSpeed">New rotation speed in degrees per second</param>
    public void SetSpinSpeed(float newSpeed)
    {
        spinSpeed = newSpeed;
    }
}
