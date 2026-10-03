using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// PowerManager (Mock simplified version, to be replaced when the player team delivers the official version).
    /// Demo: MaxPower 50, Turret PowerCost 40 (second turret must trigger "insufficient power").
    /// </summary>
    public class PowerManager : Singleton<PowerManager>
    {
        [Header("Power Settings")]
        [SerializeField] private int maxPower = 100;
        [SerializeField] private int currentLoad = 0;

        public int MaxPower => maxPower;
        public int CurrentLoad => currentLoad;
        public int AvailablePower => maxPower - currentLoad;

        /// <summary>
        /// Attempts to allocate power. Returns true if successful, false if insufficient.
        /// Called when activating a turret.
        /// </summary>
        public bool TryAllocate(int cost)
        {
            if (currentLoad + cost > maxPower)
            {
                Debug.Log($"[PowerManager] Insufficient power! Required: {cost}, Available: {AvailablePower}");
                return false;
            }

            currentLoad += cost;
            EventRegistry.BroadcastPowerChanged(currentLoad, maxPower);
            Debug.Log($"[PowerManager] Allocated {cost} power, current {currentLoad}/{maxPower}");
            return true;
        }

        /// <summary>
        /// Refunds power (called when turret is destroyed or manually disabled).
        /// </summary>
        public void Refund(int cost)
        {
            currentLoad = Mathf.Max(0, currentLoad - cost);
            EventRegistry.BroadcastPowerChanged(currentLoad, maxPower);
            Debug.Log($"[PowerManager] Refunded {cost} power, current {currentLoad}/{maxPower}");
        }

        /// <summary>
        /// Test helper: Resets power (for debugging).
        /// </summary>
        [ContextMenu("Test - Reset Power")]
        private void TestResetPower()
        {
            currentLoad = 0;
            EventRegistry.BroadcastPowerChanged(currentLoad, maxPower);
            Debug.Log("[PowerManager] Power reset.");
        }
    }
}
