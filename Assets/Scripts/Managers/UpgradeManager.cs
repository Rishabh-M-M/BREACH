using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Owns the upgrade-point economy (TeamDoc 8.5 / 9.4).
    ///
    /// Spec: the player starts a run with 100 points and an upgrade costs 50.
    /// Kills trickle extra points in so a run can snowball.
    ///
    /// GRASP: Controller -- this is the single place that decides whether a
    /// purchase is legal, so no individual turret needs to reason about the budget.
    ///
    /// Must exist in the scene: Singleton has no lazy creation.
    /// </summary>
    public class UpgradeManager : Singleton<UpgradeManager>
    {
        // Run-scoped state: upgrade points must reset to their scene defaults on
        // every run, so this manager must NOT survive scene loads (a restart
        // should start a fresh 100-point budget, not keep the previous run's).
        protected override bool PersistAcrossScenes => false;

        [Header("Economy (spec 9.4)")]
        [SerializeField] private int startingPoints = 100;
        [SerializeField] private int pointsPerKill = 10;

        [Header("Defaults")]
        [Tooltip("Asset applied by PurchaseDefault, so the fallback path grants a real bonus " +
                 "instead of only deducting points.")]
        [SerializeField] private UpgradeData defaultUpgrade;

        [Tooltip("Fallback cost so a ContextMenu test purchase works without an asset.")]
        [SerializeField] private int fallbackCost = 50;

        private int _points;
        private bool _initialised;

        public int Points => _points;

        protected override void Awake()
        {
            // Registers the singleton. Must run before anything resolves Instance.
            base.Awake();
        }

        private void Start()
        {
            _points = startingPoints;
            _initialised = true;
            Broadcast();
        }

        private void OnEnable()
        {
            EventRegistry.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            EventRegistry.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled()
        {
            if (pointsPerKill <= 0)
            {
                return;
            }

            _points += pointsPerKill;
            Broadcast();
        }

        /// <summary>
        /// Spend points and apply the upgrade. Returns false (and spends nothing)
        /// if the player cannot afford it or the target refuses.
        /// </summary>
        public bool Purchase(IUpgradable target, UpgradeData data)
        {
            if (target == null || data == null)
            {
                Debug.LogWarning("[UpgradeManager] Purchase called with null target or data.");
                return false;
            }

            if (target.Level >= data.MaxLevel)
            {
                UIManager.Instance?.ShowMessage("Already at max level.");
                return false;
            }

            if (_points < data.Cost)
            {
                UIManager.Instance?.ShowMessage("Not enough upgrade points.");
                return false;
            }

            if (!target.TryApplyUpgrade(data))
            {
                return false;
            }

            _points -= data.Cost;
            Broadcast();

            Debug.Log($"[UpgradeManager] Purchased '{data.UpgradeName}' -> level {target.Level}. " +
                      $"Points {_points} left.");
            return true;
        }

        /// <summary>
        /// Asset-free purchase used by ContextMenu testing and by any caller that just
        /// wants the spec default numbers.
        /// </summary>
        public bool PurchaseDefault(IUpgradable target)
        {
            if (target == null)
            {
                return false;
            }

            // Route through Purchase: affordability/level checks and the actual bonus
            // application stay in one place.
            return Purchase(target, defaultUpgrade);
        }

        private void Broadcast()
        {
            EventRegistry.BroadcastUpgradePointsChanged(_points);
        }

        [ContextMenu("Test - Grant 50 Points")]
        private void TestGrant()
        {
            if (!_initialised)
            {
                _initialised = true;
                _points = startingPoints;
            }

            _points += 50;
            Broadcast();
        }
    }
}
