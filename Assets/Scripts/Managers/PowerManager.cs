using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Central power resource for the game.
    ///
    /// Permanent power:
    /// - Used when a permanent turret upgrade is purchased.
    /// - Never refunded.
    ///
    /// Operating power:
    /// - Temporarily allocated while a powered structure is ON.
    /// - Refunded when that structure turns OFF or is destroyed.
    /// </summary>
    public class PowerManager : Singleton<PowerManager>
    {
        // Run-scoped state: power must reset to its scene defaults on every run,
        // so this manager must NOT survive scene loads (a restart after a loss
        // should not carry over the previous run's power/load).
        protected override bool PersistAcrossScenes => false;

        [Header("Power Settings")]
        [SerializeField] private int maxPower = 100;

        [Tooltip(
            "Upper bound for maxPower. The Generator passive grows maxPower " +
            "exponentially (+5% of current), so without a cap it overflows int " +
            "and wraps negative on long runs. Tune as needed.")]
        [SerializeField] private int maxPowerCap = 1000000;

        // Power permanently committed to upgrades.
        private int permanentLoad = 0;

        // Power temporarily consumed by currently operating structures.
        private int operatingLoad = 0;

        public int MaxPower => maxPower;

        /// <summary>
        /// Increases the permanent power ceiling (used by the Generator room
        /// passive). Broadcasts the new budget so the UI refreshes.
        /// </summary>
        public void IncreaseMaxPower(int amount)
        {
            if (amount <= 0)
                return;

            // Clamp so exponential Generator growth can never overflow int.
            maxPower = Mathf.Min(maxPower + amount, maxPowerCap);

            BroadcastPowerChanged();

            Debug.Log(
                $"[PowerManager] maxPower increased by {amount}. " +
                $"Total capacity: {maxPower}"
            );
        }

        /// <summary>
        /// Total power currently committed.
        /// </summary>
        public int CurrentLoad =>
            permanentLoad + operatingLoad;

        public int PermanentLoad =>
            permanentLoad;

        public int OperatingLoad =>
            operatingLoad;

        /// <summary>
        /// Power capacity still available for either a permanent
        /// upgrade or temporary operation.
        /// </summary>
        public int AvailablePower =>
            Mathf.Max(0, maxPower - CurrentLoad);

        /// <summary>
        /// Attempts to allocate temporary operating power.
        /// </summary>
        public bool TryAllocate(int cost)
        {
            if (cost < 0)
            {
                Debug.LogError(
                    "[PowerManager] Cannot allocate negative power."
                );

                return false;
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
        /// This NEVER refunds permanent upgrade power.
        /// </summary>
        public void Refund(int cost)
        {
            if (cost < 0)
            {
                Debug.LogError(
                    "[PowerManager] Cannot refund negative power."
                );

                return;
            }

            operatingLoad =
                Mathf.Max(0, operatingLoad - cost);

            BroadcastPowerChanged();

            Debug.Log(
                $"[PowerManager] Operating power refunded: {cost}. " +
                $"Permanent: {permanentLoad}, " +
                $"Operating: {operatingLoad}, " +
                $"Total: {CurrentLoad}/{maxPower}"
            );
        }

        /// <summary>
        /// Permanently commits power for an upgrade.
        ///
        /// This allocation is not refundable.
        /// </summary>
        public bool TryAllocatePermanent(int cost)
        {
            if (cost < 0)
            {
                Debug.LogError(
                    "[PowerManager] Cannot allocate negative permanent power."
                );

                return false;
            }

            if (CurrentLoad + cost > maxPower)
            {
                Debug.Log(
                    $"[PowerManager] Insufficient permanent power capacity! " +
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

        private void BroadcastPowerChanged()
        {
            EventRegistry.BroadcastPowerChanged(
                CurrentLoad,
                maxPower
            );
        }

        /// <summary>
        /// Test helper: resets both permanent and operating power.
        /// </summary>
        [ContextMenu("Test - Reset Power")]
        private void TestResetPower()
        {
            permanentLoad = 0;
            operatingLoad = 0;

            BroadcastPowerChanged();

            Debug.Log(
                "[PowerManager] Power reset. " +
                $"Total: {CurrentLoad}/{maxPower}"
            );
        }
    }
}
