using UnityEngine;

namespace ProjectBreach.Boss
{
    public class CameraJamCoverUI : JamCoverUI
    {
        [SerializeField] private int roomIndex;
        [SerializeField] private CameraUIController cameraUIController;

        protected override int GetRoomIndexToCheck() => roomIndex;
        protected override bool IsJammed(RoomJamState state) => state.CameraJammed;
        protected override float GetProgress(RoomJamState state) => state.CameraClearProgress;

        protected override void RegisterClick(RoomJamState state)
        {
            if (cameraUIController != null)
            {
                cameraUIController.ShowRoom(roomIndex);
            }
            else
            {
                Debug.LogWarning("[CameraJamCoverUI] CameraUIController reference not assigned; clicking the cover won't switch the view.");
            }

            BossJamManager manager = BossJamManager.Instance;
            if (manager == null) return;

            state.RegisterCameraClick(manager.ClickAmountPerPress, manager.ClicksRequiredToClear);
        }
    }
}