using UnityEngine;

namespace ProjectBreach.Core
{
    public class TurretUpgradeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Turret turret;
        [SerializeField] private DoubleBarrelUpgrade doubleBarrelUpgrade;
        [SerializeField] private SharpshooterUpgrade sharpshooterUpgrade;

        [Header("Current Variant")]
        [SerializeField] private TurretVariant currentVariant = TurretVariant.Base;

        [Header("Base Turret")]
        [SerializeField] private int baseOperatingPowerCost = 30;

        [Header("Base Visual")]
        [SerializeField] private GameObject baseVisual;

        public TurretVariant CurrentVariant => currentVariant;
        public bool IsBase => currentVariant == TurretVariant.Base;
        public bool IsDoubleBarrel => currentVariant == TurretVariant.DoubleBarrel;
        public bool IsSharpshooter => currentVariant == TurretVariant.Sharpshooter;

        public string DisplayName
        {
            get
            {
                switch (currentVariant)
                {
                    case TurretVariant.DoubleBarrel:
                        return "Double Barrel";

                    case TurretVariant.Sharpshooter:
                        return "Sharpshooter (SS)";

                    case TurretVariant.MachineLaser:
                        return "Machine Laser";

                    default:
                        return "Base Turret";
                }
            }
        }

        private void Awake()
        {
            if (turret == null)
                turret = GetComponent<Turret>();

            if (doubleBarrelUpgrade == null)
                doubleBarrelUpgrade = GetComponent<DoubleBarrelUpgrade>();

            if (sharpshooterUpgrade == null)
                sharpshooterUpgrade = GetComponent<SharpshooterUpgrade>();

            if (turret == null)
            {
                Debug.LogError(
                    $"[TurretUpgradeController] {name} has no Turret component."
                );
                return;
            }

            switch (currentVariant)
            {
                case TurretVariant.Base:
                    turret.SetPowerCost(baseOperatingPowerCost);
                    break;

                case TurretVariant.DoubleBarrel:
                    if (doubleBarrelUpgrade != null)
                        turret.SetPowerCost(
                            doubleBarrelUpgrade.OperatingPowerCost
                        );
                    break;

                case TurretVariant.Sharpshooter:
                    if (sharpshooterUpgrade != null)
                        turret.SetPowerCost(
                            sharpshooterUpgrade.OperatingPowerCost
                        );
                    break;
            }

            ApplyVisuals();
        }

        // ============================================================
        // DOUBLE BARREL PURCHASE
        // ============================================================

        public bool TryUpgradeToDoubleBarrel()
        {
            if (turret == null || doubleBarrelUpgrade == null)
                return false;

            if (currentVariant != TurretVariant.Base)
                return false;

            if (PowerManager.Instance == null)
                return false;

            int permanentCost =
                doubleBarrelUpgrade.PermanentPowerCost;

            if (!PowerManager.Instance.TryAllocatePermanent(
                    permanentCost))
            {
                UIManager.Instance?.ShowMessage(
                    "Insufficient power capacity!"
                );
                return false;
            }

            if (turret.IsActive)
                turret.TurnOff();

            currentVariant = TurretVariant.DoubleBarrel;

            turret.SetPowerCost(
                doubleBarrelUpgrade.OperatingPowerCost
            );

            ApplyVisuals();
            turret.RefreshActiveVisuals();

            UIManager.Instance?.ShowMessage(
                "Double Barrel upgrade installed!"
            );

            return true;
        }

        // ============================================================
        // SHARPSHOOTER PURCHASE
        // ============================================================

