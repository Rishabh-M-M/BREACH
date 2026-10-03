using UnityEngine;

public class CameraUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainHUD;
    public GameObject cameraScreen;
    public GameObject power;
    public GameObject equipmentPanel;

    [Header("Recording UI")]
    public CameraRecordingUI cameraRecordingUI;

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
        if (mainHUD != null)
            mainHUD.SetActive(true);

        if (cameraScreen != null)
            cameraScreen.SetActive(false);

        if (power != null)
            power.SetActive(true);

        // No room selected initially
        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        if (mainCamera != null)
            mainCamera.enabled = true;

        if (securityCamera != null)
        {
            securityCamera.enabled = false;
            securityCamera.cullingMask = 0;
        }

        if (backgroundCamera != null)
            backgroundCamera.enabled = false;

        // Set default REC display
        if (cameraRecordingUI != null)
            cameraRecordingUI.StopRecording();
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

        if (power != null)
        {
            power.SetActive(true);
            power.transform.SetAsLastSibling();
        }

        // No room selected yet
        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        // IMPORTANT:
        // Do NOT reset or stop the recording timer here.
        // The timer keeps its previous time.
        //
        // Recording only continues when a room is selected.

        if (mainCamera != null)
            mainCamera.enabled = false;

        if (securityCamera != null)
        {
            securityCamera.enabled = true;
            securityCamera.cullingMask = 0;
        }

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

        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        // IMPORTANT:
        // Stop counting, but keep the recorded time.
        if (cameraRecordingUI != null)
            cameraRecordingUI.StopRecording();

        if (securityCamera != null)
        {
            securityCamera.enabled = false;
            securityCamera.cullingMask = 0;
        }

        if (backgroundCamera != null)
            backgroundCamera.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;

        if (cameraController != null)
            cameraController.ResetLook();

        if (roomCameraSwitcher != null)
            roomCameraSwitcher.GoToMainRoom();
    }

    public void ShowRoom1()
    {
        if (securityCamera == null)
        {
            Debug.LogWarning("Security Camera is not assigned.");
            return;
        }

        if (room1CameraPoint == null)
        {
            Debug.LogWarning("Room 1 Camera Point is not assigned.");
            return;
        }

        securityCamera.transform.position =
            room1CameraPoint.position;

        securityCamera.transform.rotation =
            room1CameraPoint.rotation;

        securityCamera.cullingMask = -1;

        // Show equipment after selecting Room 1
        if (equipmentPanel != null)
            equipmentPanel.SetActive(true);

        // Start/continue recording
        if (cameraRecordingUI != null)
            cameraRecordingUI.StartRecording();

        Debug.Log("Showing Room 1 camera.");
    }
}