using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
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
                );

                return false;
            }

            if (currentUpgrade == upgradeType)
            {
                Debug.Log(
                    $"[TurretUpgradeController] " +
                    $"{targetTurret.name} already uses {upgradeType}."
                );

                return false;
            }

            GameObject prefab = GetPrefab(upgradeType);

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[TurretUpgradeController] No prefab assigned " +
                    $"for {upgradeType}."
                );

                return false;
            }

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
}