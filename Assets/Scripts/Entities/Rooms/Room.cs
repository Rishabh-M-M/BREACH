using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Room (GRASP: Information Expert + Indirection)
    /// Fallen check only counts Turret, this is the hard rule from GDD
    /// </summary>
    public class Room : MonoBehaviour
    {
        [SerializeField] private int roomIndex;
        [SerializeField] private string roomName;
        [Header("Room Shield") ]
        [SerializeField] private Shield roomShield;
        [SerializeField] private bool isMainRoom;
        private bool permanentlyDisabled;
        [SerializeField]
        private List<DefensiveStructure> structures
            = new List<DefensiveStructure>();

        public RoomState State { get; private set; } = RoomState.Active;
        public int RoomIndex => roomIndex;
        public string RoomName => roomName;
        public bool IsPermanentlyDisabled => permanentlyDisabled;

        private void Awake()
        {
            if (roomShield == null) roomShield = GetComponentInChildren<Shield>(true);
        }

        public void OnShieldBroken(Shield shield)
        {
            if (shield != roomShield || permanentlyDisabled) return;
            if (isMainRoom)
            {
                Debug.LogError($"[Room {roomName}] Main room shield broken. GAME OVER.");
                if (GameManager.Instance != null) GameManager.Instance.RegisterCoreDestroyed();
                return;
            }
            permanentlyDisabled = true;
            State = RoomState.Fallen;
            foreach (var structure in structures)
            {
                if (structure is Turret turret) turret.PermanentlyDisableFromRoom();
            }
            Debug.LogWarning($"[Room {roomName}] Shield broken. All room turrets permanently disabled.");
        }

        // ========== Registration ==========
        public void RegisterStructure(DefensiveStructure s)
        {
            if (!structures.Contains(s))
            {
                structures.Add(s);
                Debug.Log($"[Room {roomName}] Registered structure: {s.name}");
            }
        }

        // ========== Indirection: structure destroyed notification ==========
        public void NotifyStructureDestroyed(DefensiveStructure s)
        {
            // Replace Turret to notify PowerManager (Low Coupling)
            if (s is IPowerable powerable && s.IsActive)
            {
                // TODO W2: PowerManager.Instance.Refund(powerable);
                Debug.Log($"[Room {roomName}] Structure destroyed, should refund power {s.PowerCost}");
            }

            CheckFallen();
        }

        // ========== Fallen check (Turret only) ==========
        private void CheckFallen()
        {
            if (State == RoomState.Fallen) return;

            foreach (var s in structures)
            {
                if (s is Turret turret && !turret.IsDead && turret.gameObject.activeInHierarchy)
                    return;  // Still have alive turrets, not fallen
            }

            SetFallen();
        }

        private void SetFallen()
        {
            State = RoomState.Fallen;
            Debug.LogWarning($"[Room {roomName}] Room fallen!");

            // EventRegistry.BroadcastRoomFallen(this);
        }
    }
}