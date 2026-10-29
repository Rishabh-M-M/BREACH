
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Room management:
    /// Tracks defensive structures, room state, and permanent shutdown.
    /// </summary>
    public class Room : MonoBehaviour
    {
        [Header("Room Settings")]
        [SerializeField] private int roomIndex;
        [SerializeField] private string roomName;

        [Header("Defensive Structures")]
        [SerializeField]
        private List<DefensiveStructure> structures =
            new List<DefensiveStructure>();

        public RoomState State { get; private set; } = RoomState.Active;

        public int RoomIndex => roomIndex;
        public string RoomName => roomName;

        // Once disabled by a broken shield, this remains true
        // for the rest of the current run.
        public bool IsPermanentlyDisabled { get; private set; } = false;

        public IReadOnlyList<DefensiveStructure> Structures => structures;
        public event System.Action<Room> OnPermanentlyDisabled;

        // ============================================================
        // STRUCTURE REGISTRATION
        // ============================================================

        public void RegisterStructure(DefensiveStructure structure)
        {
            if (structure == null)
                return;

            if (!structures.Contains(structure))
            {
                structures.Add(structure);

                Debug.Log(
                    $"[Room {roomName}] Registered structure: " +
                    structure.name
                );
            }
        }

        // ============================================================
        // PERMANENT ROOM SHUTDOWN
        // ============================================================

        public void PermanentlyDisableRoom()
        {
            if (IsPermanentlyDisabled)
                return;

            IsPermanentlyDisabled = true;
            State = RoomState.Fallen;

            Debug.LogWarning(
                $"[Room {roomName}] Permanently disabled: " +
                "the room's shield has been destroyed."
            );

            OnPermanentlyDisabled?.Invoke(this);
        }

        // ============================================================
        // STRUCTURE DESTROYED NOTIFICATION
        // ============================================================

        public void NotifyStructureDestroyed(DefensiveStructure structure)
        {
            if (structure == null)
                return;

            if (structure is IPowerable powerable && structure.IsActive)
            {
                Debug.Log(
                    $"[Room {roomName}] Structure destroyed. " +
                    $"Power refund may be required: {structure.PowerCost}"
                );
            }

            CheckFallen();
        }

        // ============================================================
        // FALLEN CHECK
        // ============================================================

        private void CheckFallen()
        {
            if (State == RoomState.Fallen || IsPermanentlyDisabled)
                return;

            foreach (DefensiveStructure structure in structures)
            {
                if (structure is Turret turret && !turret.IsDead)
                    return;
            }

            SetFallen();
        }

        private void SetFallen()
        {
            if (State == RoomState.Fallen)
                return;

            State = RoomState.Fallen;

            Debug.LogWarning(
                $"[Room {roomName}] Room fallen!"
            );

            // Keep your existing room-fallen event integration here
            // if another script is responsible for broadcasting it.
        }
    }
}
