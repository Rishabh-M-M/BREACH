using System;

namespace ProjectBreach.Boss
{
    /// <summary>
    /// Tracks the boss's jam state for a single room: whether its camera feed
    /// and/or turret group is currently compromised, and how much progress has
    /// been made clearing each. This class only owns state - it doesn't know
    /// about UI covers, audio, the boss decoy model, or the timer that decides
    /// when a jam starts. Everything else reacts to the events below.
    /// </summary>
    public class RoomJamState
    {
        public int RoomIndex { get; }

        public bool CameraJammed { get; private set; }
        public bool TurretsLocked { get; private set; }

        private float cameraClearProgress;
        private float turretClearProgress;

        public event Action<int> OnCameraJamStarted;
        public event Action<int> OnCameraJamCleared;
        public event Action<int> OnTurretJamStarted;
        public event Action<int> OnTurretJamCleared;
        public float CameraClearProgress => cameraClearProgress;
        public float TurretClearProgress => turretClearProgress;

        public RoomJamState(int roomIndex)
        {
            RoomIndex = roomIndex;
        }

        public void StartCameraJam()
        {
            if (CameraJammed) return;

            CameraJammed = true;
            cameraClearProgress = 0f;
            OnCameraJamStarted?.Invoke(RoomIndex);
        }

        public void StartTurretJam()
        {
            if (TurretsLocked) return;

            TurretsLocked = true;
            turretClearProgress = 0f;
            OnTurretJamStarted?.Invoke(RoomIndex);
        }

        // amount is typically 1 per click; clearThreshold is how many clicks
        // the cover UI requires - kept here as a parameter rather than a
        // hardcoded constant so it stays a tunable value on whatever
        // component calls this, for playtesting.
        public void RegisterCameraClick(float amount, float clearThreshold)
        {
            if (!CameraJammed) return;

            cameraClearProgress += amount;
            if (cameraClearProgress >= clearThreshold)
            {
                CameraJammed = false;
                cameraClearProgress = 0f;
                OnCameraJamCleared?.Invoke(RoomIndex);
            }
        }

        public void RegisterTurretClick(float amount, float clearThreshold)
        {
            if (!TurretsLocked) return;

            turretClearProgress += amount;
            if (turretClearProgress >= clearThreshold)
            {
                TurretsLocked = false;
                turretClearProgress = 0f;
                OnTurretJamCleared?.Invoke(RoomIndex);
            }
        }

        public void ForceClearCameraJam()
        {
            if (!CameraJammed) return;

            CameraJammed = false;
            cameraClearProgress = 0f;
            OnCameraJamCleared?.Invoke(RoomIndex);
        }

        public void ForceClearTurretJam()
        {
            if (!TurretsLocked) return;

            TurretsLocked = false;
            turretClearProgress = 0f;
            OnTurretJamCleared?.Invoke(RoomIndex);
        }
    }
}