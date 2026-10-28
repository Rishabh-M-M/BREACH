
using TMPro;
using UnityEngine;

public class CameraNumberUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cameraNumberText;

    public void SetCameraNumber(int roomNumber)
    {
        if (cameraNumberText == null)
            return;

        cameraNumberText.text = $"CAM {roomNumber:00}";
    }
}
