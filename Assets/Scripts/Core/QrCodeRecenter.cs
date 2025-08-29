// Description: This script is responsible for scanning QR codes and recentering the AR session based on the QR code's target text.
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using ZXing;

/// <summary>
/// Scans QR codes and recenters AR session to target locations
/// </summary>
public class QrCodeRecenter : MonoBehaviour
{
    // Component references
    [SerializeField]
    private ARSession session; // AR session for reset functionality
    [SerializeField]
    private XROrigin sessionOrigin; // AR origin to reposition during recentering
    [SerializeField]
    private ARCameraManager cameraManager; // Camera manager for frame capture
    [SerializeField]
    private TargetHandler targetHandler; // Handler to find targets by QR code text
    [SerializeField]
    private GameObject qrCodeScanningPanel; // UI panel shown during QR scanning

    // QR code scanning variables
    private Texture2D cameraImageTexture; // Texture for processing camera frames
    private IBarcodeReader reader = new BarcodeReader(); // ZXing QR code reader
    private bool scanningEnabled = false; // Controls whether QR scanning is active

    private void Update()
    {
        // Debug: Space key for testing QR code recentering without actual QR scan
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetQrCodeRecenterTarget("MainEntrance");
        }
    }

    private void OnEnable()
    {
        // Subscribe to camera frame events for QR code detection
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        // Unsubscribe from camera frame events to prevent memory leaks
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    /// <summary>
    /// Processes camera frames to detect QR codes when scanning is enabled
    /// </summary>
    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!scanningEnabled) return;

        // Try to get latest camera image for QR code detection
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        // Setup image conversion for QR detection
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2), // Downsample for performance
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        int size = image.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Convert camera image to texture for QR scanning
        image.Convert(conversionParams, buffer);
        image.Dispose();

        cameraImageTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();
        buffer.Dispose();

        // Attempt QR code detection in the processed image
        var result = reader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);

        if (result != null)
        {
            SetQrCodeRecenterTarget(result.Text);
            ToggleScanning(); // Stop scanning after successful detection
        }
    }

    /// <summary>
    /// Recenters AR session to the target location specified by QR code text
    /// </summary>
    private void SetQrCodeRecenterTarget(string targetText)
    {
        Target currentTarget = targetHandler.GetCurrentTargetByTargetText(targetText);
        if (currentTarget != null)
        {
            // Reset AR session and move origin to target location
            session.Reset();
            sessionOrigin.transform.position = currentTarget.PositionObject.transform.position;
            sessionOrigin.transform.rotation = currentTarget.PositionObject.transform.rotation;
        }
    }

    /// <summary>
    /// Toggles QR code scanning on/off and shows/hides scanning UI
    /// </summary>
    public void ToggleScanning()
    {
        scanningEnabled = !scanningEnabled;
        qrCodeScanningPanel.SetActive(scanningEnabled);
    }
}