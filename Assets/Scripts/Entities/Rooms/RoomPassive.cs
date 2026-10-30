using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Base class for a room's passive ability (design doc §4.1).
    ///
    /// A passive is enabled only while its owner room is online
    /// (<see cref="RoomState.Active"/>). Because the current fall logic has
    /// two sources (shield broken -> PermanentlyDisableRoom, or all turrets
    /// destroyed -> CheckFallen), both end in <see cref="RoomState.Fallen"/>,
    /// so each passive simply polls <see cref="IsEnabled"/> on its own tick to
    /// stop itself — this covers both fall paths without touching the fall
    /// detection itself.
    /// </summary>
    public abstract class RoomPassive : MonoBehaviour
    {
        [Header("Room Passive")]
        [Tooltip("The room that owns this passive. Auto-resolved from the parent if empty.")]
        [SerializeField] protected Room ownerRoom;

        public Room OwnerRoom => ownerRoom;

        /// <summary>True while the owner room is online (not fallen/disabled).</summary>
        public bool IsEnabled =>
            ownerRoom != null && ownerRoom.State == RoomState.Active;

        protected virtual void Awake()
        {
            if (ownerRoom == null)
                ownerRoom = GetComponentInParent<Room>();
        }

        /// <summary>
        /// Hook subclasses override to run their per-frame/tick logic.
        /// The base implementation does nothing; the passive stays inert.
        /// </summary>
        protected virtual void Update()
        {
        }
    }
}
