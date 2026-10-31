using UnityEngine;
using UnityEngine.UI;

namespace ProjectBreach.Boss
{
    /// <summary>
    /// Base for a UI element that covers a real button while the boss has
    /// jammed whatever that button controls. While active, this sits on top
    /// of the real button in the hierarchy so it intercepts the click
    /// instead of the button underneath; clicking it registers progress
    /// toward clearing the jam, and once cleared it deactivates itself,
    /// revealing the real button again.
    ///
    /// Subclasses only say which room to check and how a click counts -
    /// showing/hiding and the Button wiring live here.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public abstract class JamCoverUI : MonoBehaviour
    {
        private Button button;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnCoverClicked);
        }

        protected virtual void Update()
        {
            RoomJamState state = GetState();
            bool jammed = state != null && IsJammed(state);

            if (gameObject.activeSelf != jammed)
            {
                gameObject.SetActive(jammed);
            }
        }

        private void OnCoverClicked()
        {
            RoomJamState state = GetState();
            if (state != null) RegisterClick(state);
        }

        private RoomJamState GetState()
        {
            int roomIndex = GetRoomIndexToCheck();
            if (roomIndex <= 0 || BossJamManager.Instance == null) return null;

            return BossJamManager.Instance.GetState(roomIndex);
        }

        protected abstract int GetRoomIndexToCheck();
        protected abstract bool IsJammed(RoomJamState state);
        protected abstract void RegisterClick(RoomJamState state);
    }
}