using UnityEngine;
using TMPro;

public class CameraRecordingUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI recText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 0.5f;

    private float recordingTime;
    private bool recording;
    private float blinkTimer;
    private bool dotVisible;

    private void Start()
    {
        // Default display.
        // Not recording yet.
        recording = false;
        recordingTime = 0f;

        if (timerText != null)
            timerText.text = "00:00:000";

        dotVisible = true;
        UpdateRECText();
    }

    private void Update()
    {
        if (!recording)
            return;

        // Keep counting while recording
        recordingTime += Time.deltaTime;

        // Display MM:SS:MS
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(recordingTime / 60f);
            int seconds = Mathf.FloorToInt(recordingTime % 60f);
            int milliseconds = Mathf.FloorToInt(
                (recordingTime * 1000f) % 1000f
            );

            timerText.text =
                $"{minutes:00}:{seconds:00}:{milliseconds:000}";
        }

        // Blink the dot
        blinkTimer += Time.deltaTime;

        if (blinkTimer >= blinkSpeed)
        {
            blinkTimer = 0f;
            dotVisible = !dotVisible;

            UpdateRECText();
        }
    }

    public void StartRecording()
    {
        // Continue from the existing time.
        // Do NOT reset recordingTime here.
        recording = true;

        blinkTimer = 0f;
        dotVisible = true;

        UpdateRECText();
    }

    public void StopRecording()
    {
        // Stop counting but KEEP the current time.
        recording = false;

        // Keep the dot visible when not recording.
        dotVisible = true;

        UpdateRECText();
    }

    private void UpdateRECText()
    {
        if (recText == null)
            return;

        if (dotVisible)
        {
            // Red dot is visible.
            recText.text =
                "<color=#FF3333>●</color> REC";
        }
        else
        {
            // Keep the exact same space for the dot.
            // Only make it transparent.
            recText.text =
                "<color=#FF333300>●</color> REC";
        }
    }
}