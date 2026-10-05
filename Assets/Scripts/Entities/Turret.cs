using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Base turret weapon.
    ///
    /// Turrets no longer have individual HP.
    /// The room shield determines whether the room's defensive structures
    /// can remain operational.
    /// </summary>
    public class Turret : DefensiveStructure
    {
        [Header("Turret Combat Settings")]
<<<<<<< Updated upstream
        [SerializeField] private int maxHP = 30;
        [SerializeField] private int damage = 5;
        [SerializeField] private float fireRate = 1f;      // Seconds between shots
        [SerializeField] private float attackRange = 8f;
=======
        [SerializeField] protected int damage = 5;
        [SerializeField] protected float fireRate = 1f;
        [SerializeField] protected float attackRange = 8f;
>>>>>>> Stashed changes

        [Header("References")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform firePoint;

        [Header("Turret Rotation")]
        [SerializeField] protected Transform turretHead;
        [SerializeField] protected float rotationSpeed = 90f;
        [SerializeField] protected float rotationOffset = 0f;
        [SerializeField] protected bool lockXRotation = true;

        [Header("Activation Visuals")]
        [SerializeField] protected Renderer topRenderer;
        [SerializeField] protected Renderer baseRenderer;
        [SerializeField] protected float activationLerpSpeed = 4f;

        [Header("Shooting Recoil")]
        [SerializeField] protected float recoilDistance = 0.12f;
        [SerializeField] protected float recoilRecoverySpeed = 6f;
        [SerializeField] protected Vector3 recoilLocalDirection =
            new Vector3(0f, 0f, -1f);

        protected float fireTimer;
        protected IDamageable currentTarget;

        private static readonly int ActivationID =
            Shader.PropertyToID("_Activation");

        private static readonly int FireTimeID =
            Shader.PropertyToID("_FireTime");

        private MaterialPropertyBlock topPropBlock;
        private MaterialPropertyBlock basePropBlock;

        private float currentActivation;
        private float targetActivation;

        private Vector3 headRestLocalPos;
        protected float recoilAmount;

        private float defaultLocalX;
        private float defaultLocalZ;

<<<<<<< Updated upstream
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

            if (turretHead != null)
            {
                defaultLocalX = turretHead.localEulerAngles.x;   // 缓存 -90
                defaultLocalZ = turretHead.localEulerAngles.z;   // 缓存 0
=======
        protected override void Awake()
        {
            base.Awake();

            if (turretHead != null)
            {
                defaultLocalX = turretHead.localEulerAngles.x;
                defaultLocalZ = turretHead.localEulerAngles.z;
>>>>>>> Stashed changes
                headRestLocalPos = turretHead.localPosition;
            }

            topPropBlock = new MaterialPropertyBlock();
            basePropBlock = new MaterialPropertyBlock();

            targetActivation = isActive ? 1f : 0f;
            currentActivation = targetActivation;

            ApplyActivationVisual();
        }

        protected virtual void Update()
        {
<<<<<<< Updated upstream
            // Activation glow/grayscale and recoil recovery run regardless of active state,
            // so a turret smoothly fades out and its head settles even the instant it's turned off.
=======
>>>>>>> Stashed changes
            UpdateActivationVisual();
            UpdateRecoil();

            if (!isActive)
                return;

            AcquireTarget();

            if (currentTarget != null && turretHead != null)
                RotateTowardsTarget();

            fireTimer += Time.deltaTime;

            if (fireTimer >= fireRate && currentTarget != null)
            {
                fireTimer = 0f;
                Shoot();
            }
        }

        protected virtual void AcquireTarget()
        {
<<<<<<< Updated upstream
            if (Mathf.Approximately(currentActivation, targetActivation)) return;
=======
            Collider[] hits =
                Physics.OverlapSphere(transform.position, attackRange);

            float nearestDistance = float.MaxValue;
            IDamageable nearest = null;
>>>>>>> Stashed changes

            foreach (Collider hit in hits)
            {
                if (hit.gameObject == gameObject)
                    continue;

                if (hit.TryGetComponent(
                        out IDamageable target))
                {
                    if (target.IsDead)
                        continue;

                    float distance =
                        Vector3.Distance(
                            transform.position,
                            hit.transform.position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = target;
                    }
                }
            }

            currentTarget = nearest;
        }

        protected virtual void Shoot()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning(
                    $"[Turret] {name}: No projectile prefab assigned.");
                return;
            }

            if (firePoint == null)
            {
                Debug.LogWarning(
                    $"[Turret] {name}: No fire point assigned.");
                return;
            }

            if (currentTarget == null)
                return;

            GameObject projectileObject =
                Instantiate(
                    projectilePrefab,
                    firePoint.position,
                    firePoint.rotation);

            if (projectileObject.TryGetComponent(
                    out Projectile projectile))
            {
                projectile.Initialize(
                    currentTarget,
                    damage);
            }

            recoilAmount = 1f;

            if (baseRenderer != null)
            {
                baseRenderer.GetPropertyBlock(basePropBlock);

                basePropBlock.SetFloat(
                    FireTimeID,
                    Time.time);

                baseRenderer.SetPropertyBlock(
                    basePropBlock);
            }
        }

        protected virtual void RotateTowardsTarget()
        {
            MonoBehaviour targetObject =
                currentTarget as MonoBehaviour;

            if (targetObject == null)
                return;

            Vector3 direction =
                targetObject.transform.position -
                turretHead.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            float targetYaw =
                Quaternion.LookRotation(direction)
                    .eulerAngles.y;

            float parentYaw =
                turretHead.parent != null
                    ? turretHead.parent.eulerAngles.y
                    : 0f;

            float localYaw =
                targetYaw -
                parentYaw +
                rotationOffset;

            Quaternion targetRotation =
                Quaternion.Euler(
                    defaultLocalX,
                    localYaw,
                    defaultLocalZ);

            turretHead.localRotation =
                Quaternion.RotateTowards(
                    turretHead.localRotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
        }

        protected virtual void UpdateActivationVisual()
        {
            if (Mathf.Approximately(
                    currentActivation,
                    targetActivation))
                return;

            currentActivation =
                Mathf.MoveTowards(
                    currentActivation,
                    targetActivation,
                    activationLerpSpeed *
                    Time.deltaTime);

            ApplyActivationVisual();
        }

        protected virtual void ApplyActivationVisual()
        {
            if (topRenderer != null)
            {
                topRenderer.GetPropertyBlock(
                    topPropBlock);

                topPropBlock.SetFloat(
                    ActivationID,
                    currentActivation);

                topRenderer.SetPropertyBlock(
                    topPropBlock);
            }

            if (baseRenderer != null)
            {
                baseRenderer.GetPropertyBlock(
                    basePropBlock);

                basePropBlock.SetFloat(
                    ActivationID,
                    currentActivation);

                baseRenderer.SetPropertyBlock(
                    basePropBlock);
            }
        }

        protected virtual void UpdateRecoil()
        {
            if (turretHead == null)
                return;

            if (recoilAmount <= 0f)
                return;

            recoilAmount =
                Mathf.MoveTowards(
                    recoilAmount,
                    0f,
                    recoilRecoverySpeed *
                    Time.deltaTime);

            Vector3 offset =
                turretHead.localRotation *
                (
                    recoilLocalDirection.normalized *
                    recoilDistance *
                    recoilAmount
                );

            turretHead.localPosition =
                headRestLocalPos + offset;
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

        public float Damage => damage;
        public float FireRate => fireRate;
        public float AttackRange => attackRange;

        protected void SetStats(
            int newDamage,
            float newFireRate,
            float newRange)
        {
<<<<<<< Updated upstream
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
                Debug.Log($"[Turret] Fired at target, damage: {damage}");
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
            Debug.Log($"[Turret] Took {amount} damage, HP: {currentHP}/{maxHP}");

            if (currentHP <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            Debug.Log("[Turret] Destroyed! Refunding power.");

            // Refund power to PowerManager
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.Refund(powerCost);
            }

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
=======
            damage = newDamage;
            fireRate = newFireRate;
            attackRange = newRange;
>>>>>>> Stashed changes
        }
    }
}