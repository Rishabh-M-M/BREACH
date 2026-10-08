using UnityEngine;

namespace ProjectBreach.Core
{
    public class TurretUpgradeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Turret turret;
        [SerializeField] private DoubleBarrelUpgrade doubleBarrelUpgrade;

        [Header("Current Variant")]
        [SerializeField] private TurretVariant currentVariant = TurretVariant.Base;

        [Header("Base Turret")]
        [SerializeField] private int baseOperatingPowerCost = 30;

        [Header("Base Visual")]
        [SerializeField] private GameObject baseVisual;

        public TurretVariant CurrentVariant => currentVariant;
        public bool IsBase => currentVariant == TurretVariant.Base;
        public bool IsDoubleBarrel => currentVariant == TurretVariant.DoubleBarrel;

        public string DisplayName
        {
            get
            {
                switch (currentVariant)
                {
                    case TurretVariant.DoubleBarrel: return "Double Barrel";
                    case TurretVariant.LongRange: return "Long Range";
                    case TurretVariant.MachineLaser: return "Machine Laser";
                    default: return "Base Turret";
                }
            }
        }

        private void Awake()
        {
            if (turret == null)
                turret = GetComponent<Turret>();

            if (doubleBarrelUpgrade == null)
                doubleBarrelUpgrade = GetComponent<DoubleBarrelUpgrade>();

            if (turret == null)
            {
                Debug.LogError($"[TurretUpgradeController] {name} has no Turret component.");
                return;
            }

            if (currentVariant == TurretVariant.Base)
                turret.SetPowerCost(baseOperatingPowerCost);

            ApplyVisuals();
        }

        public bool TryUpgradeToDoubleBarrel()
        {
            if (turret == null || doubleBarrelUpgrade == null)
                return false;

            if (currentVariant != TurretVariant.Base)
                return false;

            if (PowerManager.Instance == null)
                return false;

            int permanentCost = doubleBarrelUpgrade.PermanentPowerCost;

            if (!PowerManager.Instance.TryAllocatePermanent(permanentCost))
            {
                UIManager.Instance?.ShowMessage("Insufficient power capacity!");
                return false;
            }

            if (turret.IsActive)
                turret.TurnOff();

            currentVariant = TurretVariant.DoubleBarrel;
            turret.SetPowerCost(doubleBarrelUpgrade.OperatingPowerCost);

            ApplyVisuals();
            turret.RefreshActiveVisuals();

            UIManager.Instance?.ShowMessage("Double Barrel upgrade installed!");
            return true;
        }

        private TurretUpgrade GetActiveUpgrade()
        {
            return currentVariant == TurretVariant.DoubleBarrel
                ? doubleBarrelUpgrade
                : null;
        }

        public int GetProjectileDamage(int baseDamage)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();
            return upgrade != null ? upgrade.GetDamage(baseDamage) : baseDamage;
        }

        public float GetFireRate(float baseFireRate)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();
            return upgrade != null ? upgrade.GetFireRate(baseFireRate) : baseFireRate;
        }

        public float GetAttackRange(float baseRange)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();
            return upgrade != null ? upgrade.GetAttackRange(baseRange) : baseRange;
        }

        public bool TryFireUpgrade(Turret turretInstance, IDamageable target, int projectileDamage)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();
            return upgrade != null &&
                   upgrade.TryFire(turretInstance, target, projectileDamage);
        }

        public Transform GetActiveTurretHead(Transform baseHead)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();
            return upgrade != null ? upgrade.GetTurretHead(baseHead) : baseHead;
        }

        public Transform GetActiveFirePoint(Transform baseFirePoint)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();
            return upgrade != null ? upgrade.GetFirePoint(baseFirePoint) : baseFirePoint;
        }

        public bool FiresTwoProjectiles()
        {
            return currentVariant == TurretVariant.DoubleBarrel;
        }

        public void RefreshVisuals()
        {
            ApplyVisuals();
            turret?.RefreshActiveVisuals();
        }

        private void ApplyVisuals()
        {
            if (baseVisual != null)
                baseVisual.SetActive(currentVariant == TurretVariant.Base);

            if (doubleBarrelUpgrade != null)
                doubleBarrelUpgrade.SetVisualActive(
                    currentVariant == TurretVariant.DoubleBarrel
                );
        }

        public void DrawGizmos(Turret turretInstance)
        {
            GetActiveUpgrade()?.DrawGizmos(turretInstance);
        }

        [ContextMenu("TEST - Buy Double Barrel")]
        private void TestBuyDoubleBarrel()
        {
            TryUpgradeToDoubleBarrel();
        }
    }
}