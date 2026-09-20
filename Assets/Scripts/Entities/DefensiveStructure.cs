using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Abstract base class for defensive structures (Temporary shell, to be deleted when the combat team's official version arrives)
    /// </summary>
    public abstract class DefensiveStructure : MonoBehaviour, IPowerable
    {
        [SerializeField] protected int powerCost = 10;
        [SerializeField] protected bool isActive = false;  // Used by Enemy
        public Room OwnerRoom { get; protected set; }
        public int PowerCost => powerCost;
        public bool IsActive => isActive;

        protected virtual void Awake()
        {
            OwnerRoom = GetComponentInParent<Room>();
            OwnerRoom?.RegisterStructure(this);
        }

        public virtual void TurnOn() { isActive = true; }
        public virtual void TurnOff() { isActive = false; }
        public virtual void OnPowerCut() { TurnOff(); }
    }
}