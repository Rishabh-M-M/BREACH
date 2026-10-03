using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Turret: Defensive structure that automatically acquires and shoots enemies.
    /// Implements IDamageable (can be destroyed), IUpgradable (W6).
    /// Demo values: HP 30, Damage 5, FireRate 1s, Range 80m, PowerCost 40.
    /// </summary>
    public class Turret : DefensiveStructure, IDamageable  // IUpgradable removed for W2 Demo
    {
        [Header("Turret Combat Settings")]
        [SerializeField] private int maxHP = 30;
        [SerializeField] private int damage = 5;
        [SerializeField] private float fireRate = 1f;      // Seconds between shots
        [SerializeField] private float attackRange = 8f;

        [Header("Target Filtering")]
        [Tooltip("Collider layers considered for targeting. Left as Everything on purpose: the EnemyHealth guard in AcquireTarget is what blocks friendly fire, so an unconfigured mask cannot silently disarm the turret.")]
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("References")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Turret Rotation")]
        [SerializeField] private Transform turretHead;    // The rotating part (turret_4)
        [SerializeField] private float rotationSpeed = 90f; // Degrees per second
        [SerializeField] private float rotationOffset = 0f;  // Fix model forward direction (0/90/180/270)
        [SerializeField] private bool lockXRotation = true;

        [Header("Activation Visuals")]
        [SerializeField] private Renderer topRenderer;     // turret_1_high_turret_1_0 (uses TurretTop shader)
        [SerializeField] private Renderer baseRenderer;    // bottom_1_high_bottom_1_0 (uses TurretBase shader)
        [SerializeField] private float activationLerpSpeed = 4f; // How fast _Activation animates toward its target

        [Header("Shooting Recoil")]
        [SerializeField] private float recoilDistance = 0.12f;      // How far the head kicks back
        [SerializeField] private float recoilRecoverySpeed = 6f;    // How fast it returns to rest
        [SerializeField] private Vector3 recoilLocalDirection = new Vector3(0f, 0f, -1f); // Tune per model orientation

        private static readonly int ActivationID = Shader.PropertyToID("_Activation");
        private static readonly int FireTimeID = Shader.PropertyToID("_FireTime");

        private MaterialPropertyBlock topPropBlock;
        private MaterialPropertyBlock basePropBlock;
        private float currentActivation;
        private float targetActivation;

        private Vector3 headRestLocalPos;
        private float recoilAmount; // 0 = rest, 1 = full recoil, decays over time

        private float defaultLocalX;

        private float defaultLocalZ;

        // Verbose combat logging is a WebGL performance tax (console I/O every shot).
        private static bool verboseLogging = false;

        // Whether THIS turret currently holds a power allocation. Tracked per-turret so
        // the ledger never depends on which code path toggled the turret (button,
        // Inspector, EMP, future systems) -- see ReconcilePowerState().
        private bool holdingPower;

        /// <summary>
        /// Keeps the power ledger in sync with the actual IsActive state, every frame.
        /// Any activation path (UI button, direct Inspector toggle, scripted enable)
        /// allocates exactly once; any deactivation refunds exactly once. If power is
        /// insufficient the turret is forced back off and the player is notified, so
        /// there is no such thing as a free activation.
        /// </summary>
        private void ReconcilePowerState()
        {
            if (IsDead) return;

            if (isActive && !holdingPower)
            {
                if (PowerManager.Instance == null || PowerManager.Instance.TryAllocate(powerCost))
                {
                    // No PowerManager in a test scene: activate for free rather than brick the turret.
                    holdingPower = true;
                }
                else
                {
                    isActive = false;
                    UIManager.Instance?.ShowMessage("Insufficient power!");
                }
            }
            else if (!isActive && holdingPower)
            {
                PowerManager.Instance?.Refund(powerCost);
                holdingPower = false;
            }
        }

        private int currentHP;
        private float fireTimer;
        private IDamageable currentTarget;
        private int level = 1;

        // IDamageable implementation
        public int MaxHP => maxHP;
        public int CurrentHP => currentHP;
        public bool IsDead => currentHP <= 0;

        // IUpgradable implementation (W6, commented out for Demo)
        // public int Level => level

        protected override void Awake()
        {
            base.Awake();
            currentHP = maxHP;

            // Safety net: a zero mask silently disarms this turret (OverlapSphere returns
            // nothing) whenever the Enemy layer has not been configured yet. Fall back to
            // Everything; the EnemyHealth guard in AcquireTarget still blocks friendly fire.
            if (targetMask.value == 0)
            {
                targetMask = ~0;
            }

            if (turretHead != null)
            {
                defaultLocalX = turretHead.localEulerAngles.x;   // Cache the authored rest pose
                defaultLocalZ = turretHead.localEulerAngles.z;   // Cache the authored rest pose
                headRestLocalPos = turretHead.localPosition;
            }

            topPropBlock = new MaterialPropertyBlock();
            basePropBlock = new MaterialPropertyBlock();

            // Sync visuals with whatever isActive was set to at spawn (e.g. from prefab defaults)
            targetActivation = isActive ? 1f : 0f;
            currentActivation = targetActivation;
            ApplyActivationVisual();
        }

        private void Update()
        {
            // Activation glow/grayscale and recoil recovery run regardless of active state,
            // so a turret smoothly fades out and its head settles even the instant it's turned off.
            ReconcilePowerState();
            UpdateActivationVisual();
            UpdateRecoil();

            // Only active turrets can shoot
            if (!isActive || IsDead) return;

            // Acquire target
            AcquireTarget();

            // Rotate turret head toward target (Y axis only)
            if (currentTarget != null && turretHead != null)
            {
                RotateTowardsTarget();
            }

            // Fire timer
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate && currentTarget != null)
            {
                fireTimer = 0f;
                Shoot();
            }
        }

        /// <summary>
        /// Smoothly moves currentActivation toward targetActivation and pushes it to both
        /// materials via MaterialPropertyBlock. Using property blocks (not renderer.material)
        /// keeps each turret instance's shader state fully independent without leaking
        /// per-instance material copies, and without touching the shared material asset
        /// (which would otherwise change every turret in the scene at once).
        /// </summary>
        private void UpdateActivationVisual()
        {
            // Derive the visual target from the source of truth (isActive) every frame,
            // so edits made straight in the Inspector (or any system that flips isActive
            // without calling TurnOn/TurnOff, e.g. a power cut) are reflected too.
            targetActivation = isActive ? 1f : 0f;

            if (Mathf.Approximately(currentActivation, targetActivation)) return;

            currentActivation = Mathf.MoveTowards(currentActivation, targetActivation, activationLerpSpeed * Time.deltaTime);
            ApplyActivationVisual();
        }

        private void ApplyActivationVisual()
        {
            if (topRenderer != null)
            {
                topRenderer.GetPropertyBlock(topPropBlock);
                topPropBlock.SetFloat(ActivationID, currentActivation);
                topRenderer.SetPropertyBlock(topPropBlock);
            }

            if (baseRenderer != null)
            {
                baseRenderer.GetPropertyBlock(basePropBlock);
                basePropBlock.SetFloat(ActivationID, currentActivation);
                baseRenderer.SetPropertyBlock(basePropBlock);
            }
        }

        /// <summary>
        /// Kicks the turret head back along recoilLocalDirection, then eases it back to rest.
        /// The offset is rotated by turretHead's CURRENT localRotation before being applied, so
        /// recoil always kicks straight back along the barrel — even while the head is rotated
        /// off-axis tracking a target — instead of sliding sideways along the parent's fixed axes.
        /// </summary>
        private void UpdateRecoil()
        {
            if (turretHead == null) return;
            if (recoilAmount <= 0f) return;

            recoilAmount = Mathf.MoveTowards(recoilAmount, 0f, recoilRecoverySpeed * Time.deltaTime);

            Vector3 rotatedOffset = turretHead.localRotation * (recoilLocalDirection.normalized * recoilDistance * recoilAmount);
            turretHead.localPosition = headRestLocalPos + rotatedOffset;
        }

        public override void TurnOn()
        {
            base.TurnOn();
            targetActivation = 1f;
        }

        public override void TurnOff()
        {
            base.TurnOff();
            targetActivation = 0f;
        }

        /// <summary>
        /// Rotate turret head to face current target (Y axis only).
        /// Uses LookAt for simplicity, works even with offset pivots.
        /// </summary>
        private void RotateTowardsTarget()
        {
            MonoBehaviour targetMb = currentTarget as MonoBehaviour;
            if (targetMb == null) return;

            Vector3 toTarget = targetMb.transform.position - turretHead.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            float targetYaw = Quaternion.LookRotation(toTarget).eulerAngles.y;
            float parentYaw = turretHead.parent != null ? turretHead.parent.eulerAngles.y : 0f;
            float localYaw = targetYaw - parentYaw + rotationOffset;


            Quaternion targetLocalRotation = Quaternion.Euler(defaultLocalX, localYaw, defaultLocalZ);


            turretHead.localRotation = Quaternion.RotateTowards(
                turretHead.localRotation,
                targetLocalRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// Find nearest alive enemy within attack range.
        /// Uses OverlapSphere for Demo simplicity.
        /// </summary>
        private void AcquireTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
            float nearestDist = float.MaxValue;
            IDamageable nearest = null;

            foreach (var hit in hits)
            {
                if (hit.gameObject == this.gameObject) continue;

                // Friendly-fire guard: turrets and the MainControlRoom are IDamageable too,
                // so the interface alone cannot tell friend from foe. Only colliders with an
                // EnemyHealth in their hierarchy are valid targets.
                if (hit.GetComponentInParent<EnemyHealth>() == null) continue;

                if (hit.TryGetComponent(out IDamageable target) && !target.IsDead)
                {

                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = target;
                    }
                }
            }

            currentTarget = nearest;
        }

        /// <summary>
        /// Fire a projectile at the current target.
        /// </summary>
        private void Shoot()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[Turret] No projectile prefab assigned!");
                return;
            }

            if (firePoint == null)
            {
                Debug.LogWarning("[Turret] No fire point assigned!");
                return;
            }

            if (currentTarget == null) return;

            GameObject projObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (projObj.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(currentTarget, damage);  // Use Initialize, not Init
                if (verboseLogging) Debug.Log($"[Turret] Fired at target, damage: {damage}");
            }

            recoilAmount = 1f; // UpdateRecoil() eases this back to 0 every frame

            // Trigger the base's fire ripple: the shader expands a ring outward from
            // (Time.time - _FireTime), so we just need to stamp "now" on each shot.
            if (baseRenderer != null)
            {
                baseRenderer.GetPropertyBlock(basePropBlock);
                basePropBlock.SetFloat(FireTimeID, Time.time);
                baseRenderer.SetPropertyBlock(basePropBlock);
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            currentHP -= amount;
            if (verboseLogging) Debug.Log($"[Turret] Took {amount} damage, HP: {currentHP}/{maxHP}");

            if (currentHP <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            Debug.Log("[Turret] Destroyed!");

            // Power refund is owned by Room.NotifyStructureDestroyed (Indirection), which
            // refunds only when this turret was active. Refunding here as well double-pays
            // the player and silently inflates the power budget.
            // Notify owner room
            OwnerRoom?.NotifyStructureDestroyed(this);

            // Destroy self
            Destroy(gameObject);
        }

        // W6 content, commented out for Demo
        // public void ApplyUpgrade(UpgradeData data)
        // {
        //     level++;
        // }

        /// <summary>
        /// Test helper: Right-click component in Inspector to test damage.
        /// </summary>
        [ContextMenu("Test - Take 10 Damage")]
        private void TestTakeDamage()
        {
            TakeDamage(10);
        }
    }
}