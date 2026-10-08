using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Turret: Defensive structure that automatically acquires and shoots enemies.
    ///
    /// Responsibilities:
    /// - Base turret combat, targeting and rotation
    /// - Operating power allocation/refund
    /// - Activation shader and recoil
    /// - Delegation of upgrade-specific behaviour to upgrade components
    /// </summary>
    public class Turret : DefensiveStructure, IDamageable
    {
        // ============================================================
        // TURRET COMBAT SETTINGS
        // ============================================================

        [Header("Turret Combat Settings")]
        [SerializeField] private int maxHP = 30;
        [SerializeField] private int damage = 5;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float attackRange = 8f;

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

        // ============================================================
        // TURRET ROTATION
        // ============================================================

        [Header("Turret Rotation")]
        [SerializeField] private Transform turretHead;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float rotationOffset = -90f;
        [SerializeField] private bool lockXRotation = true;

        // ============================================================
        // AUDIO
        // Audio clips/settings are shared by ALL turrets through
        // TurretAudioController. Each turret still owns its local
        // AudioSources so 3D spatial audio remains correct.

        private AudioSource fireAudioSource;
        private AudioSource rotationAudioSource;

        // Base references. Upgrade-specific transforms are supplied by
        // TurretUpgradeController.
        private Transform baseTurretHead;
        private Transform baseFirePoint;
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
        [SerializeField] private float activationLerpSpeed = 8f;

        // ============================================================
        // RECOIL
        // ============================================================

        [Header("Shooting Recoil")]
        [SerializeField] private float recoilDistance = 0.12f;
        [SerializeField] private float recoilRecoverySpeed = 6f;

        [SerializeField] private Vector3 recoilLocalDirection =
            new Vector3(0f, -1f, 0f);

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

        private float currentActivation;
        private float targetActivation;

        // ============================================================
        // RECOIL
        // ============================================================

        private Vector3 headRestLocalPos;
        private float recoilAmount;

        private float defaultLocalX;
        private float defaultLocalY;
        private float defaultLocalZ;

        // ============================================================
        // POWER
        // ============================================================

        /// <summary>
        /// Operating power cost for this individual turret.
        /// The value is changed by TurretUpgradeController when an
        /// upgrade changes the turret's operating cost.
        /// </summary>
        public int PowerCost => powerCost;

        /// <summary>
        /// Changes this turret's operating power cost.
        /// This only changes the stored cost; it does not allocate or
        /// refund power. Turret.TurnOn/TurnOff owns operating power.
        /// </summary>
        public void SetPowerCost(int newPowerCost)
        {
            if (newPowerCost < 0)
            {
                Debug.LogError(
                    "[Turret] Power cost cannot be negative."
                );
                return;
            }

            powerCost = newPowerCost;
        }

        // True when THIS turret currently owns its operating power.
        private bool holdingPower;

        // ============================================================
        // COMBAT STATE
        // ============================================================

        private int currentHP;
        private float fireTimer;

        private IDamageable currentTarget;

        /// <summary>
        /// Current target selected by this turret.
        /// Exposed so upgrade components can use the same target.
        /// </summary>
        public IDamageable CurrentTarget => currentTarget;

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

            SetupAudioSources();

            currentHP = maxHP;

            if (upgradeController == null)
            {
                upgradeController =
                    GetComponent<TurretUpgradeController>();
            }

            if (targetMask.value == 0)
            {
                targetMask = ~0;
            }

            // --------------------------------------------------------
            // Save the Inspector-assigned base references and find the
            // Double Barrel head automatically.
            // --------------------------------------------------------

            baseTurretHead = turretHead;
            baseFirePoint = firePoint;

            ApplyVariantTransforms();

            // --------------------------------------------------------
            // Rotation setup
            // --------------------------------------------------------

            if (turretHead != null)
            {
                defaultLocalX =
                    turretHead.localEulerAngles.x;

                defaultLocalY =
                    turretHead.localEulerAngles.y;

                defaultLocalZ =
                    turretHead.localEulerAngles.z;

                headRestLocalPos =
                    turretHead.localPosition;
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
            UpdateActivationVisual();

            UpdateRecoil();

            // --------------------------------------------------------
            // Combat
            // --------------------------------------------------------

            if (!isActive || IsDead)
            {
                StopRotationSound();
                return;
            }

            AcquireTarget();

            if (currentTarget != null &&
                turretHead != null)
            {
                RotateTowardsTarget();
            }
            else
            {
                StopRotationSound();
            }

            fireTimer += Time.deltaTime;

            float currentFireRate =
                GetCurrentFireRate();

            if (fireTimer >= currentFireRate &&
                currentTarget != null)
            {
                fireTimer = 0f;

                Shoot();
            }
        }

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
                    PlayPowerOnSound();
                    return;
                }

                if (PowerManager.Instance.TryAllocate(powerCost))
                {
                    holdingPower = true;
                    PlayPowerOnSound();
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
                PlayPowerOffSound();
            }
        }

        // ============================================================
        // TURN ON
        // ============================================================

        public override void TurnOn()
        {
            base.TurnOn();

            targetActivation = 1f;
            RefreshActiveVisualRenderers();

            // Ensure the active Def/DB model is immediately in its
            // powered-on visual state. Update() will continue to keep
            // it synchronized afterwards.
            currentActivation = 1f;
            ApplyActivationVisual();
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

        // ============================================================
        // ACTIVE VARIANT TRANSFORMS
        // ============================================================

        private void ApplyVariantTransforms()
        {
            Transform desiredHead = baseTurretHead;
            Transform desiredFirePoint = baseFirePoint;

            if (upgradeController != null)
            {
                desiredHead =
                    upgradeController.GetActiveTurretHead(
                        baseTurretHead
                    );

                desiredFirePoint =
                    upgradeController.GetActiveFirePoint(
                        baseFirePoint
                    );
            }

            if (desiredHead == null)
                return;

            bool headChanged =
                activeVariantHead != desiredHead;

            turretHead = desiredHead;
            firePoint =
                desiredFirePoint != null
                    ? desiredFirePoint
                    : desiredHead;

            // Only recache the authored rest transform when switching
            // between turret variants. Never reset the head every frame.
            if (headChanged)
            {
                activeVariantHead = desiredHead;

                defaultLocalX =
                    turretHead.localEulerAngles.x;

                defaultLocalY =
                    turretHead.localEulerAngles.y;

                defaultLocalZ =
                    turretHead.localEulerAngles.z;

                headRestLocalPos =
                    turretHead.localPosition;
            }
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

            bool preferFurthest =
                upgradeController != null &&
                upgradeController.PrefersFurthestTarget();

            float selectedDistance =
                preferFurthest
                    ? float.MinValue
                    : float.MaxValue;

            IDamageable selectedTarget =
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

                bool isBetterTarget =
                    preferFurthest
                        ? distance > selectedDistance
                        : distance < selectedDistance;

                if (isBetterTarget)
                {
                    selectedDistance = distance;
                    selectedTarget = target;
                }
            }

            currentTarget =
                selectedTarget;
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

            Quaternion previousRotation = turretHead.localRotation;

            turretHead.localRotation =
                Quaternion.RotateTowards(
                    turretHead.localRotation,
                    targetLocalRotation,
                    rotationSpeed *
                    Time.deltaTime
                );

            float rotationDelta =
                Quaternion.Angle(
                    previousRotation,
                    turretHead.localRotation
                );

            UpdateRotationSound(rotationDelta);
        }

        // ============================================================
        // AUDIO
        // ============================================================

        private void SetupAudioSources()
        {
            if (fireAudioSource == null)
            {
                fireAudioSource = gameObject.AddComponent<AudioSource>();
            }

            if (rotationAudioSource == null)
            {
                rotationAudioSource = gameObject.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(fireAudioSource, false);
            ConfigureAudioSource(rotationAudioSource, true);
        }

        private void ConfigureAudioSource(
            AudioSource source,
            bool looping
        )
        {
            if (source == null)
                return;

            source.playOnAwake = false;
            source.loop = looping;
            source.spatialBlend = 0f;
        }

        private TurretAudioController GetAudioController()
        {
            return TurretAudioController.Instance;
        }

        private void PlayFireSound()
        {
            TurretAudioController audio = GetAudioController();

            if (fireAudioSource == null || audio == null ||
                !audio.IsRoomAudioAudible(this))
                return;

            TurretVariant variant = TurretVariant.Base;

            if (upgradeController != null)
                variant = upgradeController.CurrentVariant;

            AudioClip clip = audio.GetFireSound(variant);

            if (clip == null)
                return;

            fireAudioSource.volume = audio.FireVolume;
            fireAudioSource.PlayOneShot(clip);
        }

        private void PlayPowerOnSound()
        {
            TurretAudioController audio = GetAudioController();

            if (fireAudioSource == null || audio == null ||
                audio.PowerOnSound == null ||
                !audio.IsRoomAudioAudible(this))
                return;

            fireAudioSource.volume = audio.PowerVolume;
            fireAudioSource.PlayOneShot(audio.PowerOnSound);
        }

        private void PlayPowerOffSound()
        {
            TurretAudioController audio = GetAudioController();

            if (fireAudioSource == null || audio == null ||
                audio.PowerOffSound == null ||
                !audio.IsRoomAudioAudible(this))
                return;

            fireAudioSource.volume = audio.PowerVolume;
            fireAudioSource.PlayOneShot(audio.PowerOffSound);
        }

        private void UpdateRotationSound(float rotationDelta)
        {
            TurretAudioController audio = GetAudioController();

            if (rotationAudioSource == null || audio == null)
                return;

            AudioClip clip = audio.RotationSound;
            float threshold = audio.RotationAudioThreshold;

            if (!audio.IsRoomAudioAudible(this) ||
                clip == null || rotationDelta < threshold)
            {
                StopRotationSound();
                return;
            }

            if (rotationAudioSource.clip != clip)
            {
                rotationAudioSource.Stop();
                rotationAudioSource.clip = clip;
            }

            rotationAudioSource.volume = audio.RotationVolume;
            rotationAudioSource.loop = true;

            if (!rotationAudioSource.isPlaying)
                rotationAudioSource.Play();
        }

        private void StopRotationSound()
        {
            if (rotationAudioSource == null)
                return;

            if (rotationAudioSource.isPlaying)
                rotationAudioSource.Stop();
        }

        private void OnDisable()
        {
            StopRotationSound();
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

            int currentDamage = GetCurrentDamage();

            // Upgrade-specific firing belongs to the active upgrade.
            // Base Turret keeps the original homing projectile.
            if (upgradeController != null &&
                upgradeController.HasActiveUpgrade())
            {
                if (!upgradeController.TryFireUpgrade(
                        this,
                        currentTarget,
                        currentDamage))
                {
                    return;
                }
            }
            else
            {
                FireProjectile(
                    firePoint.position,
                    firePoint.rotation,
                    currentDamage
                );
            }

            PlayFireSound();

            recoilAmount = 1f;
            ApplyFireEffect();
        }

        // ============================================================
        // STRAIGHT DOUBLE BARREL PROJECTILE
        // ============================================================

        public void FireStraightProjectile(
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
        // SHARPSHOOTER PROJECTILE
        // ============================================================

        public void FireSharpshooterProjectile(
            Vector3 position,
            Vector3 direction,
            int baseDamage,
            float projectileSpeed,
            float damagePerMetre,
            int maxDamage
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
                projectile.InitializeSharpshooter(
                    direction,
                    baseDamage,
                    projectileSpeed,
                    damagePerMetre,
                    maxDamage
                );

                if (verboseLogging)
                {
                    Debug.Log(
                        $"[Turret] Sharpshooter projectile fired. " +
                        $"Base Damage = {baseDamage}, " +
                        $"Speed = {projectileSpeed}"
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

        public Vector3 GetTargetAimPoint(IDamageable target)
        {
            MonoBehaviour targetBehaviour =
                target as MonoBehaviour;

            if (targetBehaviour == null)
            {
                return turretHead != null
                    ? turretHead.position + turretHead.forward
                    : transform.position + transform.forward;
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
            // Base turret uses its Inspector range. Upgrades may override
            // the range through TurretUpgradeController.
            if (upgradeController != null)
            {
                return Mathf.Max(
                    0f,
                    upgradeController.GetAttackRange(attackRange)
                );
            }

            return Mathf.Max(0f, attackRange);
        }

        // ============================================================
        // ACTIVE VISUAL RENDERERS
        // ============================================================

        /// <summary>
        /// Refreshes the renderer list used by the activation and fire
        /// effects after the active turret visual changes.
        /// </summary>
        public void RefreshActiveVisuals()
        {
            RefreshActiveVisualRenderers();
        }

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

                Material sharedMaterial =
                    renderer.sharedMaterial;

                if (sharedMaterial == null ||
                    !sharedMaterial.HasProperty(activationShaderID))
                {
                    continue;
                }

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

            float currentRecoilDistance =
                upgradeController != null
                    ? upgradeController.GetRecoilDistance(
                        recoilDistance
                    )
                    : recoilDistance;

            float currentRecoilRecoverySpeed =
                upgradeController != null
                    ? upgradeController.GetRecoilRecoverySpeed(
                        recoilRecoverySpeed
                    )
                    : recoilRecoverySpeed;

            Vector3 currentRecoilDirection =
                upgradeController != null
                    ? upgradeController.GetRecoilLocalDirection(
                        recoilLocalDirection
                    )
                    : recoilLocalDirection;

            recoilAmount =
                Mathf.MoveTowards(
                    recoilAmount,
                    0f,
                    currentRecoilRecoverySpeed *
                    Time.deltaTime
                );

            // Recoil directions are deliberately expressed in the turret head's
            // PARENT local space, because turretHead.localPosition uses that space.
            // This avoids the model/head rotation (including the SS -90 degree
            // authoring rotation) turning vertical recoil into sideways movement.
            Vector3 recoilOffset =
                currentRecoilDirection.normalized *
                currentRecoilDistance *
                recoilAmount;

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

            if (verboseLogging)
            {
                Debug.Log(
                    $"[Turret] Took {amount} damage. " +
                    $"HP = {currentHP}/{maxHP}"
                );
            }

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

            Destroy(gameObject);
        }

        // ============================================================
        // DEBUG
        // ============================================================

        [ContextMenu("Test - Take 10 Damage")]
        private void TestTakeDamage()
        {
            TakeDamage(10);
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

            if (currentTarget != null &&
                turretHead != null)
            {
                Gizmos.DrawLine(
                    turretHead.position,
                    GetTargetAimPoint(currentTarget)
                );
            }
        }
    }
}