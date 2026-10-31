namespace ProjectBreach.Boss
{
    /// <summary>
    /// Sits over the equipment panel's turret slots. Not fixed to one room -
    /// the same four slots get reassigned to whichever room is currently
    /// displayed (EquipmentPanelManager.ShowRoomX), so this tracks
    /// CameraUIController.CurrentViewedRoomIndex instead of a fixed index.
    /// </summary>
    public class TurretJamCoverUI : JamCoverUI
    {
        protected override int GetRoomIndexToCheck() => CameraUIController.CurrentViewedRoomIndex;
        protected override bool IsJammed(RoomJamState state) => state.TurretsLocked;

        protected override void RegisterClick(RoomJamState state)
        {
            BossJamManager manager = BossJamManager.Instance;
            if (manager == null) return;

            state.RegisterTurretClick(manager.ClickAmountPerPress, manager.ClicksRequiredToClear);
        }
    }
}