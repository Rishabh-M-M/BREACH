using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
<<<<<<< Updated upstream
    /// Base turret weapon.
    ///
    /// Turrets no longer have individual HP.
    /// The room shield determines whether the room's defensive structures
    /// can remain operational.
    /// </summary>
    public class Turret : DefensiveStructure
=======
    /// Turret: Defensive structure that automatically acquires and shoots enemies.
    ///
    /// Supports:
    /// - Base turret
    /// - Turret upgrades
    /// - Double Barrel firing
    /// - Operating power allocation/refund
    /// - Automatic ON/OFF activation shader
    /// - Existing turret targeting/rotation/projectile system
    /// </summary>
    public class Turret : DefensiveStructure, IDamageable
>>>>>>> Stashed changes
    {
        // ============================================================
        // TURRET COMBAT SETTINGS
        // ============================================================

        [Header("Turret Combat Settings")]
<<<<<<< Updated upstream
        [SerializeField] private int maxHP = 30;
        [SerializeField] private int damage = 5;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float attackRange = 8f;
<<<<<<< Updated upstream
=======
        [SerializeField] protected int damage = 5;
        [SerializeField] protected float fireRate = 1f;
        [SerializeField] protected float attackRange = 8f;
>>>>>>> Stashed changes

        [Header("References")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform firePoint;
=======

        // ============================================================
        // TARGETING
        // ============================================================

        [Header("Target Filtering")]
        [Tooltip("Collider layers considered for targeting.")]
        [SerializeField] private LayerMask targetMask = ~0;

        // ============================================================
        // PROJECTILE
        // ============================================================

        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;

        [Tooltip(
            "Main firing point. Double Barrel uses this same point " +
            "and creates two projectiles with an adjustable horizontal gap."
        )]
        [SerializeField] private Transform firePoint;
>>>>>>> Stashed changes

        // ============================================================
        // DOUBLE BARREL
        // ============================================================

        [Header("Double Barrel Projectile Spawners")]
        [Tooltip(
            "Projectile spawner at the centre of the LEFT DB muzzle."
        )]
        [SerializeField] private Transform doubleBarrelLeftSpawner;

        [Tooltip(
            "Projectile spawner at the centre of the RIGHT DB muzzle."
        )]
        [SerializeField] private Transform doubleBarrelRightSpawner;

        // ============================================================
        // TURRET ROTATION
        // ============================================================

        [Header("Turret Rotation")]
<<<<<<< Updated upstream
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

=======
        [SerializeField] private Transform turretHead;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float rotationOffset = -90f;
        [SerializeField] private bool lockXRotation = true;

        // Runtime references for the installed turret variant.
        private Transform baseTurretHead;
        private Transform baseFirePoint;
        private Transform doubleBarrelHead;
        private Transform activeVariantHead;

        // ============================================================
        // ACTIVATION SHADER
        // ============================================================

        [Header("Activation Shader")]
        [Tooltip(
            "Shader property used to control turret ON/OFF appearance."
        )]
        [SerializeField] private string activationShaderProperty =
            "_Activation";

        [Tooltip(
            "How quickly the ON/OFF shader transitions."
        )]
        [SerializeField] private float activationLerpSpeed = 4f;

        // ============================================================
        // RECOIL
        // ============================================================

        [Header("Shooting Recoil")]
        [SerializeField] private float recoilDistance = 0.12f;
        [SerializeField] private float recoilRecoverySpeed = 6f;

        [SerializeField] private Vector3 recoilLocalDirection =
            new Vector3(0f, 0f, -1f);

        // ============================================================
        // UPGRADE
        // ============================================================

        [Header("Upgrade")]
        [SerializeField] private TurretUpgradeController upgradeController;

        // ============================================================
        // SHADER
        // ============================================================

        private int activationShaderID;
        private static readonly int FireTimeID =
            Shader.PropertyToID("_FireTime");

        private Renderer[] activeVisualRenderers;
        private MaterialPropertyBlock activationPropertyBlock;
        private MaterialPropertyBlock firePropertyBlock;

>>>>>>> Stashed changes
        private float currentActivation;
        private float targetActivation;

        // ============================================================
        // RECOIL
        // ============================================================

        private Vector3 headRestLocalPos;
<<<<<<< Updated upstream
        protected float recoilAmount;

        private float defaultLocalX;
        private float defaultLocalZ;

<<<<<<< Updated upstream
=======
        private float recoilAmount;

        private float defaultLocalX;
        private float defaultLocalY;
        private float defaultLocalZ;

        // ============================================================
        // POWER
        // ============================================================

        // True when THIS turret currently owns its operating power.
        private bool holdingPower;

        // ============================================================
        // COMBAT STATE
        // ============================================================

>>>>>>> Stashed changes
        private int currentHP;
        private float fireTimer;

        private IDamageable currentTarget;

        // ============================================================
        // DEBUG
        // ============================================================

        private static bool verboseLogging = false;

        // ============================================================
        // IDAMAGEABLE
        // ============================================================

        public int MaxHP => maxHP;

        public int CurrentHP => currentHP;

        public bool IsDead => currentHP <= 0;

        // ============================================================
        // AWAKE
        // ============================================================

        protected override void Awake()
        {
            base.Awake();

            currentHP = maxHP;

<<<<<<< Updated upstream
            if (turretHead != null)
=======
            if (upgradeController == null)
            {
                upgradeController =
                    GetComponent<TurretUpgradeController>();
            }

            if (targetMask.value == 0)
>>>>>>> Stashed changes
            {
                defaultLocalX = turretHead.localEulerAngles.x;   // 缓存 -90
                defaultLocalZ = turretHead.localEulerAngles.z;   // 缓存 0
=======
        protected override void Awake()
        {
            base.Awake();

            // --------------------------------------------------------
            // Save the Inspector-assigned base references and find the
            // Double Barrel head automatically.
            // --------------------------------------------------------

            baseTurretHead = turretHead;
            baseFirePoint = firePoint;
            doubleBarrelHead = FindChildTransform(transform, "DB head");

            ApplyVariantTransforms();

            // --------------------------------------------------------
            // Rotation setup
            // --------------------------------------------------------

            if (turretHead != null)
            {
<<<<<<< Updated upstream
                defaultLocalX = turretHead.localEulerAngles.x;
                defaultLocalZ = turretHead.localEulerAngles.z;
>>>>>>> Stashed changes
                headRestLocalPos = turretHead.localPosition;
=======
                defaultLocalX =
                    turretHead.localEulerAngles.x;

                defaultLocalY =
                    turretHead.localEulerAngles.y;

                defaultLocalZ =
                    turretHead.localEulerAngles.z;

                headRestLocalPos =
                    turretHead.localPosition;
>>>>>>> Stashed changes
            }

            // --------------------------------------------------------
            // Shader setup
            // --------------------------------------------------------

            activationShaderID =
                Shader.PropertyToID(
                    activationShaderProperty
                );

            activationPropertyBlock =
                new MaterialPropertyBlock();

            firePropertyBlock =
                new MaterialPropertyBlock();

            // --------------------------------------------------------
            // Initial activation state
            // --------------------------------------------------------

            targetActivation =
                isActive ? 1f : 0f;

            currentActivation =
                targetActivation;
        }

        // ============================================================
        // START
        // ============================================================

        private void Start()
        {
            /*
             * Start runs after all Awake calls.
             *
             * This is important because TurretUpgradeController also
             * switches the Def / DB visual in Awake().
             */

            RefreshActiveVisualRenderers();

<<<<<<< Updated upstream
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
=======
            ApplyActivationVisual();
        }

        // ============================================================
        // UPDATE
        // ============================================================

        private void Update()
        {
            // --------------------------------------------------------
            // Power
            // --------------------------------------------------------

            ReconcilePowerState();

            // --------------------------------------------------------
            // Visuals
            // --------------------------------------------------------

            ApplyVariantTransforms();
>>>>>>> Stashed changes
            UpdateActivationVisual();

            UpdateRecoil();

<<<<<<< Updated upstream
            if (!isActive)
=======
            // --------------------------------------------------------
            // Combat
            // --------------------------------------------------------

            if (!isActive || IsDead)
>>>>>>> Stashed changes
                return;

            AcquireTarget();

<<<<<<< Updated upstream
            if (currentTarget != null && turretHead != null)
=======
            if (currentTarget != null &&
                turretHead != null)
            {
>>>>>>> Stashed changes
                RotateTowardsTarget();

            fireTimer += Time.deltaTime;

<<<<<<< Updated upstream
            if (fireTimer >= fireRate && currentTarget != null)
=======
            float currentFireRate =
                GetCurrentFireRate();

            if (fireTimer >= currentFireRate &&
                currentTarget != null)
>>>>>>> Stashed changes
            {
                fireTimer = 0f;

                Shoot();
            }
        }

<<<<<<< Updated upstream
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
=======
        // ============================================================
        // POWER
        // ============================================================

        private void ReconcilePowerState()
        {
            if (IsDead)
                return;

            // --------------------------------------------------------
            // Turret ON
            // --------------------------------------------------------

            if (isActive && !holdingPower)
            {
                if (PowerManager.Instance == null)
                {
                    /*
                     * Allows turret testing in scenes where the
                     * PowerManager isn't present.
                     */
                    holdingPower = true;
                    return;
                }

                if (PowerManager.Instance.TryAllocate(powerCost))
                {
                    holdingPower = true;
                }
                else
                {
                    // Not enough operating power.
                    isActive = false;

                    UIManager.Instance?.ShowMessage(
                        "Insufficient power!"
                    );
                }
            }

            // --------------------------------------------------------
            // Turret OFF
            // --------------------------------------------------------

            else if (!isActive && holdingPower)
            {
                if (PowerManager.Instance != null)
                {
                    PowerManager.Instance.Refund(
                        powerCost
                    );
                }

                holdingPower = false;
            }
        }

        // ============================================================
        // TURN ON
        // ============================================================
>>>>>>> Stashed changes

        public override void TurnOn()
        {
            base.TurnOn();

            targetActivation = 1f;

            RefreshActiveVisualRenderers();
        }

        // ============================================================
        // TURN OFF
        // ============================================================

        public override void TurnOff()
        {
            /*
             * VERY IMPORTANT:
             *
             * If the turret is Base:
             *
             *     Operating cost = 30
             *
             * After upgrade:
             *
             *     Operating cost = 50
             *
             * We MUST refund the currently held cost BEFORE the
             * upgrade controller changes PowerCost from 30 -> 50.
             *
             * base.TurnOff() changes isActive to false.
             * ReconcilePowerState() then refunds the old cost.
             */

            base.TurnOff();

            ReconcilePowerState();

            targetActivation = 0f;

            RefreshActiveVisualRenderers();
        }

<<<<<<< Updated upstream
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
=======
        // ============================================================
        // ACTIVE VARIANT TRANSFORMS
        // ============================================================

        private void ApplyVariantTransforms()
        {
            bool isDoubleBarrel =
                upgradeController != null &&
                upgradeController.FiresTwoProjectiles();
>>>>>>> Stashed changes

            Transform desiredHead =
                isDoubleBarrel && doubleBarrelHead != null
                    ? doubleBarrelHead
                    : baseTurretHead;

            if (desiredHead == null)
                return;

            // Only recache the rest state when the actual active head
            // changes. Do NOT reset the rotation every frame.
            if (activeVariantHead == desiredHead)
                return;

            activeVariantHead = desiredHead;

            turretHead = desiredHead;
            firePoint =
                isDoubleBarrel && doubleBarrelHead != null
                    ? doubleBarrelHead
                    : baseFirePoint;

            defaultLocalX =
                turretHead.localEulerAngles.x;

            defaultLocalY =
                turretHead.localEulerAngles.y;

            defaultLocalZ =
                turretHead.localEulerAngles.z;

            headRestLocalPos =
                turretHead.localPosition;
        }

        private Transform FindChildTransform(
            Transform root,
            string childName)
        {
            if (root == null)
                return null;

            Transform[] children =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
<<<<<<< Updated upstream
                if (hit.gameObject == this.gameObject) continue;

                if (hit.TryGetComponent(out IDamageable target) && !target.IsDead)
=======
                if (child != root &&
                    child.name == childName)
>>>>>>> Stashed changes
                {
                    return child;
                }
            }

            return null;
        }

        // ============================================================
        // TARGET ACQUISITION
        // ============================================================

        private void AcquireTarget()
        {
            float currentRange =
                GetCurrentAttackRange();

            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    currentRange,
                    targetMask
                );

            float nearestDistance =
                float.MaxValue;

            IDamageable nearestTarget =
                null;

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                // Never target any collider belonging to this turret.
                if (hit.transform == transform ||
                    hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                // Enemy colliders can be children of the EnemyHealth
                // object, so resolve the enemy from the collider's parent.
                EnemyHealth enemy =
                    hit.GetComponentInParent<EnemyHealth>();

                if (enemy == null)
                    continue;

                IDamageable target = enemy;

                if (target.IsDead)
                    continue;

                // Measure range from the turret to the enemy root, not
                // to an arbitrary child collider.
                float distance =
                    Vector3.Distance(
                        transform.position,
                        enemy.transform.position
                    );

                if (distance > currentRange)
                    continue;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = target;
                }
            }

            currentTarget =
                nearestTarget;
        }

        // ============================================================
        // ROTATION
        // ============================================================

        private void RotateTowardsTarget()
        {
            MonoBehaviour targetBehaviour =
                currentTarget as MonoBehaviour;

            if (targetBehaviour == null)
                return;

            Vector3 direction =
                GetTargetAimPoint(currentTarget) -
                turretHead.position;

            if (lockXRotation)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetWorldRotation =
                Quaternion.LookRotation(direction);

            float targetYaw =
                targetWorldRotation.eulerAngles.y;

            float parentYaw =
                turretHead.parent != null
                    ? turretHead.parent.eulerAngles.y
                    : 0f;

            // Preserve the active head's authored local Y orientation.
            // The DB head can have a different model-forward orientation
            // from the base head; discarding that authored Y rotation is
            // what caused the DB model to snap by 90 degrees.
            float localYaw =
                targetYaw -
                parentYaw +
                rotationOffset +
                defaultLocalY;

            Quaternion targetLocalRotation;

            if (lockXRotation)
            {
                targetLocalRotation =
                    Quaternion.Euler(
                        defaultLocalX,
                        localYaw,
                        defaultLocalZ
                    );
            }
            else
            {
                targetLocalRotation =
                    Quaternion.Euler(
                        0f,
                        localYaw,
                        0f
                    );
            }

            turretHead.localRotation =
                Quaternion.RotateTowards(
                    turretHead.localRotation,
                    targetLocalRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        // ============================================================
        // SHOOT
        // ============================================================

        private void Shoot()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning(
                    "[Turret] No projectile prefab assigned!"
                );

                return;
            }

            if (firePoint == null)
            {
                Debug.LogWarning(
                    "[Turret] No Fire Point assigned!"
                );

                return;
            }

            if (currentTarget == null)
                return;

            int currentDamage =
                GetCurrentDamage();

            // ========================================================
            // DOUBLE BARREL
            // ========================================================

            if (upgradeController != null &&
                upgradeController.FiresTwoProjectiles())
            {
<<<<<<< Updated upstream
                proj.Initialize(currentTarget, damage);  // Use Initialize, not Init
                Debug.Log($"[Turret] Fired at target, damage: {damage}");
=======
                if (doubleBarrelLeftSpawner == null ||
                    doubleBarrelRightSpawner == null)
                {
                    Debug.LogWarning(
                        $"[Turret] {name}: Double Barrel projectile " +
                        "spawners are not assigned. Assign both " +
                        "Left Spawner and Right Spawner in the Inspector."
                    );

                    return;
                }

                // Each barrel has its own physical projectile spawner.
                Vector3 leftMuzzle =
                    doubleBarrelLeftSpawner.position;

                Vector3 rightMuzzle =
                    doubleBarrelRightSpawner.position;

                // The midpoint is the centre line of the two barrels.
                // It determines ONE firing direction for both shots.
                Vector3 muzzleCenter =
                    (leftMuzzle + rightMuzzle) * 0.5f;

                Vector3 targetAimPoint =
                    GetTargetAimPoint(currentTarget);

                Vector3 firingDirection =
                    targetAimPoint - muzzleCenter;

                if (firingDirection.sqrMagnitude < 0.0001f)
                {
                    firingDirection =
                        doubleBarrelHead != null
                            ? doubleBarrelHead.forward
                            : firePoint.forward;
                }
                else
                {
                    firingDirection.Normalize();
                }

                // Both projectiles start at their own muzzle, but both
                // travel in exactly the same straight direction.
                FireStraightProjectile(
                    leftMuzzle,
                    firingDirection,
                    currentDamage
                );

                FireStraightProjectile(
                    rightMuzzle,
                    firingDirection,
                    currentDamage
                );
>>>>>>> Stashed changes
            }

            // ========================================================
            // BASE TURRET
            // ========================================================

            else
            {
                FireProjectile(
                    firePoint.position,
                    firePoint.rotation,
                    currentDamage
                );
            }

            // Recoil.
            recoilAmount = 1f;

            // Fire shader ripple.
            ApplyFireEffect();
        }

        // ============================================================
        // STRAIGHT DOUBLE BARREL PROJECTILE
        // ============================================================

        private void FireStraightProjectile(
            Vector3 position,
            Vector3 direction,
            int projectileDamage
        )
        {
            GameObject projectileObject =
                Instantiate(
                    projectilePrefab,
                    position,
                    Quaternion.LookRotation(direction)
                );

            if (projectileObject == null)
                return;

            if (projectileObject.TryGetComponent(
                    out Projectile projectile))
            {
                projectile.InitializeStraight(
                    direction,
                    projectileDamage
                );

                if (verboseLogging)
                {
                    Debug.Log(
                        $"[Turret] Double Barrel straight projectile fired. " +
                        $"Damage = {projectileDamage}"
                    );
                }
            }
            else
            {
                Debug.LogError(
                    $"[Turret] {name}: Projectile prefab does not " +
                    "have a Projectile component."
                );
            }
        }

        // ============================================================
        // TARGET AIM POINT FOR DOUBLE BARREL CENTER LINE
        // ============================================================

        private Vector3 GetTargetAimPoint(IDamageable target)
        {
            MonoBehaviour targetBehaviour =
                target as MonoBehaviour;

            if (targetBehaviour == null)
            {
                return doubleBarrelHead != null
                    ? doubleBarrelHead.position + doubleBarrelHead.forward
                    : firePoint.position + firePoint.forward;
            }

            // The IDamageable is normally EnemyHealth on the enemy root,
            // while the collider may be on a child. Find the actual enemy
            // collider so the centre-line aims at the enemy body.
            Collider targetCollider =
                targetBehaviour.GetComponentInChildren<Collider>();

            if (targetCollider != null)
            {
                return targetCollider.bounds.center;
            }

            return targetBehaviour.transform.position;
        }

        // ============================================================
        // FIRE PROJECTILE
        // ============================================================

        private void FireProjectile(
            Vector3 position,
            Quaternion rotation,
            int projectileDamage
        )
        {
            GameObject projectileObject =
                Instantiate(
                    projectilePrefab,
                    position,
                    rotation
                );

            if (projectileObject.TryGetComponent(
                    out Projectile projectile))
            {
                projectile.Initialize(
                    currentTarget,
                    projectileDamage
                );

                if (verboseLogging)
                {
                    Debug.Log(
                        $"[Turret] Projectile fired. " +
                        $"Damage = {projectileDamage}"
                    );
                }
            }
        }

        // ============================================================
        // FIRE SHADER EFFECT
        // ============================================================

        private void ApplyFireEffect()
        {
            /*
             * Only affect the currently active visual.
             *
             * Each visual keeps its own material and texture.
             */

            if (activeVisualRenderers == null)
                return;

            foreach (Renderer renderer in activeVisualRenderers)
            {
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(
                    firePropertyBlock
                );

                firePropertyBlock.SetFloat(
                    FireTimeID,
                    Time.time
                );

                renderer.SetPropertyBlock(
                    firePropertyBlock
                );
            }
        }

        // ============================================================
        // UPGRADE STATS
        // ============================================================

        private int GetCurrentDamage()
        {
            if (upgradeController != null)
            {
                return upgradeController.GetProjectileDamage(
                    damage
                );
            }

            return damage;
        }

        private float GetCurrentFireRate()
        {
            if (upgradeController != null)
            {
                return upgradeController.GetFireRate(
                    fireRate
                );
            }

            return fireRate;
        }

        private float GetCurrentAttackRange()
        {
            // The Turret Inspector value is the authoritative attack
            // range for this turret. This prevents the Double Barrel
            // upgrade's old hard-coded range from overriding a value such
            // as 80 or 240 set on the actual turret.
            return Mathf.Max(0f, attackRange);
        }

        // ============================================================
        // ACTIVE VISUAL RENDERERS
        // ============================================================

        private void RefreshActiveVisualRenderers()
        {
            /*
             * We deliberately DON'T assign specific Def/DB renderers
             * in the Inspector.
             *
             * TurretUpgradeController switches:
             *
             *     Def
             *     OR
             *     DB
             *
             * on/off.
             *
             * We then find the renderers that are actually active.
             *
             * This means Def and DB can use completely different
             * materials and textures.
             */

            Renderer[] allRenderers =
                GetComponentsInChildren<Renderer>(
                    true
                );

            int activeCount = 0;

            foreach (Renderer renderer in allRenderers)
            {
                if (renderer == null)
                    continue;

                if (renderer.gameObject.activeInHierarchy)
                {
                    activeCount++;
                }
            }

            activeVisualRenderers =
                new Renderer[activeCount];

            int index = 0;

            foreach (Renderer renderer in allRenderers)
            {
                if (renderer == null)
                    continue;

                if (!renderer.gameObject.activeInHierarchy)
                    continue;

                activeVisualRenderers[index] =
                    renderer;

                index++;
            }
        }

        // ============================================================
        // ACTIVATION SHADER
        // ============================================================

        private void UpdateActivationVisual()
        {
            targetActivation =
                isActive ? 1f : 0f;

            currentActivation =
                Mathf.MoveTowards(
                    currentActivation,
                    targetActivation,
                    activationLerpSpeed *
                    Time.deltaTime
                );

            ApplyActivationVisual();
        }

        private void ApplyActivationVisual()
        {
            if (activeVisualRenderers == null)
                return;

            foreach (Renderer renderer in activeVisualRenderers)
            {
                if (renderer == null)
                    continue;

                /*
                 * IMPORTANT:
                 *
                 * GetPropertyBlock() starts from the renderer's
                 * existing material properties.
                 *
                 * We only change _Activation.
                 *
                 * Therefore:
                 *
                 * Def material + Def texture stay intact.
                 * DB material + DB texture stay intact.
                 */

                renderer.GetPropertyBlock(
                    activationPropertyBlock
                );

                activationPropertyBlock.SetFloat(
                    activationShaderID,
                    currentActivation
                );

                renderer.SetPropertyBlock(
                    activationPropertyBlock
                );
            }
        }

        // ============================================================
        // RECOIL
        // ============================================================

        private void UpdateRecoil()
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
                    Time.deltaTime
                );

            Vector3 recoilOffset =
                turretHead.localRotation *
                (
                    recoilLocalDirection.normalized *
                    recoilDistance *
                    recoilAmount
                );

            turretHead.localPosition =
                headRestLocalPos +
                recoilOffset;
        }

        // ============================================================
        // DAMAGE
        // ============================================================

        public void TakeDamage(int amount)
        {
            if (IsDead)
                return;

            currentHP -= amount;
<<<<<<< Updated upstream
            Debug.Log($"[Turret] Took {amount} damage, HP: {currentHP}/{maxHP}");
=======

            if (verboseLogging)
            {
                Debug.Log(
                    $"[Turret] Took {amount} damage. " +
                    $"HP = {currentHP}/{maxHP}"
                );
            }
>>>>>>> Stashed changes

            if (currentHP <= 0)
            {
                currentHP = 0;
                Die();
            }
        }

        // ============================================================
        // DIE
        // ============================================================

        public void Die()
        {
<<<<<<< Updated upstream
            Debug.Log("[Turret] Destroyed! Refunding power.");

            // Refund power to PowerManager
            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.Refund(powerCost);
            }

            // Notify owner room
            OwnerRoom?.NotifyStructureDestroyed(this);
