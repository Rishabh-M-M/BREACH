using System.Collections.Generic;
using UnityEngine;

namespace ProjectBreach.Core
{
    public class TurretUpgradeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Turret turret;
        [SerializeField] private DoubleBarrelUpgrade doubleBarrelUpgrade;
        [SerializeField] private SharpshooterUpgrade sharpshooterUpgrade;
        [SerializeField] private HeliosIncineratorUpgrade heliosIncineratorUpgrade;
        [SerializeField] private RocketLauncherUpgrade rocketLauncherUpgrade;

        [Header("Current Variant")]
        [SerializeField] private TurretVariant currentVariant = TurretVariant.Base;

        [Header("Base Turret")]
        [SerializeField] private int baseOperatingPowerCost = 30;

        [Header("Base Visual")]
        [SerializeField] private GameObject baseVisual;

        // Variants purchased previously on this turret.
        // These remain committed even after switching to another variant.
        private readonly HashSet<TurretVariant> committedVariants =
            new HashSet<TurretVariant>();

        public TurretVariant CurrentVariant => currentVariant;

        public bool IsBase => currentVariant == TurretVariant.Base;
        public bool IsDoubleBarrel => currentVariant == TurretVariant.DoubleBarrel;
        public bool IsSharpshooter => currentVariant == TurretVariant.Sharpshooter;
        public bool IsHeliosIncinerator => currentVariant == TurretVariant.HeliosIncinerator;
        public bool IsRocketLauncher => currentVariant == TurretVariant.RocketLauncher;

        public HeliosIncineratorUpgrade ActiveHeliosIncinerator =>
            heliosIncineratorUpgrade;

        public RocketLauncherUpgrade ActiveRocketLauncher =>
            rocketLauncherUpgrade;

        // Kept for compatibility with existing UI code.
        // This now means the CURRENT variant is not Base.
        public bool HasCommittedUpgrade => !IsBase;

        public bool IsVariantCommitted(TurretVariant variant)
        {
            return committedVariants.Contains(variant);
        }

        public bool IsVariantPurchased(TurretVariant variant)
        {
            return variant == currentVariant ||
                   committedVariants.Contains(variant);
        }

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

                    case TurretVariant.HeliosIncinerator:
                        return "Helios Incinerator";

                    case TurretVariant.RocketLauncher:
                        return "RocketLauncher";

