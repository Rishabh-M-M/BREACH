using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Boss
{
    public class BossJamManager : Singleton<BossJamManager>
    {
        [Header("Rooms")]
        [SerializeField] private int roomCount = 4; // rooms 1..roomCount

        [Header("Clear Tuning")]
        [SerializeField] private float clickAmountPerPress = 1f;
        [SerializeField] private float clicksRequiredToClear = 6f;

        [Header("Boss Encounter")]
        [SerializeField] private bool isBossEncounterActive = false;
        [SerializeField] private int activateOnWaveNumber = 5; // player-facing wave number, not zero-based waveIndex

        [Header("Debug / Testing")]
        [SerializeField] private int testRoomIndex = 1;

        private readonly Dictionary<int, RoomJamState> jamStates = new();
        private readonly Dictionary<int, Room> rooms = new();

        public bool IsBossEncounterActive => isBossEncounterActive;
        public float ClickAmountPerPress => clickAmountPerPress;
        public float ClicksRequiredToClear => clicksRequiredToClear;
        public IReadOnlyCollection<int> RoomIndices => jamStates.Keys;

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this) return;

            for (int i = 1; i <= roomCount; i++)
            {
                RoomJamState state = new RoomJamState(i);
                state.OnCameraJamStarted += HandleCameraJamStarted;
                state.OnCameraJamCleared += HandleCameraJamCleared;
                state.OnTurretJamStarted += HandleTurretJamStarted;
                state.OnTurretJamCleared += HandleTurretJamCleared;
                jamStates[i] = state;
            }

            EventRegistry.OnWaveStart += HandleWaveStart; // confirm this matches the event name in EventRegistry.cs
        }

        private void Start()
        {
            Room[] foundRooms = FindObjectsOfType<Room>();
            foreach (Room room in foundRooms)
            {
                if (room.RoomIndex >= 1 && room.RoomIndex <= roomCount)
                {
                    rooms[room.RoomIndex] = room;
                    room.OnPermanentlyDisabled += HandleRoomPermanentlyDisabled;
                }
            }
        }

        private void OnDestroy()
        {
            EventRegistry.OnWaveStart -= HandleWaveStart;
        }

        // ============================================================
        // BOSS ENCOUNTER ON/OFF
        // ============================================================

        public void ActivateBossEncounter()
        {
            isBossEncounterActive = true;
            Debug.Log("[BossJamManager] Boss encounter activated.");
        }

        public void DeactivateBossEncounter()
        {
            isBossEncounterActive = false;

            foreach (RoomJamState state in jamStates.Values)
            {
                state.ForceClearCameraJam();
                state.ForceClearTurretJam();
            }

            Debug.Log("[BossJamManager] Boss encounter deactivated; all jams cleared.");
        }

        // ============================================================
        // LOOKUP
        // ============================================================

        public RoomJamState GetState(int roomIndex)
        {
            if (jamStates.TryGetValue(roomIndex, out RoomJamState state))
            {
                return state;
            }

            Debug.LogWarning($"[BossJamManager] No jam state for room index {roomIndex}.");
            return null;
        }

        public Room GetRoom(int roomIndex)
        {
            rooms.TryGetValue(roomIndex, out Room room);
            return room;
        }

        // ============================================================
        // GUARDED JAM TRIGGERS
        // Use these instead of calling RoomJamState.StartCameraJam /
        // StartTurretJam directly - this is where a fallen room gets
        // excluded. BossController's picker should still skip fallen rooms
        // when choosing a target too, but this is the backstop.
        // ============================================================

        public bool TryStartCameraJam(int roomIndex)
        {
            Room room = GetRoom(roomIndex);
            if (room != null && room.IsPermanentlyDisabled)
            {
                Debug.Log($"[BossJamManager] Skipped camera jam for room {roomIndex}: room is permanently disabled.");
                return false;
            }

            GetState(roomIndex)?.StartCameraJam();
            return true;
        }

        public bool TryStartTurretJam(int roomIndex)
        {
            Room room = GetRoom(roomIndex);
            if (room != null && room.IsPermanentlyDisabled)
            {
                Debug.Log($"[BossJamManager] Skipped turret jam for room {roomIndex}: room is permanently disabled.");
                return false;
            }

            GetState(roomIndex)?.StartTurretJam();
            return true;
        }

        // ============================================================
        // ROOM FALLEN -> CLEAR ANY ACTIVE JAM
        // ============================================================

        private void HandleRoomPermanentlyDisabled(Room room)
        {
            RoomJamState state = GetState(room.RoomIndex);
            if (state == null) return;

            bool hadCameraJam = state.CameraJammed;
            bool hadTurretJam = state.TurretsLocked;

            state.ForceClearCameraJam();
            state.ForceClearTurretJam();

            if (hadCameraJam || hadTurretJam)
            {
                Debug.Log($"[BossJamManager] Room {room.RoomIndex} fell; cleared its active boss jam(s).");
            }
        }

        // ============================================================
        // TURRET JAM EFFECT
        // ============================================================

        private void HandleTurretJamStarted(int roomIndex)
        {
            Room room = GetRoom(roomIndex);
            if (room == null)
            {
                Debug.LogWarning($"[BossJamManager] Turret jam started for room {roomIndex}, but no Room was found for that index.");
                return;
            }

            int shutOffCount = 0;
            foreach (DefensiveStructure structure in room.Structures)
            {
                if (structure is Turret turret && turret.IsActive)
                {
                    turret.TurnOff();
                    shutOffCount++;
                }
            }

            Debug.Log($"[BossJamManager] Turret jam in room {roomIndex}: switched off {shutOffCount} active turret(s).");
        }

        private void HandleTurretJamCleared(int roomIndex)
        {
            Debug.Log($"[BossJamManager] Turret jam cleared in room {roomIndex}.");
        }

        private void HandleCameraJamStarted(int roomIndex)
        {
            Debug.Log($"[BossJamManager] Camera jam started in room {roomIndex}.");
        }

        private void HandleCameraJamCleared(int roomIndex)
        {
            Debug.Log($"[BossJamManager] Camera jam cleared in room {roomIndex}.");
        }

        // ============================================================
        // WAVE INTEGRATION
        // ============================================================

        private void HandleWaveStart(int waveIndex)
        {
            if (isBossEncounterActive) return;

            int waveNumber = waveIndex + 1; // WaveManager's waveIndex is zero-based
            if (waveNumber >= activateOnWaveNumber)
            {
                ActivateBossEncounter();
            }
        }

        // ============================================================
        // DEBUG / TESTING
        // ============================================================

        [ContextMenu("Test - Start Camera Jam (Test Room)")]
        private void TestStartCameraJam() => TryStartCameraJam(testRoomIndex);

        [ContextMenu("Test - Start Turret Jam (Test Room)")]
        private void TestStartTurretJam() => TryStartTurretJam(testRoomIndex);

        [ContextMenu("Test - Click Camera Once (Test Room)")]
        private void TestClickCamera()
        {
            RoomJamState state = GetState(testRoomIndex);
            if (state == null) return;

            state.RegisterCameraClick(clickAmountPerPress, clicksRequiredToClear);
            Debug.Log($"[BossJamManager] Camera click registered for room {testRoomIndex}. Progress: {state.CameraClearProgress}/{clicksRequiredToClear}, Jammed: {state.CameraJammed}");
        }

        [ContextMenu("Test - Click Turret Once (Test Room)")]
        private void TestClickTurret()
        {
            RoomJamState state = GetState(testRoomIndex);
            if (state == null) return;

            state.RegisterTurretClick(clickAmountPerPress, clicksRequiredToClear);
            Debug.Log($"[BossJamManager] Turret click registered for room {testRoomIndex}. Progress: {state.TurretClearProgress}/{clicksRequiredToClear}, Locked: {state.TurretsLocked}");
        }

        [ContextMenu("Test - Force Clear Both (Test Room)")]
        private void TestForceClearRoom()
        {
            RoomJamState state = GetState(testRoomIndex);
            state?.ForceClearCameraJam();
            state?.ForceClearTurretJam();
        }

        [ContextMenu("Test - Permanently Disable Test Room")]
        private void TestPermanentlyDisableRoom() => GetRoom(testRoomIndex)?.PermanentlyDisableRoom();

        [ContextMenu("Test - Activate Boss Encounter")]
        private void TestActivate() => ActivateBossEncounter();

        [ContextMenu("Test - Deactivate Boss Encounter")]
        private void TestDeactivate() => DeactivateBossEncounter();
    }
}