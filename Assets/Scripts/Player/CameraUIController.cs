using UnityEngine;
using System.Collections;

public class CameraUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainHUD;
    public GameObject cameraScreen;
    public GameObject power;
    public GameObject equipmentPanel;

    [Header("Equipment Panel")]
    public EquipmentPanelManager equipmentPanelManager;

    [Header("Glitch Transition")]
    public GameObject glitchOverlay;

    [SerializeField] private float glitchDuration = 0.4f;

    [Header("Glitch Shake")]
    [SerializeField] private float glitchShakeX = 10f;
    [SerializeField] private float glitchShakeY = 5f;

    [Header("Camera Animation")]
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private float startingScale = 0.85f;

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
    public Transform room2CameraPoint;
    public Transform room3CameraPoint;
    public Transform room4CameraPoint;

    private RectTransform cameraScreenRect;
    private CanvasGroup cameraScreenCanvasGroup;
    private CanvasGroup glitchCanvasGroup;

    private bool isAnimating = false;

    // 0 = Main Control Room, 1-4 = Room 1-4.
    // Turret audio uses this to determine which room's sounds are audible.
    private static int currentViewedRoomIndex = 0;
    private static bool securityCameraOpen = false;

    public static int CurrentViewedRoomIndex => currentViewedRoomIndex;
    public static bool IsSecurityCameraOpen => securityCameraOpen;

    public static bool IsRoomCurrentlyViewed(int roomIndex)
    {
        return currentViewedRoomIndex == roomIndex;
    }

    void Start()
    {
        currentViewedRoomIndex = 0;
        securityCameraOpen = false;

        if (cameraScreen != null)
        {
            cameraScreenRect =
                cameraScreen.GetComponent<RectTransform>();

            cameraScreenCanvasGroup =
                cameraScreen.GetComponent<CanvasGroup>();

            if (cameraScreenCanvasGroup == null)
            {
                cameraScreenCanvasGroup =
                    cameraScreen.AddComponent<CanvasGroup>();
            }

            cameraScreenRect.localScale =
                Vector3.one * startingScale;

            cameraScreenCanvasGroup.alpha = 0f;
            cameraScreenCanvasGroup.interactable = false;
            cameraScreenCanvasGroup.blocksRaycasts = false;

            cameraScreen.SetActive(false);
        }

        if (glitchOverlay != null)
        {
            glitchCanvasGroup =
                glitchOverlay.GetComponent<CanvasGroup>();

            if (glitchCanvasGroup == null)
            {
                glitchCanvasGroup =
                    glitchOverlay.AddComponent<CanvasGroup>();
            }

            glitchCanvasGroup.alpha = 0f;
            glitchCanvasGroup.interactable = false;
            glitchCanvasGroup.blocksRaycasts = false;

            glitchOverlay.SetActive(false);
        }

        if (mainHUD != null)
            mainHUD.SetActive(true);

        if (power != null)
            power.SetActive(true);

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

        if (cameraRecordingUI != null)
            cameraRecordingUI.StopRecording();
    }

    public void OpenCameraScreen()
    {
        if (isAnimating)
            return;

        securityCameraOpen = true;

        if (mainHUD != null)
            mainHUD.SetActive(false);

        if (cameraScreen != null)
        {
            cameraScreen.SetActive(true);

            cameraScreen.transform.SetAsLastSibling();

            cameraScreenRect.localScale =
                Vector3.one * startingScale;

            cameraScreenCanvasGroup.alpha = 0f;

            StartCoroutine(AnimateCameraOpen());
        }

        if (power != null)
        {
            power.SetActive(true);
            power.transform.SetAsLastSibling();
        }

        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

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

    private IEnumerator AnimateCameraOpen()
    {
        isAnimating = true;

        float timer = 0f;

        cameraScreenCanvasGroup.interactable = false;
        cameraScreenCanvasGroup.blocksRaycasts = false;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / animationDuration
                );

            float smoothProgress =
                1f - Mathf.Pow(
                    1f - progress,
                    3f
                );

            float scale =
                Mathf.Lerp(
                    startingScale,
                    1f,
                    smoothProgress
                );

            float alpha =
                Mathf.Lerp(
                    0f,
                    1f,
                    smoothProgress
                );

            cameraScreenRect.localScale =
                Vector3.one * scale;

            cameraScreenCanvasGroup.alpha =
                alpha;

            yield return null;
        }

        cameraScreenRect.localScale =
            Vector3.one;

        cameraScreenCanvasGroup.alpha =
            1f;

        cameraScreenCanvasGroup.interactable =
            true;

        cameraScreenCanvasGroup.blocksRaycasts =
            true;

        isAnimating = false;
    }

    public void CloseCameraScreen()
    {
        if (isAnimating)
            return;

        // Once the security camera is closed, the player is back in the
        // Main Control Room view.
        currentViewedRoomIndex = 0;
        securityCameraOpen = false;

        if (cameraRecordingUI != null)
            cameraRecordingUI.StopRecording();

        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        StartCoroutine(GlitchAndClose());
    }

    private IEnumerator GlitchAndClose()
    {
        isAnimating = true;

        if (cameraScreenCanvasGroup != null)
        {
            cameraScreenCanvasGroup.alpha = 1f;
            cameraScreenCanvasGroup.interactable = false;
            cameraScreenCanvasGroup.blocksRaycasts = false;
        }

        Vector3 originalCameraScreenPosition =
            cameraScreen.transform.localPosition;

        if (glitchOverlay != null)
        {
            glitchOverlay.SetActive(true);

            glitchCanvasGroup.alpha = 1f;

            glitchOverlay.transform.SetAsLastSibling();

            if (power != null)
                power.transform.SetAsLastSibling();
        }

        float timer = 0f;

        while (timer < glitchDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (cameraScreen != null)
            {
                float shakeX =
                    Random.Range(-glitchShakeX, glitchShakeX);

                float shakeY =
                    Random.Range(-glitchShakeY, glitchShakeY);

                cameraScreen.transform.localPosition =
                    originalCameraScreenPosition +
                    new Vector3(shakeX, shakeY, 0f);
            }

            if (glitchCanvasGroup != null)
                glitchCanvasGroup.alpha = 1f;

            yield return null;
        }

        if (cameraScreen != null)
        {
            cameraScreen.transform.localPosition =
                originalCameraScreenPosition;
        }

        if (cameraScreen != null)
            cameraScreen.SetActive(false);

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

        if (glitchCanvasGroup != null)
            glitchCanvasGroup.alpha = 0f;

        if (glitchOverlay != null)
            glitchOverlay.SetActive(false);

        if (cameraScreenRect != null)
            cameraScreenRect.localScale =
                Vector3.one;

        if (cameraScreenCanvasGroup != null)
        {
            cameraScreenCanvasGroup.alpha = 0f;
            cameraScreenCanvasGroup.interactable = false;
            cameraScreenCanvasGroup.blocksRaycasts = false;
        }

        if (mainHUD != null)
            mainHUD.SetActive(true);

        if (power != null)
            power.SetActive(true);

        isAnimating = false;
    }

    // =========================================================
    // ROOM 1
    // =========================================================

    public void ShowRoom1()
    {
        currentViewedRoomIndex = 1;

        ShowRoom(
            room1CameraPoint,
            "Room 1"
        );

        if (equipmentPanelManager != null)
            equipmentPanelManager.ShowRoom1();
    }

    // =========================================================
    // ROOM 2
    // =========================================================

    public void ShowRoom2()
    {
        currentViewedRoomIndex = 2;

        ShowRoom(
            room2CameraPoint,
            "Room 2"
        );

        if (equipmentPanelManager != null)
            equipmentPanelManager.ShowRoom2();
    }

    // =========================================================
    // ROOM 3
    // =========================================================

    public void ShowRoom3()
    {
        currentViewedRoomIndex = 3;

        ShowRoom(
            room3CameraPoint,
            "Room 3"
        );

        if (equipmentPanelManager != null)
            equipmentPanelManager.ShowRoom3();
    }

    // =========================================================
    // ROOM 4
    // =========================================================

    public void ShowRoom4()
    {
        currentViewedRoomIndex = 4;

        ShowRoom(
            room4CameraPoint,
            "Room 4"
        );

        if (equipmentPanelManager != null)
            equipmentPanelManager.ShowRoom4();
    }

    // =========================================================
    // COMMON ROOM CAMERA FUNCTION
    // =========================================================

    private void ShowRoom(
        Transform roomCameraPoint,
        string roomName)
    {
        if (securityCamera == null)
        {
            Debug.LogWarning(
                "Security Camera is not assigned."
            );

            return;
        }

        if (roomCameraPoint == null)
        {
            Debug.LogWarning(
                roomName +
                " Camera Point is not assigned."
            );

            return;
        }

        securityCamera.transform.position =
            roomCameraPoint.position;

        securityCamera.transform.rotation =
            roomCameraPoint.rotation;

        securityCamera.cullingMask = -1;

        if (equipmentPanel != null)
            equipmentPanel.SetActive(true);

        if (cameraRecordingUI != null)
            cameraRecordingUI.StartRecording();

        Debug.Log(
            "Showing " +
            roomName +
            " camera."
        );
    }
}