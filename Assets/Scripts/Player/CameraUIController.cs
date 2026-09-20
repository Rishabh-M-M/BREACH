using UnityEngine;

public class CameraUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainHUD;
    public GameObject cameraScreen;
    public GameObject power;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera securityCamera;

    [Header("Background Camera")]
    public Camera backgroundCamera;

    [Header("Player Camera")]
    public CameraController cameraController;

    [Header("Room Switching")]
    public RoomCameraSwitcher roomCameraSwitcher;

    [Header("Room Camera Points")]
    public Transform room1CameraPoint;

    void Start()
    {
        // Normal gameplay starts with the HUD visible
        // and the camera screen hidden.
        if (mainHUD != null)
            mainHUD.SetActive(true);

        if (cameraScreen != null)
            cameraScreen.SetActive(false);

        // Power remains visible in both gameplay and camera mode.
        if (power != null)
            power.SetActive(true);

        // Main gameplay camera starts active.
        if (mainCamera != null)
            mainCamera.enabled = true;

        // Security camera starts disabled and renders nothing
        // until a room is selected.
        if (securityCamera != null)
        {
            securityCamera.enabled = false;
            securityCamera.cullingMask = 0;
        }

        // The background camera is only needed while viewing cameras.
        if (backgroundCamera != null)
            backgroundCamera.enabled = false;
    }

    public void OpenCameraScreen()
    {
        if (mainHUD != null)
            mainHUD.SetActive(false);

        if (cameraScreen != null)
        {
            cameraScreen.SetActive(true);
            cameraScreen.transform.SetAsLastSibling();
        }

        // Keep Power above the camera screen.
        if (power != null)
        {
            power.SetActive(true);
            power.transform.SetAsLastSibling();
        }

        // Replace the normal gameplay camera with the camera interface.
        if (mainCamera != null)
            mainCamera.enabled = false;

        if (securityCamera != null)
        {
            securityCamera.enabled = true;

            // No room is selected initially, so don't render
            // any 3D scene into the camera feed.
            securityCamera.cullingMask = 0;
        }

        // Render the plain background while no room is selected.
        if (backgroundCamera != null)
            backgroundCamera.enabled = true;
    }

    public void CloseCameraScreen()
    {
        if (cameraScreen != null)
            cameraScreen.SetActive(false);

        if (mainHUD != null)
            mainHUD.SetActive(true);

        if (power != null)
            power.SetActive(true);

        if (securityCamera != null)
        {
            securityCamera.enabled = false;
            securityCamera.cullingMask = 0;
        }

        if (backgroundCamera != null)
            backgroundCamera.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;

        // Return the player camera to its default orientation.
        if (cameraController != null)
            cameraController.ResetLook();

        // Return to the main room when leaving the camera interface.
        if (roomCameraSwitcher != null)
            roomCameraSwitcher.GoToMainRoom();
    }

    public void ShowRoom1()
    {
        if (securityCamera == null)
        {
            Debug.LogWarning(
                "Security Camera is not assigned."
            );
            return;
        }

        if (room1CameraPoint == null)
        {
            Debug.LogWarning(
                "Room 1 Camera Point is not assigned."
            );
            return;
        }

        // Move the security camera to the selected room's viewpoint.
        securityCamera.transform.position =
            room1CameraPoint.position;

        securityCamera.transform.rotation =
            room1CameraPoint.rotation;

        // Allow the security camera to render the room.
        securityCamera.cullingMask = -1;

        Debug.Log("Showing Room 1 camera.");
    }
}