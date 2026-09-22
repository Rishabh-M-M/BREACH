using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    public float sensitivity = 3f;

    [Header("Camera Limits")]
    public float maxLeft = -45f;
    public float maxRight = 45f;
    public float maxUp = 25f;
    public float maxDown = -25f;

    private float yaw;
    private float pitch;

    // Rotation supplied by CameraEffects during a breach.
    // Keeping it separate allows mouse movement and shaking
    // to be combined without either system overwriting the other.
    private Quaternion shakeRotation = Quaternion.identity;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * sensitivity;
        pitch -= mouseY * sensitivity;

        yaw = Mathf.Clamp(yaw, maxLeft, maxRight);
        pitch = Mathf.Clamp(pitch, maxDown, maxUp);

        Quaternion playerRotation =
            Quaternion.Euler(pitch, yaw, 0f);

        // Combine normal player look with the additional
        // breach rotation supplied by CameraEffects.
        transform.localRotation =
            playerRotation * shakeRotation;
    }

    public void SetShakeRotation(Quaternion rotation)
    {
        shakeRotation = rotation;
    }

    public void ResetShakeRotation()
    {
        shakeRotation = Quaternion.identity;
    }

    public void ResetLook()
    {
        yaw = 0f;
        pitch = 0f;
        shakeRotation = Quaternion.identity;

        transform.localRotation =
            Quaternion.identity;
    }
}