        public bool TryUpgradeToSharpshooter()
        {
            if (turret == null || sharpshooterUpgrade == null)
            {
                Debug.LogError(
                    $"[TurretUpgradeController] {name} has no SharpshooterUpgrade component."
                );
                return false;
            }

            if (currentVariant != TurretVariant.Base)
            {
                Debug.LogWarning(
                    $"[TurretUpgradeController] Cannot upgrade {name} " +
                    $"from {currentVariant} to Sharpshooter."
                );
                return false;
            }

            if (PowerManager.Instance == null)
            {
                Debug.LogError(
                    "[TurretUpgradeController] PowerManager.Instance is missing."
                );
                return false;
            }

            int permanentCost =
                sharpshooterUpgrade.PermanentPowerCost;

            if (!PowerManager.Instance.TryAllocatePermanent(
                    permanentCost))
            {
                UIManager.Instance?.ShowMessage(
                    "Insufficient power capacity!"
                );
                return false;
            }

            // Refund the currently held Base operating cost BEFORE
            // changing PowerCost to the SS operating cost.
            if (turret.IsActive)
                turret.TurnOff();

            currentVariant = TurretVariant.Sharpshooter;

            turret.SetPowerCost(
                sharpshooterUpgrade.OperatingPowerCost
            );

            ApplyVisuals();
            turret.RefreshActiveVisuals();

            UIManager.Instance?.ShowMessage(
                "Sharpshooter upgrade installed!"
            );

            return true;
        }

        // ============================================================
        // ACTIVE UPGRADE
        // ============================================================

        private TurretUpgrade GetActiveUpgrade()
        {
            switch (currentVariant)
            {
                case TurretVariant.DoubleBarrel:
                    return doubleBarrelUpgrade;

                case TurretVariant.Sharpshooter:
                    return sharpshooterUpgrade;

                default:
                    return null;
            }
        }

        public bool HasActiveUpgrade()
        {
            return GetActiveUpgrade() != null;
        }

        public int GetProjectileDamage(int baseDamage)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetDamage(baseDamage)
                : baseDamage;
        }

        public float GetFireRate(float baseFireRate)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetFireRate(baseFireRate)
                : baseFireRate;
        }

        public float GetAttackRange(float baseRange)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetAttackRange(baseRange)
                : baseRange;
        }

        public bool TryFireUpgrade(
            Turret turretInstance,
            IDamageable target,
            int projectileDamage
        )
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null &&
                   upgrade.TryFire(
                       turretInstance,
                       target,
                       projectileDamage
                   );
        }

        public Transform GetActiveTurretHead(
            Transform baseHead
        )
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetTurretHead(baseHead)
                : baseHead;
        }

        public Transform GetActiveFirePoint(
            Transform baseFirePoint
        )
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetFirePoint(baseFirePoint)
                : baseFirePoint;
        }

        public bool FiresTwoProjectiles()
        {
            return currentVariant ==
                   TurretVariant.DoubleBarrel;
        }

        public bool PrefersFurthestTarget()
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null &&
                   upgrade.PrefersFurthestTarget();
        }

        public float GetRecoilDistance(float baseDistance)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetRecoilDistance(baseDistance)
                : baseDistance;
        }

        public float GetRecoilRecoverySpeed(
            float baseRecoverySpeed
        )
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetRecoilRecoverySpeed(baseRecoverySpeed)
                : baseRecoverySpeed;
        }

        public Vector3 GetRecoilLocalDirection(
            Vector3 baseDirection
        )
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetRecoilLocalDirection(baseDirection)
                : baseDirection;
        }

        public void RefreshVisuals()
        {
            ApplyVisuals();
            turret?.RefreshActiveVisuals();
        }

        private void ApplyVisuals()
        {
            if (baseVisual != null)
            {
                baseVisual.SetActive(
                    currentVariant == TurretVariant.Base
                );
            }

            if (doubleBarrelUpgrade != null)
            {
                doubleBarrelUpgrade.SetVisualActive(
                    currentVariant ==
                    TurretVariant.DoubleBarrel
                );
            }

            if (sharpshooterUpgrade != null)
            {
                sharpshooterUpgrade.SetVisualActive(
                    currentVariant ==
                    TurretVariant.Sharpshooter
                );
            }
        }

        public void DrawGizmos(Turret turretInstance)
        {
            GetActiveUpgrade()?.DrawGizmos(
                turretInstance
            );
        }

        [ContextMenu("TEST - Buy Double Barrel")]
        private void TestBuyDoubleBarrel()
        {
            TryUpgradeToDoubleBarrel();
        }

        [ContextMenu("TEST - Buy Sharpshooter")]
        private void TestBuySharpshooter()
        {
            TryUpgradeToSharpshooter();
        }
    }
}
