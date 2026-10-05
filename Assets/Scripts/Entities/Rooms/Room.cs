using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Represents one defensive room.
    ///
    /// The room owns its shield and defensive structures.
    /// When the shield is destroyed, every defensive structure
    /// belonging to THIS room is powered off.
    ///
    /// Other rooms are unaffected.
    /// </summary>
    public class Room : MonoBehaviour
    {
        [Header("Room")]
        [SerializeField] private int roomIndex;
        [SerializeField] private string roomName;

        [Header("Shield")]
        [SerializeField] private Shield shield;

        private readonly List<DefensiveStructure> structures =
            new List<DefensiveStructure>();

        private RoomState state = RoomState.Active;

        // =========================================================
        // PUBLIC ACCESS
        // =========================================================

        public int RoomIndex => roomIndex;
        public string RoomName => roomName;

        public Shield Shield => shield;

        public RoomState State => state;

        public bool IsFallen =>
            state == RoomState.Fallen;

        // =========================================================
        // INITIALISATION
        // =========================================================

        private void Awake()
        {
            if (shield == null)
            {
                shield = GetComponentInChildren<Shield>();
            }
        }

        // =========================================================
        // STRUCTURE REGISTRATION
        // =========================================================

        /// <summary>
        /// Called automatically by DefensiveStructure.Awake().
        /// </summary>
        public void RegisterStructure(
            DefensiveStructure structure)
        {
            if (structure == null)
                return;

            if (!structures.Contains(structure))
            {
                structures.Add(structure);
            }
        }

        /// <summary>
        /// Removes a structure from this room.
        ///
        /// Used when a turret is replaced by an upgrade prefab.
        /// </summary>
        public void UnregisterStructure(
            DefensiveStructure structure)
        {
            if (structure == null)
                return;

            structures.Remove(structure);
        }

        // =========================================================
        // SHIELD DESTRUCTION
        // =========================================================

        /// <summary>
        /// Called by this room's Shield when its HP reaches zero.
        /// </summary>
        public void SetFallen()
        {
            // Prevent this from running multiple times.
            if (state == RoomState.Fallen)
                return;

            state = RoomState.Fallen;

            Debug.Log(
                $"[Room] {GetRoomDisplayName()} shield destroyed. " +
                "Disabling room structures."
            );

            DisableStructures();

            EventRegistry.BroadcastRoomFallen(this);
        }

        /// <summary>
        /// Turns off every defensive structure belonging to this room.
        ///
        /// This does NOT affect structures in other rooms.
        /// </summary>
        private void DisableStructures()
        {
            // Iterate backwards so the list can safely tolerate
            // structures being removed during the process.
            for (int i = structures.Count - 1; i >= 0; i--)
            {
                DefensiveStructure structure =
                    structures[i];

                if (structure == null)
                {
                    structures.RemoveAt(i);
                    continue;
                }

                structure.OnPowerCut();
            }
        }

        // =========================================================
        // ROOM RESTORATION
        // =========================================================

        /// <summary>
        /// Restores the room's gameplay state.
        ///
        /// Structures remain OFF after restoration and must be
        /// manually powered back on.
        /// </summary>
        public void RestoreRoom()
        {
            state = RoomState.Active;

            Debug.Log(
                $"[Room] {GetRoomDisplayName()} restored."
            );

            foreach (DefensiveStructure structure
                     in structures)
            {
                if (structure == null)
                    continue;

                structure.TurnOff();
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private string GetRoomDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(roomName))
                return roomName;

            return $"Room {roomIndex}";
        }
    }
}