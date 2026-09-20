using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("Breach Effect")]
    public bool breachActive = false;

    [Header("Room Shake Strength")]
    public float mainRoomShakeAmount = 0.15f;
    public float otherRoomShakeAmount = 0.05f;

    [Header("Shake Settings")]
    public float shakeSpeed = 15f;

    [Header("Rotation Distortion")]
    public float rotationDistortion = 2f;

    private float currentShakeAmount;
    private float noiseTime;

    private Vector3 originalLocalPosition;

    private CameraController cameraController;

    void Start()
    {
        currentShakeAmount = mainRoomShakeAmount;

        cameraController = GetComponent<CameraController>();
    }

    void Update()
    {
        // When there is no active breach, restore the camera position
        // and remove any rotation added by the shake effect.
        if (!breachActive)
        {
            transform.localPosition = originalLocalPosition;

            if (cameraController != null)
            {
                cameraController.ResetShakeRotation();
            }

            return;
        }

        // Increase the noise input over time so the shake continuously changes.
        noiseTime += Time.deltaTime * shakeSpeed;

        // Perlin Noise produces smooth pseudo-random values.
        // This gives the camera a more natural shake than completely random movement.
        float x =
            (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * 2f;

        float y =
            (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * 2f;

        // Convert the noise values into a local position offset.
        Vector3 shakeOffset =
            new Vector3(x, y, 0f) * currentShakeAmount;

        transform.localPosition =
            originalLocalPosition + shakeOffset;

        // Convert the same noise into small rotation changes.
        float rotationX =
            y * rotationDistortion;

        float rotationZ =
            x * rotationDistortion;

        Quaternion shakeRotation =
            Quaternion.Euler(
                rotationX,
                0f,
                rotationZ
            );

        // Send the additional rotation to CameraController
        // so the breach effect does not overwrite the player's mouse look.
        if (cameraController != null)
        {
            cameraController.SetShakeRotation(shakeRotation);
        }
    }

    public void StartBreachEffect()
    {
        breachActive = true;
        noiseTime = 0f;

        // Store the current position so the camera can return to it
        // after the breach effect finishes.
        originalLocalPosition =
            transform.localPosition;
    }

    public void StopBreachEffect()
    {
        breachActive = false;

        transform.localPosition =
            originalLocalPosition;

        if (cameraController != null)
        {
            cameraController.ResetShakeRotation();
        }
    }

    public void SetMainRoomShake()
    {
        currentShakeAmount =
            mainRoomShakeAmount;
    }

    public void SetOtherRoomShake()
    {
        currentShakeAmount =
            otherRoomShakeAmount;
    }

    public void ResetEffectTransform()
    {
        breachActive = false;
        noiseTime = 0f;

        transform.localPosition =
            Vector3.zero;

        if (cameraController != null)
        {
            cameraController.ResetShakeRotation();
        }

        originalLocalPosition =
            Vector3.zero;
    }
}