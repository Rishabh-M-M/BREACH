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
        [Header("Rocket Launcher")]
        [Tooltip("Eight muzzle transforms ordered to match the launcher tubes.")]
        [SerializeField] private Transform[] rocketFiringPoints = new Transform[8];
        [SerializeField] private float rocketProjectileSpeed = 16f;
        [SerializeField] private float rocketBlastRadius = 3.5f;
        [SerializeField] private float rocketReloadDuration = 4f;
        [SerializeField] private GameObject blueRocketExplosionPrefab;
        [Tooltip("Separate dark-hole visual objects. Do NOT assign the firing-point GameObjects here.")]
        [SerializeField] private GameObject[] rocketSpentHoleVisuals = new GameObject[8];
        private int nextRocketTube;
        private float rocketReloadTimer;
        private bool rocketReloading;

        // ============================================================
        // TURRET ROTATION

        // ============================================================
        [Header("Turret Rotation")]
        [SerializeField] private Transform turretHead;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float rotationOffset = -90f;
        [SerializeField] private bool lockXRotation = false;

        [Tooltip("Extra upward visual tilt (degrees) when aiming at a target above the turret. Set to 0 to disable.")]
        [SerializeField, Range(0f, 15f)] private float upwardAimTilt = 5f;

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
        private RocketLauncherUpgrade rocketLauncherUpgrade;

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
        private float fireTimer;
        private IDamageable currentTarget;
        /// <summary>
        /// Current target selected by this turret.
        /// Exposed so upgrade components can use the same target.
        /// </summary>
        public IDamageable CurrentTarget => currentTarget;
        // Exposed to continuous-fire upgrades without changing normal targeting.
        public Transform ActiveFirePoint => firePoint;

        // ============================================================
        // DEBUG

        // ============================================================
        private static bool verboseLogging = false;

        // ============================================================
        // IDAMAGEABLE

        // ============================================================
        // Turrets no longer have health; they are disabled only by room shield failure.
        public int MaxHP => int.MaxValue;
        public int CurrentHP => int.MaxValue;
        public bool IsDead => false;

        // ============================================================
        // AWAKE

        // ============================================================
        protected override void Awake()
        {
            base.Awake();
            SetupAudioSources();
            if (upgradeController == null)
            {
                upgradeController =
                    GetComponent<TurretUpgradeController>();
            }
            rocketLauncherUpgrade = GetComponentInChildren<RocketLauncherUpgrade>(true);
            if (rocketLauncherUpgrade == null)
                rocketLauncherUpgrade = GetComponent<RocketLauncherUpgrade>();
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
            // Do not allow a spent-tube visual reference to disable a firing
            // point (or a parent containing one). Valid spent visuals start hidden.
            ValidateRocketTubeVisuals();
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
            UpdateRocketReload();
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
            if (!isActive || (OwnerRoom != null && OwnerRoom.IsPermanentlyDisabled))
            {
                StopRotationSound();
                if (upgradeController != null && upgradeController.ActiveHeliosIncinerator != null)
                    upgradeController.ActiveHeliosIncinerator.StopForTurretOff();
                return;
            }
            // Helios Incinerator owns its target lock, continuous DPS ramp, and
            // overheat cycle. Keep the existing projectile path untouched for
            // Base, Double Barrel, and Sharpshooter.
            if (upgradeController != null && upgradeController.IsHeliosIncinerator)
            {
                HeliosIncineratorUpgrade laser = upgradeController.ActiveHeliosIncinerator;
                if (laser != null)
                {
                    laser.Tick(this, Time.deltaTime);
                    currentTarget = laser.LockedTarget;
                }
                if (currentTarget != null && turretHead != null)
                    RotateTowardsTarget();
                else
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
            float currentFireRate = GetCurrentFireRate();
            if (fireTimer >= currentFireRate && currentTarget != null)
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
            // A permanently disabled room cannot reactivate this turret.
            // Still release any operating power it was holding.
            if (OwnerRoom != null && OwnerRoom.IsPermanentlyDisabled)
            {
                if (isActive)
                    base.TurnOff();
                if (holdingPower)
                {
                    if (PowerManager.Instance != null)
                        PowerManager.Instance.Refund(powerCost);
                    holdingPower = false;
                    PlayPowerOffSound();
                }
                targetActivation = 0f;
                return;
            }
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
        public void PermanentlyDisableFromRoom()
        {
            // Keep this component enabled so Update() can animate the OFF
            // shader. The Room flag and TurnOn() guard prevent reactivation.
            base.TurnOff();
            targetActivation = 0f;
            ReconcilePowerState();
            RefreshActiveVisualRenderers();
        }

        public override void TurnOn()
        {
            if (OwnerRoom != null && OwnerRoom.IsPermanentlyDisabled) return;
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
            MonoBehaviour targetBehaviour = currentTarget as MonoBehaviour;
            if (targetBehaviour == null || turretHead == null)
                return;
            // Aim at the centre of the enemy collider so flying enemies are
            // tracked at their actual height rather than at ground level.
            Vector3 worldDirection =
                GetTargetAimPoint(currentTarget) - turretHead.position;
            if (lockXRotation)
                worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 0.0001f)
                return;
            worldDirection.Normalize();
            float targetWorldYaw =
                Quaternion.LookRotation(worldDirection, Vector3.up).eulerAngles.y;
            float parentYaw = turretHead.parent != null
                ? turretHead.parent.eulerAngles.y
                : 0f;
            // Keep the model's authored yaw and configured forward-axis offset.
            float localYaw =
                targetWorldYaw - parentYaw + rotationOffset + defaultLocalY;
            float localPitch = defaultLocalX;
            if (!lockXRotation)
            {
                // Convert the aim direction into the head parent's local space.
                // The negative pitch sign is intentional: in Unity, rotating
                // around local +X pitches a forward-facing barrel downward.
                Vector3 localDirection = turretHead.parent != null
                    ? turretHead.parent.InverseTransformDirection(worldDirection)
                    : worldDirection;
                float horizontalMagnitude = new Vector2(
                    localDirection.x, localDirection.z).magnitude;
                float pitchDegrees = Mathf.Atan2(
                    localDirection.y,
                    Mathf.Max(horizontalMagnitude, 0.0001f)) * Mathf.Rad2Deg;
                // Add a small extra upward tilt only when the target is above
                // the turret. This changes the head's visual aim, not projectile
                // direction or target position. Negative local X pitches the
                // forward-facing barrel upward in this model's rotation convention.
                float extraUpwardTilt = worldDirection.y > 0.01f
                    ? upwardAimTilt
                    : 0f;

                localPitch = defaultLocalX - pitchDegrees - extraUpwardTilt;
            }
            Quaternion targetLocalRotation = Quaternion.Euler(
                localPitch,
                localYaw,
                defaultLocalZ);
            Quaternion previousRotation = turretHead.localRotation;
            turretHead.localRotation = Quaternion.RotateTowards(
                turretHead.localRotation,
                targetLocalRotation,
                rotationSpeed * Time.deltaTime);
            float rotationDelta = Quaternion.Angle(
                previousRotation,
                turretHead.localRotation);
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
            if (fireAudioSource == null)
                return;

            TurretAudioController audio = GetAudioController();
            TurretVariant variant = upgradeController != null
                ? upgradeController.CurrentVariant
                : TurretVariant.Base;

            // Preserve the game's camera/room audio routing when the manager exists.
            if (audio != null && !audio.IsRoomAudioAudible(this))
                return;

            if (variant == TurretVariant.RocketLauncher)
            {
                if (rocketLauncherUpgrade == null)
                    rocketLauncherUpgrade = GetComponentInChildren<RocketLauncherUpgrade>(true);

                AudioClip rocketClip = rocketLauncherUpgrade != null
                    ? rocketLauncherUpgrade.GetFireSound()
                    : null;

                // The upgrade Inspector is the primary source. The audio manager
                // is a fallback for scenes/prefabs where the upgrade clip is empty.
                if (rocketClip == null && audio != null)
                    rocketClip = audio.GetFireSound(TurretVariant.RocketLauncher);

                if (rocketClip == null)
                {
                    Debug.LogWarning(
                        "[Turret] Rocket Launcher fired but no Rocket Launcher Fire Sound is assigned " +
                        "on RocketLauncherUpgrade or TurretAudioController.", this);
                    return;
                }

                // Keep the existing shared fire-volume convention, then apply the
                // Rocket Launcher component's own 0..1 volume as PlayOneShot scale.
                if (audio != null)
                    fireAudioSource.volume = audio.FireVolume;
                else
                    fireAudioSource.volume = 1f;

                float rocketVolume = rocketLauncherUpgrade != null
                    ? rocketLauncherUpgrade.FireVolume
                    : 1f;
                fireAudioSource.PlayOneShot(rocketClip, Mathf.Clamp01(rocketVolume));
                return;
            }

            if (audio == null)
                return;

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
            if (currentTarget == null)
                return;

            bool rocketVariant = upgradeController != null &&
                                 upgradeController.CurrentVariant == TurretVariant.RocketLauncher;

            // Rocket Launcher owns its own rocket prefab and eight firing points,
            // so it must not be blocked by the base turret's projectile/firePoint fields.
            if (!rocketVariant && projectilePrefab == null)
            {
                Debug.LogWarning("[Turret] No projectile prefab assigned!", this);
                return;
            }

            if (!rocketVariant && firePoint == null)
            {
                Debug.LogWarning("[Turret] No Fire Point assigned!", this);
                return;
            }

            int currentDamage = GetCurrentDamage();

            if (upgradeController != null && upgradeController.HasActiveUpgrade())
            {
                // This is the single firing path for all upgrade variants.
                // RocketLauncherUpgrade sets its projectile's explosion audio and prefab;
                // Turret plays the launch sound once only after a successful shot.
                if (!upgradeController.TryFireUpgrade(this, currentTarget, currentDamage))
                    return;
            }
            else
            {
                if (projectilePrefab == null || firePoint == null)
                    return;

                FireProjectile(firePoint.position, firePoint.rotation, currentDamage);
            }

            PlayFireSound();
            recoilAmount = 1f;
            ApplyFireEffect();
        }

        private bool FireRocketLauncher(int rocketDamage)
        {
            if (rocketReloading) return false;
            if (rocketFiringPoints == null || rocketFiringPoints.Length == 0)
            {
                Debug.LogWarning("RocketLauncher requires firing points assigned in the Turret Inspector.", this);
                return false;
            }
            if (nextRocketTube >= rocketFiringPoints.Length)
            {
                rocketReloading = true;
                rocketReloadTimer = rocketReloadDuration;
                return false;
            }
            Transform muzzle = rocketFiringPoints[nextRocketTube];
            if (muzzle == null)
            {
                Debug.LogWarning($"RocketLauncher firing point {nextRocketTube} is not assigned.", this);
                return false;
            }
            Vector3 aimPoint = GetTargetAimPoint(currentTarget);
            Vector3 direction = (aimPoint - muzzle.position).normalized;
            GameObject rocketObject = Instantiate(projectilePrefab, muzzle.position,
                Quaternion.LookRotation(direction, Vector3.up));
            if (rocketObject.TryGetComponent(out Projectile rocket))
                rocket.InitializeRocket(direction, rocketDamage, rocketProjectileSpeed,
                    rocketBlastRadius, blueRocketExplosionPrefab);
            else
                Debug.LogError("RocketLauncher projectile prefab needs a Projectile component.", rocketObject);
            SetSpentTubeVisual(nextRocketTube, true);
            nextRocketTube++;
            if (nextRocketTube >= rocketFiringPoints.Length)
            {
                rocketReloading = true;
                rocketReloadTimer = rocketReloadDuration;
            }
            return true;
        }

        private void UpdateRocketReload()
        {
            if (!rocketReloading) return;
            rocketReloadTimer -= Time.deltaTime;
            if (rocketReloadTimer > 0f) return;
            rocketReloading = false;
            nextRocketTube = 0;
            if (rocketSpentHoleVisuals != null)
            {
                for (int i = 0; i < rocketSpentHoleVisuals.Length; i++)
                    SetSpentTubeVisual(i, false);
            }
        }

        /// <summary>
        /// Validate the spent-tube assignments without ever disabling a muzzle.
        /// Each spent visual must be a separate GameObject, not the firing point
        /// itself or a parent GameObject containing a firing point.
        /// </summary>
        private void ValidateRocketTubeVisuals()
        {
            if (rocketSpentHoleVisuals == null)
                return;
            for (int i = 0; i < rocketSpentHoleVisuals.Length; i++)
            {
                GameObject visual = rocketSpentHoleVisuals[i];
                if (visual == null)
                    continue;
                if (IsFiringPointOrAncestor(visual.transform))
                {
                    Debug.LogWarning(
                        $"[RocketLauncher] Spent Tube Visuals element {i} points to a firing point or its parent. " +
                        "It will not be toggled, because that would disable rocket firing. " +
                        "Assign a separate visual GameObject for the dark empty-tube effect.",
                        this);
                    continue;
                }
                visual.SetActive(false);
            }
        }

        private bool IsFiringPointOrAncestor(Transform candidate)
        {
            if (candidate == null || rocketFiringPoints == null)
                return false;
            foreach (Transform muzzle in rocketFiringPoints)
            {
                if (muzzle == null)
                    continue;
                // Exact firing point or a parent of the firing point would
                // disable the muzzle when its GameObject is turned off.
                if (muzzle == candidate || muzzle.IsChildOf(candidate))
                    return true;
            }
            return false;
        }

        private void SetSpentTubeVisual(int index, bool visible)
        {
            if (rocketSpentHoleVisuals == null ||
                index < 0 ||
                index >= rocketSpentHoleVisuals.Length)
                return;
            GameObject visual = rocketSpentHoleVisuals[index];
            if (visual == null)
                return;
            if (IsFiringPointOrAncestor(visual.transform))
                return;
            visual.SetActive(visible);
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
recoilAmount =
                Mathf.MoveTowards(
                    recoilAmount,
                    0f,
                    currentRecoilRecoverySpeed *
                    Time.deltaTime
                );
            // Recoil is always visually DOWN in world space. Applying a
            // parent-local vector directly can become sideways when the turret
            // base or upgraded model has a different authored rotation.
            Transform parent = turretHead.parent;
            Vector3 restWorldPosition = parent != null
                ? parent.TransformPoint(headRestLocalPos)
                : headRestLocalPos;
            Vector3 recoilWorldPosition =
                restWorldPosition +
                Vector3.down * currentRecoilDistance * recoilAmount;
            turretHead.localPosition = parent != null
                ? parent.InverseTransformPoint(recoilWorldPosition)
                : recoilWorldPosition;
        }

        // ============================================================
        // DAMAGE

        // ============================================================
        // Legacy IDamageable compatibility: turret health has been removed.
        public void TakeDamage(int amount) { }

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
