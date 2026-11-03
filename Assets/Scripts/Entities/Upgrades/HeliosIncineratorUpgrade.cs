using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Continuous-beam turret upgrade. Selects the living enemy with the
    /// highest current HP, locks it for one beam cycle, ramps DPS, overheats,
    /// cools down, then reacquires a fresh highest-HP target.
    /// </summary>
    public class HeliosIncineratorUpgrade : TurretUpgrade
    {
        [Header("Helios Incinerator Stats")]
        [SerializeField, Min(0f)] private float attackRange = 12f;
        [SerializeField, Min(0f)] private float startingDps = 5f;
        [SerializeField, Min(0f)] private float maximumDps = 40f;
        [Tooltip("Time taken for the beam to ramp from starting DPS to maximum DPS.")]
        [SerializeField, Min(0.1f)] private float rampDuration = 5f;
        [SerializeField, Min(0f)] private float cooldownDuration = 3f;
        [SerializeField] private int permanentPowerCost = 35;
        [SerializeField] private int operatingPowerCost = 45;

        [Header("Helios Incinerator Visual")]
        [Tooltip("Visual model shown when this upgrade is installed.")]
        [SerializeField] private GameObject visual;
        [Tooltip("LineRenderer used to draw the continuous beam. Assign one on the laser model.")]
        [SerializeField] private LineRenderer beamRenderer;
        [Tooltip("Optional beam origin. Uses the active turret fire point if empty.")]
        [SerializeField] private Transform beamStart;

        [Header("Laser Audio")]
        [Tooltip("Optional AudioSource. If empty, one is created automatically. The clip is taken from TurretAudioController.")]
        [SerializeField] private AudioSource laserAudioSource;

        [SerializeField, Min(0.001f)] private float beamWidth = 0.08f;
        [SerializeField, Min(0.001f)] private float maximumBeamWidth = 0.24f;

        [Header("Beam Charge Colours")]
        [SerializeField] private Color startBeamColor = new Color(0.25f, 0.85f, 1f, 1f);
        [SerializeField] private Color chargedBeamColor = new Color(1f, 0.2f, 0.05f, 1f);
        [SerializeField] private Color startEndColor = new Color(0.2f, 0.65f, 1f, 1f);
        [SerializeField] private Color chargedEndColor = new Color(1f, 0.95f, 0.25f, 1f);

        [Header("Helios Head Rotation")]
        [Tooltip("Assign the actual rotating Helios head transform. If empty, the script searches likely child names.")]
        [SerializeField] private Transform turretHead;
        [SerializeField] private bool autoFindTurretHead = true;

        [Header("Targeting")]
        [SerializeField] private LayerMask targetMask = ~0;

        private IDamageable lockedTarget;
        private float rampTimer;
        private float cooldownTimer;
        private float fractionalDamage;
        private bool coolingDown;

        public override int PermanentPowerCost => permanentPowerCost;
        public override int OperatingPowerCost => operatingPowerCost;
        public IDamageable LockedTarget => lockedTarget;
        public bool IsCoolingDown => coolingDown;
        public float CurrentDps => coolingDown ? 0f : Mathf.Lerp(startingDps, maximumDps, Mathf.Clamp01(rampTimer / rampDuration));

        public override string UpgradeInfo =>
            $"incinerating beam thats ramps-up damage by locking onto the highest HP enemy";
        private void Awake()
        {
            ResolveTurretHead();
            EnsureLaserAudioSource();

            if (beamRenderer != null)
            {
                beamRenderer.enabled = false;
                beamRenderer.startWidth = beamWidth;
                beamRenderer.endWidth = beamWidth;
                beamRenderer.startColor = startBeamColor;
                beamRenderer.endColor = startEndColor;
                beamRenderer.useWorldSpace = true;
                beamRenderer.positionCount = 2;
            }
        }

        private void OnValidate()
        {
            maximumBeamWidth = Mathf.Max(beamWidth, maximumBeamWidth);
        }

        private void EnsureLaserAudioSource()
        {
            if (laserAudioSource == null)
                laserAudioSource = GetComponent<AudioSource>();

            if (laserAudioSource == null)
                laserAudioSource = gameObject.AddComponent<AudioSource>();

            laserAudioSource.playOnAwake = false;
            laserAudioSource.loop = true;
            laserAudioSource.spatialBlend = 0f;
            laserAudioSource.Stop();
        }

        private void UpdateLaserAudio(Turret turret, bool shouldPlay)
        {
            if (laserAudioSource == null)
                EnsureLaserAudioSource();

            if (laserAudioSource == null)
                return;

            AudioClip sharedLaserClip = TurretAudioController.Instance != null
                ? TurretAudioController.Instance.GetFireSound(TurretVariant.HeliosIncinerator)
                : null;

            if (laserAudioSource.clip != sharedLaserClip)
                laserAudioSource.clip = sharedLaserClip;

            bool roomAudible = TurretAudioController.Instance == null ||
                               TurretAudioController.Instance.IsRoomAudioAudible(turret);

            if (TurretAudioController.Instance != null)
                laserAudioSource.volume = TurretAudioController.Instance.FireVolume;
            else
                laserAudioSource.volume = 1f;

            laserAudioSource.mute = !roomAudible;

            if (shouldPlay && sharedLaserClip != null)
            {
                if (!laserAudioSource.isPlaying)
                    laserAudioSource.Play();
            }
            else if (laserAudioSource.isPlaying)
            {
                laserAudioSource.Stop();
            }
        }

        private void ResolveTurretHead()
        {
            if (turretHead != null || !autoFindTurretHead)
                return;

            string[] likelyNames =
            {
                "HI head", "Helios Incinerator head", "Helios head",
                "Laser head", "HI Head", "HeliosHead"
            };

            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (string likelyName in likelyNames)
            {
                foreach (Transform child in children)
                {
                    if (child != transform &&
                        string.Equals(child.name, likelyName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        turretHead = child;
                        return;
                    }
                }
            }

            foreach (Transform child in children)
            {
                if (child == transform) continue;
                string lowerName = child.name.ToLowerInvariant();
                if ((lowerName.Contains("helios") || lowerName.Contains("laser") || lowerName.StartsWith("hi "))
                    && lowerName.Contains("head"))
                {
                    turretHead = child;
                    return;
                }
            }
        }

        public override int GetDamage(int baseDamage) => baseDamage;
        public override float GetFireRate(float baseFireRate) => baseFireRate;
        public override bool TryFire(Turret turret, IDamageable target, int projectileDamage) => false;
        public override float GetAttackRange(float baseRange) => attackRange;
        public override Transform GetTurretHead(Transform baseHead)
        {
            ResolveTurretHead();
            return turretHead != null ? turretHead : baseHead;
        }
        public override Transform GetFirePoint(Transform baseFirePoint) => baseFirePoint;

        /// <summary>Called by Turret.Update while Helios Incinerator is active.</summary>
        public void Tick(Turret turret, float deltaTime)
        {
            if (turret == null || !turret.IsActive || turret.IsDead)
            {
                StopBeam();
                UpdateLaserAudio(turret, false);
                return;
            }

            if (coolingDown)
            {
                cooldownTimer -= deltaTime;
                StopBeam();
                UpdateLaserAudio(turret, false);
                if (cooldownTimer <= 0f)
                {
                    coolingDown = false;
                    cooldownTimer = 0f;
                    rampTimer = 0f;
                    fractionalDamage = 0f;
                    lockedTarget = null;
                }
                return;
            }

            // A target is selected only at the start of a firing cycle.
            // If that target dies, stop firing and rest; do not immediately
            // switch to another enemy during the same cycle.
            if (lockedTarget != null && lockedTarget.IsDead)
            {
                BeginCooldown();
                return;
            }

            if (lockedTarget == null)
            {
                lockedTarget = FindHighestHpTarget(turret.transform.position, attackRange);
                rampTimer = 0f;
                fractionalDamage = 0f;
            }

            if (lockedTarget == null)
            {
                StopBeam();
                UpdateLaserAudio(turret, false);
                return;
            }

            rampTimer += deltaTime;
            float normalizedRamp = Mathf.Clamp01(rampTimer / rampDuration);
            float dps = Mathf.Lerp(startingDps, maximumDps, normalizedRamp);
            fractionalDamage += dps * deltaTime;

            int wholeDamage = Mathf.FloorToInt(fractionalDamage);
            if (wholeDamage > 0)
            {
                fractionalDamage -= wholeDamage;
                if (!lockedTarget.IsDead)
                    lockedTarget.TakeDamage(wholeDamage);
            }

            UpdateBeam(turret, lockedTarget);
            UpdateLaserAudio(turret, true);

            // Reaching max DPS causes an overheat cycle. Clear the target
            // during cooldown so the next cycle performs a fresh HP ranking.
            if (rampTimer >= rampDuration)
            {
                // If the target survives until maximum DPS, overheat and
                // enter cooldown. The next cycle will rank all enemies again.
                BeginCooldown();
            }
        }

        private void BeginCooldown()
        {
            coolingDown = true;
            UpdateLaserAudio(null, false);
            cooldownTimer = cooldownDuration;
            lockedTarget = null;
            fractionalDamage = 0f;
            StopBeam();
        }

        private IDamageable FindHighestHpTarget(Vector3 origin, float range)
        {
            Collider[] hits = Physics.OverlapSphere(origin, range, targetMask);
            IDamageable best = null;
            int highestHp = int.MinValue;
            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit == null) continue;
                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.IsDead) continue;

                float distance = Vector3.Distance(origin, enemy.transform.position);
                if (distance > range) continue;

                // Highest current HP wins; closest enemy breaks HP ties.
                if (enemy.CurrentHP > highestHp ||
                    (enemy.CurrentHP == highestHp && distance < bestDistance))
                {
                    best = enemy;
                    highestHp = enemy.CurrentHP;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void UpdateBeam(Turret turret, IDamageable target)
        {
            if (beamRenderer == null || target == null) return;
            MonoBehaviour targetBehaviour = target as MonoBehaviour;
            if (targetBehaviour == null) { StopBeam(); return; }

            Transform startTransform = beamStart != null ? beamStart : turret.ActiveFirePoint;
            if (startTransform == null) startTransform = turret.transform;

            // Widen and shift colour as the DPS ramp charges.
            float charge = Mathf.Clamp01(rampTimer / rampDuration);
            float width = Mathf.Lerp(beamWidth, maximumBeamWidth, charge);
            beamRenderer.startWidth = width;
            beamRenderer.endWidth = Mathf.Lerp(width, maximumBeamWidth * 1.35f, charge);
            beamRenderer.startColor = Color.Lerp(startBeamColor, chargedBeamColor, charge);
            beamRenderer.endColor = Color.Lerp(startEndColor, chargedEndColor, charge);
            beamRenderer.positionCount = 2;
            beamRenderer.SetPosition(0, startTransform.position);
            beamRenderer.SetPosition(1, turret.GetTargetAimPoint(target));
            beamRenderer.enabled = true;
        }

        public void StopForTurretOff()
        {
            StopBeam();
            UpdateLaserAudio(null, false);
            lockedTarget = null;
            rampTimer = 0f;
            cooldownTimer = 0f;
            fractionalDamage = 0f;
            coolingDown = false;
        }

        private void StopBeam()
        {
            if (beamRenderer == null) return;

            beamRenderer.enabled = false;
            beamRenderer.startWidth = beamWidth;
            beamRenderer.endWidth = beamWidth;
            beamRenderer.startColor = startBeamColor;
            beamRenderer.endColor = startEndColor;
        }

        public override void SetVisualActive(bool active)
        {
            if (visual != null) visual.SetActive(active);
            if (!active) StopBeam();
        }

        public override void DrawGizmos(Turret turret)
        {
            if (turret == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(turret.transform.position, attackRange);
        }

        private void OnDisable()
        {
            StopBeam();
            UpdateLaserAudio(null, false);
            lockedTarget = null;
            coolingDown = false;
            rampTimer = 0f;
            cooldownTimer = 0f;
            fractionalDamage = 0f;
        }
    }
}
