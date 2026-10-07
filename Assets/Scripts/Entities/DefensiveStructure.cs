using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Base class for defensive structures.
    ///
    /// powerCost is the TEMPORARY operating power required while
    /// the structure is active.
    ///
    /// Permanent upgrade power is handled separately by PowerManager.
    /// </summary>
    public abstract class DefensiveStructure : MonoBehaviour, IPowerable
    {
        [Header("Operating Power")]
        [SerializeField] protected int powerCost = 10;

        [SerializeField] protected bool isActive = false;

        public Room OwnerRoom { get; protected set; }

        /// <summary>
        /// Temporary operating power required while active.
        /// </summary>
        public int PowerCost => powerCost;

        public bool IsActive => isActive;

        protected virtual void Awake()
        {
            OwnerRoom = GetComponentInParent<Room>();
            OwnerRoom?.RegisterStructure(this);
        }

        public virtual void TurnOn()
        {
            isActive = true;
        }

        public virtual void TurnOff()
        {
            isActive = false;
        }

        public virtual void OnPowerCut()
        {
            TurnOff();
        }

        /// <summary>
        /// Changes the temporary operating power cost.
        ///
        /// This does NOT affect permanent upgrade power.
        /// </summary>
        public void SetPowerCost(int newPowerCost)
        {
            if (newPowerCost < 0)
            {
                Debug.LogWarning(
                    $"[DefensiveStructure] Invalid power cost {newPowerCost}. " +
                    "Using 0 instead."
                );

                newPowerCost = 0;
            }

            powerCost = newPowerCost;
        }
    }
}