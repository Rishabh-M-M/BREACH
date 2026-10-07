using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Central power resource manager.
    ///
    /// There is one total power capacity, but two different types of usage:
    ///
    /// 1. Permanent Load
    ///    Power permanently committed by purchased upgrades.
    ///    Turning a turret OFF does NOT refund this power.
    ///
    /// 2. Operating Load
    ///    Temporary power consumed while turrets/defensive structures
    ///    are active.
    ///    Turning them OFF refunds this power.
    ///
    /// Example:
    ///
    /// Max Power       = 100
    /// Permanent Load  = 30
    /// Operating Load  = 20
    /// ----------------------
    /// Available Power = 50
    /// </summary>
    public class PowerManager : Singleton<PowerManager>
    {
        [Header("Power Settings")]
<<<<<<< Updated upstream
        [SerializeField] private int maxPower = 50;
        [SerializeField] private int currentLoad = 0;
=======
        [SerializeField] private int maxPower = 100;
>>>>>>> Stashed changes

        [Header("Runtime Power")]
        [SerializeField] private int permanentLoad = 0;
        [SerializeField] private int operatingLoad = 0;

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        /// <summary>
        /// Maximum total power capacity.
        /// </summary>
        public int MaxPower => maxPower;

        /// <summary>
        /// Power permanently committed by upgrades.
        /// This remains consumed even when upgraded equipment is OFF.
        /// </summary>
        public int PermanentLoad => permanentLoad;

        /// <summary>
        /// Temporary power currently consumed by active equipment.
        /// </summary>
        public int OperatingLoad => operatingLoad;

        /// <summary>
        /// Total power currently committed.
        ///
        /// PermanentLoad + OperatingLoad.
        /// </summary>
        public int CurrentLoad => permanentLoad + operatingLoad;

        /// <summary>
        /// Power that can still be used.
        ///
        /// MaxPower - PermanentLoad - OperatingLoad.
        /// </summary>
        public int AvailablePower =>
            Mathf.Max(0, maxPower - permanentLoad - operatingLoad);

        // ============================================================
        // TEMPORARY / OPERATING POWER
        // ============================================================

        /// <summary>
        /// Attempts to allocate temporary operating power.
        ///
        /// Used when a turret or other defensive structure turns ON.
        ///
        /// This power is returned by Refund().
        /// </summary>
        public bool TryAllocate(int cost)
        {
            if (cost <= 0)
            {
                return true;
            }

            if (CurrentLoad + cost > maxPower)
            {
                Debug.Log(
                    $"[PowerManager] Insufficient operating power! " +
                    $"Required: {cost}, Available: {AvailablePower}"
                );

                return false;
            }

            operatingLoad += cost;

            BroadcastPowerChanged();

            Debug.Log(
                $"[PowerManager] Operating power allocated: {cost}. " +
                $"Permanent: {permanentLoad}, " +
                $"Operating: {operatingLoad}, " +
                $"Total: {CurrentLoad}/{maxPower}"
            );

            return true;
        }

        /// <summary>
        /// Refunds temporary operating power.
        ///
        /// Used when active equipment is turned OFF.
        ///
        /// IMPORTANT:
        /// This does NOT refund permanent upgrade power.
        /// </summary>
        public void Refund(int cost)
        {
            if (cost <= 0)
            {
                return;
            }

            operatingLoad = Mathf.Max(0, operatingLoad - cost);

            BroadcastPowerChanged();

            Debug.Log(
                $"[PowerManager] Operating power refunded: {cost}. " +
                $"Permanent: {permanentLoad}, " +
                $"Operating: {operatingLoad}, " +
                $"Total: {CurrentLoad}/{maxPower}"
            );
        }

        // ============================================================
        // PERMANENT UPGRADE POWER
        // ============================================================

        /// <summary>
        /// Attempts to permanently consume power for an upgrade.
        ///
        /// Unlike TryAllocate(), this power is NOT refunded when the
        /// turret is turned OFF.
        ///
        /// Example:
        ///
        /// MaxPower = 100
        ///
        /// TryAllocatePermanent(30)
        ///
        /// PermanentLoad = 30
        /// AvailablePower = 70
        /// </summary>
        public bool TryAllocatePermanent(int cost)
        {
            if (cost <= 0)
            {
                return true;
            }

            if (CurrentLoad + cost > maxPower)
            {
                Debug.Log(
                    $"[PowerManager] Insufficient permanent power! " +
                    $"Required: {cost}, Available: {AvailablePower}"
                );

                return false;
            }

            permanentLoad += cost;

            BroadcastPowerChanged();

            Debug.Log(
                $"[PowerManager] Permanent power committed: {cost}. " +
                $"Permanent: {permanentLoad}, " +
                $"Operating: {operatingLoad}, " +
                $"Total: {CurrentLoad}/{maxPower}"
            );

            return true;
        }

        /// <summary>
        /// Refunds permanent power.
        ///
        /// This is NOT used when a turret is turned OFF.
        ///
        /// It exists for future functionality such as selling an upgrade,
        /// resetting a run, or loading/resetting a game state.
        /// </summary>
        public void RefundPermanent(int cost)
        {
            if (cost <= 0)
            {
                return;
            }

            permanentLoad = Mathf.Max(0, permanentLoad - cost);

            BroadcastPowerChanged();

            Debug.Log(
                $"[PowerManager] Permanent power refunded: {cost}. " +
                $"Permanent: {permanentLoad}, " +
                $"Operating: {operatingLoad}, " +
                $"Total: {CurrentLoad}/{maxPower}"
            );
        }

        // ============================================================
        // DEBUG / TESTING
        // ============================================================

        /// <summary>
        /// Resets all power usage.
        ///
        /// This is only intended for testing/debugging.
        /// </summary>
        [ContextMenu("Test - Reset Power")]
        private void TestResetPower()
        {
            permanentLoad = 0;
            operatingLoad = 0;

            BroadcastPowerChanged();

            Debug.Log("[PowerManager] Power reset.");
        }

        // ============================================================
        // INTERNAL
        // ============================================================

        private void BroadcastPowerChanged()
        {
            EventRegistry.BroadcastPowerChanged(
                CurrentLoad,
                maxPower
            );
        }
    }
}