=======
            if (holdingPower)
            {
                ReconcilePowerState();
            }

            Debug.Log(
                $"[Turret] {name} destroyed."
            );

            OwnerRoom?.NotifyStructureDestroyed(
                this
            );
>>>>>>> Stashed changes

            Destroy(gameObject);
        }

        // ============================================================
        // DEBUG
        // ============================================================

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

        [ContextMenu("Test - Refresh Visual Renderers")]
        private void TestRefreshVisualRenderers()
        {
            RefreshActiveVisualRenderers();

            ApplyActivationVisual();

            Debug.Log(
                $"[Turret] Active visual renderers: " +
                $"{activeVisualRenderers?.Length ?? 0}"
            );
        }

        // ============================================================
        // GIZMOS
        // ============================================================

        private void OnDrawGizmosSelected()
        {
            float range =
                Mathf.Max(0f, attackRange);

            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                transform.position,
                range
            );

            // Show the two physical DB projectile spawners and
            // their centre firing line.
            if (upgradeController != null &&
                upgradeController.FiresTwoProjectiles() &&
                doubleBarrelLeftSpawner != null &&
                doubleBarrelRightSpawner != null)
            {
                Vector3 left =
                    doubleBarrelLeftSpawner.position;

                Vector3 right =
                    doubleBarrelRightSpawner.position;

                Vector3 center =
                    (left + right) * 0.5f;

                Gizmos.color =
                    Color.yellow;

                Gizmos.DrawSphere(left, 0.05f);
                Gizmos.DrawSphere(right, 0.05f);
                Gizmos.DrawSphere(center, 0.035f);
                Gizmos.DrawLine(left, right);

                if (currentTarget != null)
                {
                    Gizmos.DrawLine(
                        center,
                        GetTargetAimPoint(currentTarget)
                    );
                }
            }
        }
    }
}