                    default:
                        return "Base Turret";
                }
            }
        }

        private void Awake()
        {
            ResolveReferences();

            if (turret == null)
            {
                Debug.LogError(
                    $"[TurretUpgradeController] {name}: Turret component missing.",
                    this);

                enabled = false;
                return;
            }

            if (!HasRequiredComponent(currentVariant))
            {
                Debug.LogError(
                    $"[TurretUpgradeController] {name}: Missing component for " +
                    $"{currentVariant}. Resetting to Base.",
                    this);

                currentVariant = TurretVariant.Base;
            }

            turret.SetPowerCost(GetOperatingPowerCost(currentVariant));

            ApplyVisuals();
            turret.RefreshActiveVisuals();

            Debug.Log(
                $"[TurretUpgradeController] Initialized {name}. " +
                $"Variant={currentVariant}, DisplayName={DisplayName}",
                this);
        }

        private void ResolveReferences()
        {
            if (turret == null)
                turret = GetComponent<Turret>();

            if (doubleBarrelUpgrade == null)
                doubleBarrelUpgrade =
                    GetComponentInChildren<DoubleBarrelUpgrade>(true);

            if (sharpshooterUpgrade == null)
                sharpshooterUpgrade =
                    GetComponentInChildren<SharpshooterUpgrade>(true);

            if (heliosIncineratorUpgrade == null)
                heliosIncineratorUpgrade =
                    GetComponentInChildren<HeliosIncineratorUpgrade>(true);

            if (rocketLauncherUpgrade == null)
                rocketLauncherUpgrade =
                    GetComponentInChildren<RocketLauncherUpgrade>(true);
        }

        private bool HasRequiredComponent(TurretVariant variant)
        {
            switch (variant)
            {
                case TurretVariant.Base:
                    return true;

                case TurretVariant.DoubleBarrel:
                    return doubleBarrelUpgrade != null;

                case TurretVariant.Sharpshooter:
                    return sharpshooterUpgrade != null;

                case TurretVariant.HeliosIncinerator:
                    return heliosIncineratorUpgrade != null;

                case TurretVariant.RocketLauncher:
                    return rocketLauncherUpgrade != null;

                default:
                    return false;
            }
        }

        private int GetOperatingPowerCost(TurretVariant variant)
        {
            switch (variant)
            {
                case TurretVariant.DoubleBarrel:
                    return doubleBarrelUpgrade != null
                        ? doubleBarrelUpgrade.OperatingPowerCost
                        : baseOperatingPowerCost;

                case TurretVariant.Sharpshooter:
                    return sharpshooterUpgrade != null
                        ? sharpshooterUpgrade.OperatingPowerCost
                        : baseOperatingPowerCost;

                case TurretVariant.HeliosIncinerator:
                    return heliosIncineratorUpgrade != null
                        ? heliosIncineratorUpgrade.OperatingPowerCost
                        : baseOperatingPowerCost;

                case TurretVariant.RocketLauncher:
                    return rocketLauncherUpgrade != null
                        ? rocketLauncherUpgrade.OperatingPowerCost
                        : baseOperatingPowerCost;

                default:
                    return baseOperatingPowerCost;
            }
        }

        private TurretUpgrade GetUpgrade(TurretVariant variant)
        {
            switch (variant)
            {
                case TurretVariant.DoubleBarrel:
                    return doubleBarrelUpgrade;

                case TurretVariant.Sharpshooter:
                    return sharpshooterUpgrade;

                case TurretVariant.HeliosIncinerator:
                    return heliosIncineratorUpgrade;

                case TurretVariant.RocketLauncher:
                    return rocketLauncherUpgrade;

                default:
                    return null;
            }
        }

        // ============================================================
        // VARIANT PURCHASE AND SWITCHING
        // ============================================================

        private bool TrySwitchVariant(
            TurretVariant targetVariant,
            TurretUpgrade targetUpgrade,
            string upgradeName)
        {
            ResolveReferences();

            if (turret == null)
                return false;

            if (targetVariant == TurretVariant.Base)
            {
                Debug.LogWarning(
                    "[TurretUpgradeController] Returning to Base is disabled.",
                    this);

                return false;
            }

            // Cannot purchase the currently selected variant again.
            if (currentVariant == targetVariant)
            {
                Debug.Log(
                    $"[TurretUpgradeController] {upgradeName} is already selected.",
                    this);

                return false;
            }

            // A previously purchased variant can never be purchased again.
            if (committedVariants.Contains(targetVariant))
            {
                Debug.LogWarning(
                    $"[TurretUpgradeController] {name}: {upgradeName} " +
                    "has already been purchased and is committed.",
                    this);

                return false;
            }

            if (targetUpgrade == null ||
                !HasRequiredComponent(targetVariant))
            {
                Debug.LogError(
                    $"[TurretUpgradeController] Cannot purchase {upgradeName}. " +
                    "The correct upgrade component is missing.",
                    this);

                return false;
            }

            if (PowerManager.Instance == null)
            {
                Debug.LogError(
                    "[TurretUpgradeController] PowerManager.Instance is missing.",
                    this);

                return false;
            }

            int permanentCost = targetUpgrade.PermanentPowerCost;
            int operatingCost = targetUpgrade.OperatingPowerCost;

            if (permanentCost < 0 || operatingCost < 0)
            {
                Debug.LogError(
                    $"[TurretUpgradeController] Invalid costs for {upgradeName}.",
                    this);

                return false;
            }

            // Reserve permanent power before changing the current variant.
            if (!PowerManager.Instance.TryAllocatePermanent(permanentCost))
            {
                UIManager.Instance?.ShowMessage(
                    "Insufficient power capacity!");

                return false;
            }

            // Commit the previous variant only after the new purchase succeeds.
            if (currentVariant != TurretVariant.Base)
                committedVariants.Add(currentVariant);

            // Stop the turret while its operating cost changes.
            if (turret.IsActive)
                turret.TurnOff();

            currentVariant = targetVariant;

            turret.SetPowerCost(operatingCost);

            ApplyVisuals();
            turret.RefreshActiveVisuals();

            UIManager.Instance?.ShowMessage(upgradeName + " selected!");

            Debug.Log(
                $"[TurretUpgradeController] VARIANT SWITCHED: {name}. " +
                $"Variant={currentVariant}, " +
                $"PermanentCost={permanentCost}, " +
                $"OperatingCost={operatingCost}, " +
                $"Committed=[{string.Join(", ", committedVariants)}]",
                this);

            return true;
        }

        // ============================================================
        // UPGRADE SELECTION
        // ============================================================

        public bool TryUpgradeToDoubleBarrel()
        {
            return TrySwitchVariant(
                TurretVariant.DoubleBarrel,
                doubleBarrelUpgrade,
                "Double Barrel");
        }

        public bool TryUpgradeToSharpshooter()
        {
            return TrySwitchVariant(
                TurretVariant.Sharpshooter,
                sharpshooterUpgrade,
                "Sharpshooter");
        }

        public bool TryUpgradeToHeliosIncinerator()
        {
            return TrySwitchVariant(
                TurretVariant.HeliosIncinerator,
                heliosIncineratorUpgrade,
                "Helios Incinerator");
        }

        public bool TryUpgradeToRocketLauncher()
        {
            return TrySwitchVariant(
                TurretVariant.RocketLauncher,
                rocketLauncherUpgrade,
                "RocketLauncher");
        }

        public bool TrySwitchToBase()
        {
            Debug.LogWarning(
                "[TurretUpgradeController] Returning to Base is disabled.",
                this);

            return false;
        }

        // ============================================================
        // ACTIVE UPGRADE
        // ============================================================

        private TurretUpgrade GetActiveUpgrade()
        {
            return GetUpgrade(currentVariant);
        }

        public bool HasActiveUpgrade()
        {
            return GetActiveUpgrade() != null;
        }

        public AudioClip GetActiveFireSound()
        {
            return GetActiveUpgrade()?.GetFireSound();
        }

        public AudioClip GetActiveRotationSound()
        {
            return GetActiveUpgrade()?.GetRotationSound();
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
            int projectileDamage)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null &&
                   upgrade.TryFire(
                       turretInstance,
                       target,
                       projectileDamage);
        }

        public Transform GetActiveTurretHead(Transform baseHead)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetTurretHead(baseHead)
                : baseHead;
        }

        public Transform GetActiveFirePoint(Transform baseFirePoint)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetFirePoint(baseFirePoint)
                : baseFirePoint;
        }

        public bool FiresTwoProjectiles()
        {
            return IsDoubleBarrel;
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

        public float GetRecoilRecoverySpeed(float baseRecoverySpeed)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetRecoilRecoverySpeed(baseRecoverySpeed)
                : baseRecoverySpeed;
        }

        public Vector3 GetRecoilLocalDirection(Vector3 baseDirection)
        {
            TurretUpgrade upgrade = GetActiveUpgrade();

            return upgrade != null
                ? upgrade.GetRecoilLocalDirection(baseDirection)
                : baseDirection;
        }

        // ============================================================
        // VISUALS
        // ============================================================

        public void RefreshVisuals()
        {
            ResolveReferences();
            ApplyVisuals();
            turret?.RefreshActiveVisuals();
        }

        private void ApplyVisuals()
        {
            if (baseVisual != null)
                baseVisual.SetActive(IsBase);

            if (doubleBarrelUpgrade != null)
                doubleBarrelUpgrade.SetVisualActive(IsDoubleBarrel);

            if (sharpshooterUpgrade != null)
                sharpshooterUpgrade.SetVisualActive(IsSharpshooter);

            if (heliosIncineratorUpgrade != null)
                heliosIncineratorUpgrade.SetVisualActive(IsHeliosIncinerator);

            if (rocketLauncherUpgrade != null)
                rocketLauncherUpgrade.SetVisualActive(IsRocketLauncher);
        }

        public void DrawGizmos(Turret turretInstance)
        {
            GetActiveUpgrade()?.DrawGizmos(turretInstance);
        }

        // ============================================================
        // DEBUG
        // ============================================================

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

        [ContextMenu("TEST - Buy Helios Incinerator")]
        private void TestBuyHeliosIncinerator()
        {
            TryUpgradeToHeliosIncinerator();
        }

        [ContextMenu("TEST - Buy RocketLauncher")]
        private void TestBuyRocketLauncher()
        {
            TryUpgradeToRocketLauncher();
        }
    }
}