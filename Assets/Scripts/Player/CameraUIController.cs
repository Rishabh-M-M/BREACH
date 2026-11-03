
using UnityEngine;
using System.Collections;
using TMPro;

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

    [Header("Room Description UI")]
    [SerializeField] private TMP_Text passiveAbilityText;
    [SerializeField] private TMP_Text breachedDescriptionText;

    [Header("Room Description Colours")]
    [SerializeField]
    private Color passiveHeadingColour =
        new Color32(0, 255, 255, 255);

    [SerializeField]
    private Color passiveDescriptionColour =
        new Color32(80, 255, 190, 255);

    [SerializeField]
    private Color breachedHeadingColour =
        new Color32(255, 105, 45, 255);

    [SerializeField]
    private Color breachedDescriptionColour =
        new Color32(255, 145, 75, 255);

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
                Mathf.Clamp01(timer / animationDuration);

            float smoothProgress =
                1f - Mathf.Pow(1f - progress, 3f);

            float scale =
                Mathf.Lerp(startingScale, 1f, smoothProgress);

            float alpha =
                Mathf.Lerp(0f, 1f, smoothProgress);

            cameraScreenRect.localScale =
                Vector3.one * scale;

            cameraScreenCanvasGroup.alpha = alpha;

            yield return null;
        }

        cameraScreenRect.localScale = Vector3.one;
        cameraScreenCanvasGroup.alpha = 1f;

        cameraScreenCanvasGroup.interactable = true;
        cameraScreenCanvasGroup.blocksRaycasts = true;

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
            cameraScreenRect.localScale = Vector3.one;

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
    // ROOM DESCRIPTIONS
    // =========================================================

    private void UpdateRoomDescription(int roomIndex)
    {
        string passive = "";
        string breached = "";

        switch (roomIndex)
        {
            case 1:
                passive =
                    "Enables the minimap and provides breach notifications through alarms and additional information.";

                breached =
                    "Minimap and breach notifications are disabled.";
                break;

            case 2:
                passive =
                    "Gradually regenerates barrier health across all rooms.";

                breached =
                    "Ship-wide barrier regeneration stops. Destroyed barriers cannot be restored.";
                break;

            case 3:
                passive =
                    "Reduces power costs for turret upgrades and trap activations.";

                breached =
                    "Turret upgrades and trap activations may cost more power.";
                break;

            case 4:
                passive =
                    "Increases maximum power capacity over time.";

                breached =
                    "Maximum power capacity growth stops.";
                break;

            default:
                if (passiveAbilityText != null)
                    passiveAbilityText.text = "";

                if (breachedDescriptionText != null)
                    breachedDescriptionText.text = "";

                return;
        }

        if (passiveAbilityText != null)
        {
            passiveAbilityText.text =
                $"<color=#{ColorUtility.ToHtmlStringRGB(passiveHeadingColour)}>PASSIVE ABILITY</color>\n" +
                $"<color=#{ColorUtility.ToHtmlStringRGB(passiveDescriptionColour)}>{passive}</color>";
        }

        if (breachedDescriptionText != null)
        {
            breachedDescriptionText.text =
                $"<color=#{ColorUtility.ToHtmlStringRGB(breachedHeadingColour)}>WHEN BREACHED</color>\n" +
                $"<color=#{ColorUtility.ToHtmlStringRGB(breachedDescriptionColour)}>{breached}</color>";
        }
    }

    // =========================================================
    // ROOM 1
    // =========================================================

    public void ShowRoom1()
    {
        currentViewedRoomIndex = 1;

        UpdateRoomDescription(1);

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

        UpdateRoomDescription(2);

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

        UpdateRoomDescription(3);

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

        UpdateRoomDescription(4);

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
                roomName + " Camera Point is not assigned."
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
            "Showing " + roomName + " camera."
        );
    }

    public void ShowRoom(int roomIndex)
    {
        switch (roomIndex)
        {
            case 1: ShowRoom1(); break;
            case 2: ShowRoom2(); break;
            case 3: ShowRoom3(); break;
            case 4: ShowRoom4(); break;

            default:
                Debug.LogWarning(
                    $"[CameraUIController] ShowRoom called with invalid room index {roomIndex}."
                );
                break;
        }
    }

    public Transform GetRoomCameraPoint(int roomIndex)
    {
        switch (roomIndex)
        {
            case 1: return room1CameraPoint;
            case 2: return room2CameraPoint;
            case 3: return room3CameraPoint;
            case 4: return room4CameraPoint;

            default: return null;
        }
    }
}
