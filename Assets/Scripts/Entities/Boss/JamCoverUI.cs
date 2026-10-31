using UnityEngine;
using UnityEngine.UI;

namespace ProjectBreach.Boss
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class JamCoverUI : MonoBehaviour
    {
        private Button button;
        private CanvasGroup canvasGroup;
        private bool lastJammedState;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            canvasGroup = GetComponent<CanvasGroup>();
            button.onClick.AddListener(OnCoverClicked);
        }

        protected virtual void Update()
        {
            RoomJamState state = GetState();
            bool jammed = state != null && IsJammed(state);

            if (jammed != lastJammedState)
            {
                Debug.Log($"[{GetType().Name}] on {name}: jammed changed to {jammed} (room {GetRoomIndexToCheck()}).");
                lastJammedState = jammed;
            }

            SetVisible(jammed);
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void OnCoverClicked()
        {
            RoomJamState state = GetState();
            if (state == null) return;

            RegisterClick(state);

            BossJamManager manager = BossJamManager.Instance;
            float threshold = manager != null ? manager.ClicksRequiredToClear : 0f;

            Debug.Log($"[{GetType().Name}] on {name}: click registered for room {GetRoomIndexToCheck()}. Progress: {GetProgress(state)}/{threshold}, Jammed: {IsJammed(state)}");
        }

        private RoomJamState GetState()
        {
            int roomIndex = GetRoomIndexToCheck();
            if (roomIndex <= 0 || BossJamManager.Instance == null) return null;

            return BossJamManager.Instance.GetState(roomIndex);
        }

        protected abstract int GetRoomIndexToCheck();
        protected abstract bool IsJammed(RoomJamState state);
        protected abstract float GetProgress(RoomJamState state);
        protected abstract void RegisterClick(RoomJamState state);
    }
}