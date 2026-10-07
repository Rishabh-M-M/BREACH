using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
<<<<<<< Updated upstream
    /// Handles replacing a base turret with one of its upgrade prefabs.
    ///
    /// Temporary controls:
    /// 1 = Double Barrel Cannon
    /// 2 = Long Range Shooter
    /// 3 = Machine Laser
    ///
    /// Only one upgrade can be active at a time.
    /// Upgrades can be switched between freely.
    ///
    /// This controller is intentionally separate from Turret.cs because
    /// upgrades such as the Machine Laser have completely different logic.
    /// </summary>
    public class TurretUpgradeController : MonoBehaviour
    {
        [Header("Target Turret")]
        [SerializeField] private Turret targetTurret;

        [Header("Upgrade Prefabs")]
        [SerializeField] private GameObject doubleBarrelPrefab;
        [SerializeField] private GameObject longRangePrefab;
        [SerializeField] private GameObject machineLaserPrefab;

        [Header("Temporary Testing")]
        [Tooltip("Enable keyboard 1/2/3 controls for this turret.")]
        [SerializeField] private bool enableHotkeys = true;

        private GameObject currentUpgradeObject;
        private TurretUpgradeType currentUpgrade =
            TurretUpgradeType.Base;

        public TurretUpgradeType CurrentUpgrade => currentUpgrade;
        public GameObject CurrentUpgradeObject => currentUpgradeObject;

        private void Awake()
        {
            if (targetTurret == null)
            {
                targetTurret = GetComponent<Turret>();
            }
        }

        private void Update()
        {
            if (!enableHotkeys)
                return;

            // Temporary testing controls.
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ApplyUpgrade(TurretUpgradeType.DoubleBarrel);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyUpgrade(TurretUpgradeType.LongRange);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ApplyUpgrade(TurretUpgradeType.MachineLaser);
            }
        }

        /// <summary>
        /// Switch this turret to the requested upgrade.
        ///
        /// Switching is allowed:
        /// Double Barrel -> Laser
        /// Laser -> Long Range
        /// Long Range -> Double Barrel
        ///
        /// Selecting the already-active upgrade does nothing.
        /// </summary>
        public bool ApplyUpgrade(TurretUpgradeType upgradeType)
        {
            if (targetTurret == null)
            {
                Debug.LogWarning(
                    "[TurretUpgradeController] No target turret assigned."
=======
    /// Handles permanent upgrades for one individual turret.
    ///
    /// IMPORTANT:
    /// This does NOT replace the Turret GameObject.
    ///
    /// The existing Turret remains responsible for:
    /// - targeting
    /// - rotation
    /// - projectiles
    /// - damage
    /// - room registration
    /// - ON/OFF state
    ///
    /// This component controls:
    /// - installed variant
    /// - permanent upgrade power
    /// - operating power cost
    /// - variant combat settings
    /// - variant visuals
    /// </summary>
    public class TurretUpgradeController : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]
        [SerializeField] private Turret turret;

        // ============================================================
        // CURRENT VARIANT
        // ============================================================

        [Header("Current Variant")]
        [SerializeField] private TurretVariant currentVariant =
            TurretVariant.Base;

        // ============================================================
        // BASE TURRET
        // ============================================================

        [Header("Base Turret")]
        [SerializeField] private int baseOperatingPowerCost = 30;

        // ============================================================
        // DOUBLE BARREL
        // ============================================================

        [Header("Double Barrel")]
        [SerializeField] private int doubleBarrelPermanentPowerCost = 30;

        [SerializeField] private int doubleBarrelOperatingPowerCost = 50;

        [Tooltip("Damage dealt by EACH barrel.")]
        [SerializeField] private int doubleBarrelDamage = 4;

        [Tooltip(
            "Seconds between Double Barrel firing cycles. " +
            "1.1 seconds gives approximately 1.45x the base DPS " +
            "if the base turret deals 5 damage every 1 second."
        )]
        [SerializeField] private float doubleBarrelFireRate = 1.1f;

        [SerializeField] private float doubleBarrelRange = 8f;

        // ============================================================
        // VISUALS
        // ============================================================

        [Header("Turret Visuals")]
        [Tooltip(
            "The current/base turret visual. This should contain ONLY " +
            "the visual model/mesh, not another Turret component."
        )]
        [SerializeField] private GameObject baseVisual;

        [Tooltip(
            "The Double Barrel visual/model. This should contain ONLY " +
            "the visual model/mesh, not another Turret component."
        )]
        [SerializeField] private GameObject doubleBarrelVisual;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public TurretVariant CurrentVariant =>
            currentVariant;

        public bool IsBase =>
            currentVariant == TurretVariant.Base;

        public bool IsDoubleBarrel =>
            currentVariant == TurretVariant.DoubleBarrel;

        public string DisplayName
        {
            get
            {
                switch (currentVariant)
                {
                    case TurretVariant.DoubleBarrel:
                        return "Double Barrel";

                    case TurretVariant.LongRange:
                        return "Long Range";

                    case TurretVariant.MachineLaser:
                        return "Machine Laser";

                    default:
                        return "Base Turret";
                }
            }
        }

        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            if (turret == null)
            {
                turret = GetComponent<Turret>();
            }

            if (turret == null)
            {
                Debug.LogError(
                    $"[TurretUpgradeController] {name} has no Turret component."
                );

                return;
            }

            // Make sure a Base turret has its correct operating cost.
            if (currentVariant == TurretVariant.Base)
            {
                turret.SetPowerCost(
                    baseOperatingPowerCost
                );
            }

            ApplyVisuals();
        }

        // ============================================================
        // DOUBLE BARREL PURCHASE
        // ============================================================

        /// <summary>
        /// Permanently purchases Double Barrel for this individual turret.
        ///
        /// The purchase:
        ///
        /// 1. Permanently consumes upgrade power.
        /// 2. Turns the turret OFF.
        /// 3. Changes its operating power cost from Base to Double Barrel.
        /// 4. Changes its combat configuration.
        /// 5. Changes its visual model.
        ///
        /// The turret remains the same GameObject.
        /// </summary>
        public bool TryUpgradeToDoubleBarrel()
        {
            if (turret == null)
            {
                Debug.LogError(
                    $"[TurretUpgradeController] No Turret assigned on {name}."
>>>>>>> Stashed changes
                );

                return false;
            }

<<<<<<< Updated upstream
            if (currentUpgrade == upgradeType)
            {
                Debug.Log(
                    $"[TurretUpgradeController] " +
                    $"{targetTurret.name} already uses {upgradeType}."
=======
            // --------------------------------------------------------
            // Already upgraded
            // --------------------------------------------------------

            if (currentVariant == TurretVariant.DoubleBarrel)
            {
                Debug.Log(
                    $"[TurretUpgradeController] {name} is already Double Barrel."
>>>>>>> Stashed changes
                );

                return false;
            }

<<<<<<< Updated upstream
            GameObject prefab = GetPrefab(upgradeType);

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[TurretUpgradeController] No prefab assigned " +
                    $"for {upgradeType}."
=======
            // --------------------------------------------------------
            // Only Base → Double Barrel for now
            // --------------------------------------------------------

            if (currentVariant != TurretVariant.Base)
            {
                Debug.LogWarning(
                    $"[TurretUpgradeController] Cannot upgrade {name} " +
                    $"from {currentVariant} to Double Barrel."
>>>>>>> Stashed changes
                );

                return false;
            }

<<<<<<< Updated upstream
            // Store everything the replacement needs before destroying
            // the original turret.
            Transform parent = targetTurret.transform.parent;
            Vector3 position = targetTurret.transform.position;
            Quaternion rotation = targetTurret.transform.rotation;

            Room ownerRoom = targetTurret.OwnerRoom;
            bool wasActive = targetTurret.IsActive;
            int oldPowerCost = targetTurret.PowerCost;

            // Disable the old turret first.
            targetTurret.TurnOff();

            // Remove it from the room's structure list.
            ownerRoom?.UnregisterStructure(targetTurret);

            // Create the upgraded turret.
            GameObject upgradedObject =
                Instantiate(
                    prefab,
                    position,
                    rotation,
                    parent
                );

            currentUpgradeObject = upgradedObject;
            currentUpgrade = upgradeType;

            // Make sure the replacement knows which room it belongs to.
            DefensiveStructure upgradedStructure =
                upgradedObject.GetComponent<DefensiveStructure>();

            if (upgradedStructure != null)
            {
                // DefensiveStructure normally discovers its room
                // through its parent in Awake().
                //
                // If it is not under a Room, warn instead of silently
                // producing a turret that has no room relationship.
                if (upgradedStructure.OwnerRoom == null)
                {
                    Debug.LogWarning(
                        $"[TurretUpgradeController] " +
                        $"{upgradedObject.name} has no OwnerRoom."
                    );
                }
            }

            // The original turret no longer exists.
            Destroy(targetTurret.gameObject);

            Debug.Log(
                $"[TurretUpgradeController] " +
                $"Turret upgraded to {upgradeType}."
            );

            // If the original turret was active, activate the upgrade.
            if (wasActive && upgradedStructure != null)
            {
                upgradedStructure.TurnOn();
            }

            return true;
        }

        private GameObject GetPrefab(
            TurretUpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case TurretUpgradeType.DoubleBarrel:
                    return doubleBarrelPrefab;

                case TurretUpgradeType.LongRange:
                    return longRangePrefab;

                case TurretUpgradeType.MachineLaser:
                    return machineLaserPrefab;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns to the base turret.
        ///
        /// This is useful later for the upgrade menu if the player
        /// needs to revert to the standard turret.
        /// </summary>
        public void ClearUpgrade()
        {
            currentUpgradeObject = null;
            currentUpgrade = TurretUpgradeType.Base;
        }
    }

    public enum TurretUpgradeType
    {
        Base,
        DoubleBarrel,
        LongRange,
        MachineLaser
    }
=======
            // --------------------------------------------------------
            // PowerManager must exist
            // --------------------------------------------------------

            if (PowerManager.Instance == null)
            {
                Debug.LogError(
                    "[TurretUpgradeController] PowerManager.Instance is missing."
                );

                return false;
            }

            Debug.Log(
                $"[TurretUpgradeController] Buying Double Barrel for {name}. " +
                $"Permanent cost = {doubleBarrelPermanentPowerCost}, " +
                $"Available = {PowerManager.Instance.AvailablePower}"
            );

            // --------------------------------------------------------
            // FIRST: Permanently reserve upgrade power
            // --------------------------------------------------------

            bool upgradePowerAllocated =
                PowerManager.Instance.TryAllocatePermanent(
                    doubleBarrelPermanentPowerCost
                );

            if (!upgradePowerAllocated)
            {
                Debug.Log(
                    $"[TurretUpgradeController] Upgrade FAILED. " +
                    $"Need {doubleBarrelPermanentPowerCost}, " +
                    $"available {PowerManager.Instance.AvailablePower}."
                );

                UIManager.Instance?.ShowMessage(
                    "Insufficient power capacity!"
                );

                return false;
            }

            // --------------------------------------------------------
            // SECOND: Turn turret OFF
            // --------------------------------------------------------
            //
            // This is deliberately AFTER the permanent purchase succeeds.
            //
            // Turret.TurnOff() will immediately release its OLD operating
            // power allocation before we change PowerCost from 30 → 50.
            //

            if (turret.IsActive)
            {
                turret.TurnOff();
            }

            // --------------------------------------------------------
            // THIRD: Change variant
            // --------------------------------------------------------

            currentVariant =
                TurretVariant.DoubleBarrel;

            // --------------------------------------------------------
            // FOURTH: Change operating power cost
            // --------------------------------------------------------

            turret.SetPowerCost(
                doubleBarrelOperatingPowerCost
            );

            // --------------------------------------------------------
            // FIFTH: Update visuals
            // --------------------------------------------------------

            ApplyVisuals();

            // --------------------------------------------------------
            // SUCCESS
            // --------------------------------------------------------

            Debug.Log(
                $"[TurretUpgradeController] SUCCESS: {name} upgraded. " +
                $"Variant = {DisplayName}, " +
                $"Operating Power = {turret.PowerCost}, " +
                $"Permanent Power = {PowerManager.Instance.PermanentLoad}, " +
                $"Operating Power Used = {PowerManager.Instance.OperatingLoad}, " +
                $"Available Power = {PowerManager.Instance.AvailablePower}"
            );

            UIManager.Instance?.ShowMessage(
                "Double Barrel upgrade installed!"
            );

            return true;
        }

        // ============================================================
        // VISUALS
        // ============================================================

        private void ApplyVisuals()
        {
            // Base visual
            if (baseVisual != null)
            {
                baseVisual.SetActive(
                    currentVariant == TurretVariant.Base
                );
            }

            // Double Barrel visual
            if (doubleBarrelVisual != null)
            {
                doubleBarrelVisual.SetActive(
                    currentVariant == TurretVariant.DoubleBarrel
                );
            }
        }

        // ============================================================
        // COMBAT DATA
        // ============================================================

        /// <summary>
        /// Returns the damage of one projectile for the current variant.
        /// </summary>
        public int GetProjectileDamage(
            int baseDamage
        )
        {
            if (currentVariant ==
                TurretVariant.DoubleBarrel)
            {
                return doubleBarrelDamage;
            }

            return baseDamage;
        }

        /// <summary>
        /// Returns the fire interval for the current variant.
        /// </summary>
        public float GetFireRate(
            float baseFireRate
        )
        {
            if (currentVariant ==
                TurretVariant.DoubleBarrel)
            {
                return doubleBarrelFireRate;
            }

            return baseFireRate;
        }

        /// <summary>
        /// Returns the attack range for the current variant.
        /// </summary>
        public float GetAttackRange(
            float baseRange
        )
        {
            if (currentVariant ==
                TurretVariant.DoubleBarrel)
            {
                return doubleBarrelRange;
            }

            return baseRange;
        }

        /// <summary>
        /// Returns whether this turret fires two projectiles.
        /// </summary>
        public bool FiresTwoProjectiles()
        {
            return currentVariant ==
                   TurretVariant.DoubleBarrel;
        }

        // ============================================================
        // TEST
        // ============================================================

        [ContextMenu("TEST - Buy Double Barrel")]
        private void TestBuyDoubleBarrel()
        {
            TryUpgradeToDoubleBarrel();
        }
    }
>>>>>>> Stashed changes
}