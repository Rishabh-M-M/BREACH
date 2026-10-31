using UnityEngine;
using UnityEngine.UI;

namespace ProjectBreach.Boss
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CameraFeedJamOverlay : MonoBehaviour
    {
        [Header("Static Effect")]
        [SerializeField] private RawImage staticImage;
        [SerializeField] private float uvJitterAmount = 0.5f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            bool jammed = IsCurrentRoomCameraJammed();

            canvasGroup.alpha = jammed ? 1f : 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (jammed && staticImage != null)
            {
                staticImage.uvRect = new Rect(
                    Random.Range(-uvJitterAmount, uvJitterAmount),
                    Random.Range(-uvJitterAmount, uvJitterAmount),
                    1f, 1f
                );
            }
        }

        private bool IsCurrentRoomCameraJammed()
        {
            int roomIndex = CameraUIController.CurrentViewedRoomIndex;
            if (roomIndex <= 0 || BossJamManager.Instance == null) return false;

            RoomJamState state = BossJamManager.Instance.GetState(roomIndex);
            return state != null && state.CameraJammed;
        }
    }
}