using UnityEngine;

public class RoomCameraSwitcher : MonoBehaviour
{
    [Header("Camera Points")]
    public Transform mainRoomCameraPoint;
    public Transform otherRoomCameraPoint;

    [Header("Camera")]
    public Transform cameraRig;
    public CameraController cameraController;

    [Header("Camera Effects")]
    public CameraEffects cameraEffects;

    void Start()
    {
        // Start the player at the main-room camera position.
        cameraRig.position =
            mainRoomCameraPoint.position;

        cameraRig.rotation =
            mainRoomCameraPoint.rotation;

        cameraController.ResetLook();
        cameraController.enabled = true;

        if (cameraEffects != null)
        {
            cameraEffects.SetMainRoomShake();

            // Do not cancel an active breach effect during setup.
            if (!cameraEffects.breachActive)
            {
                cameraEffects.ResetEffectTransform();
            }
        }
    }

    void Update()
    {
        // M returns to the main room.
        if (Input.GetKeyDown(KeyCode.M))
        {
            GoToMainRoom();
        }

        // R switches to the other room.
        if (Input.GetKeyDown(KeyCode.R))
        {
            GoToOtherRoom();
        }
    }

    public void GoToMainRoom()
    {
        cameraRig.position =
            mainRoomCameraPoint.position;

        cameraRig.rotation =
            mainRoomCameraPoint.rotation;

        cameraController.ResetLook();
        cameraController.enabled = true;

        if (cameraEffects != null)
        {
            cameraEffects.SetMainRoomShake();

            // Important: changing rooms must not stop an active breach.
            if (!cameraEffects.breachActive)
            {
                cameraEffects.ResetEffectTransform();
            }
        }
    }

    public void GoToOtherRoom()
    {
        cameraRig.position =
            otherRoomCameraPoint.position;

        cameraRig.rotation =
            otherRoomCameraPoint.rotation;

        cameraController.ResetLook();

        // The other room is intended to be a fixed viewpoint,
        // so mouse look is disabled there.
        cameraController.enabled = false;

        if (cameraEffects != null)
        {
            cameraEffects.SetOtherRoomShake();

            // Important: changing rooms must not stop an active breach.
            if (!cameraEffects.breachActive)
            {
                cameraEffects.ResetEffectTransform();
            }
        }
    }
}