using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Persistent object that owns a turret's upgrade state.
    ///
    /// The slot itself is never replaced.
    /// Only the turret prefab inside it is replaced.
    /// </summary>
    public class TurretUpgradeSlot : MonoBehaviour
    {
        [Header("Current Turret")]
        [SerializeField] private Turret currentTurret;

        [Header("Upgrade Prefabs")]
        [SerializeField] private GameObject baseTurretPrefab;
        [SerializeField] private GameObject doubleBarrelPrefab;
        [SerializeField] private GameObject longRangePrefab;
        [SerializeField] private GameObject machineLaserPrefab;

        [Header("Upgrade")]
        [SerializeField] private TurretUpgradeType currentUpgrade =
            TurretUpgradeType.Base;

        public TurretUpgradeType CurrentUpgrade =>
            currentUpgrade;

        private void Start()
        {
            if (currentTurret == null)
                currentTurret =
                    GetComponentInChildren<Turret>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ApplyUpgrade(
                    TurretUpgradeType.DoubleBarrel);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ApplyUpgrade(
                    TurretUpgradeType.LongRange);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ApplyUpgrade(
                    TurretUpgradeType.MachineLaser);
            }
        }

        public void ApplyUpgrade(
            TurretUpgradeType upgrade)
        {
            if (upgrade == currentUpgrade)
                return;

            GameObject prefab =
                GetPrefab(upgrade);

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[TurretUpgradeSlot] No prefab assigned for {upgrade}.");

                return;
            }

            bool wasActive =
                currentTurret != null &&
                currentTurret.IsActive;

            if (currentTurret != null)
            {
                Destroy(currentTurret.gameObject);
                currentTurret = null;
            }

            GameObject newTurret =
                Instantiate(
                    prefab,
                    transform.position,
                    transform.rotation,
                    transform);

            currentTurret =
                newTurret.GetComponent<Turret>();

            if (currentTurret == null)
            {
                Debug.LogError(
                    $"[TurretUpgradeSlot] Prefab {prefab.name} " +
                    "does not contain a Turret component.");

                Destroy(newTurret);
                return;
            }

            currentUpgrade = upgrade;

            if (wasActive)
                currentTurret.TurnOn();

            Debug.Log(
                $"[TurretUpgradeSlot] Upgraded to {upgrade}.");
        }

        private GameObject GetPrefab(
            TurretUpgradeType upgrade)
        {
            switch (upgrade)
            {
                case TurretUpgradeType.Base:
                    return baseTurretPrefab;

                case TurretUpgradeType.DoubleBarrel:
                    return doubleBarrelPrefab;

                case TurretUpgradeType.LongRange:
                    return longRangePrefab;

                case TurretUpgradeType.MachineLaser:
                    return machineLaserPrefab;
            }

            return null;
        }
    }
}