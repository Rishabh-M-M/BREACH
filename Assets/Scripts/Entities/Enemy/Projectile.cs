using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Projectile: Homing projectile that tracks a target and damages on hit.
    /// If target dies, continues flying straight in last known direction until collision.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private int damage = 5;
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private float hitDistance = 0.5f;
        [Tooltip("Fallback aim height above the target pivot, used only when the target has no collider.")]
        [SerializeField] private float aimHeight = 0.6f;

        [Header("Hit Feedback")]
        [Tooltip("Optional explosion particle prefab spawned at the impact point. The prefab owns its " +
                 "own lifetime (Particle System Stop Action = Destroy), so no cleanup is needed here.")]
        [SerializeField] private GameObject explosionEffectPrefab;

        private IDamageable target;
        private bool hasHit = false;
        private Vector3 flyDirection;

        // Sharpshooter projectile state.
        private bool isSharpshooterProjectile;
        private float sharpshooterSpeed;
        private float sharpshooterDamagePerMetre;
        private int sharpshooterMaxDamage;
        private Vector3 projectileStartPosition;

        // Rocket projectile state.
        private bool isRocketProjectile;
        private int rocketExplosionDamage;
        private float rocketExplosionRadius;
        private float rocketSpeed;
        [Header("Rocket Visual Orientation")]
        [Tooltip("Corrects the rocket model's authored forward axis. The default Y=180 fixes models whose nose points backward.")]
        [SerializeField] private Vector3 rocketModelRotationOffset = new Vector3(0f, 180f, 0f);
        [Header("Rocket Explosion Visual") ]
        [Tooltip("How long the explosion particles expand. The visual expansion reaches the configured damage radius at approximately this time.")]
        [SerializeField, Min(0.1f)] private float rocketExplosionEffectDuration = 1.5f;
        [Header("Rocket Audio")]
        [SerializeField] private AudioClip rocketExplosionSound;
        [SerializeField, Range(0f, 1f)] private float rocketExplosionVolume = 1f;

        // Verbose logging is a WebGL performance tax (console I/O on every spawn/hit).
        private static bool verboseLogging = false;

        private GameObject explosionEffectOverride;

        private void Awake()
        {
            if (verboseLogging) Debug.Log($"[Projectile] Awake - Speed: {speed}, Damage: {damage}, Direction: {flyDirection}");
        }

        /// <summary>
        /// Initialize projectile (called by Turret or pool)
        /// </summary>
        public void Initialize(IDamageable target, int dmg)
        {
            this.target = target;
            this.damage = dmg;
            hasHit = false;

            isSharpshooterProjectile = false;
            sharpshooterSpeed = 0f;
            sharpshooterDamagePerMetre = 0f;
            sharpshooterMaxDamage = 0;
            projectileStartPosition = transform.position;

            // Normal projectiles use the existing homing behaviour.
            UpdateFlyDirection();

            // Safety: auto-destroy after lifetime
            Destroy(gameObject, lifeTime);

            if (verboseLogging) Debug.Log($"[Projectile] Initialized, direction: {flyDirection}, speed: {speed}");
        }

        /// <summary>
        /// Initialize a projectile that travels in one fixed straight line.
        ///
        /// This is used by the Double Barrel turret. The turret calculates
        /// the firing direction from the midpoint between its two physical
        /// muzzle spawners, then both projectiles use that exact direction.
        /// The projectile therefore does NOT home toward the target.
        /// </summary>
        public void InitializeStraight(Vector3 direction, int dmg)
        {
            target = null;
            damage = dmg;
            hasHit = false;

            isSharpshooterProjectile = false;
            sharpshooterSpeed = 0f;
            sharpshooterDamagePerMetre = 0f;
            sharpshooterMaxDamage = 0;
            projectileStartPosition = transform.position;

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = transform.forward;
            }
            else
            {
                direction.Normalize();
            }

            flyDirection = direction;
            transform.forward = flyDirection;

            // Safety: auto-destroy after lifetime
            Destroy(gameObject, lifeTime);

            if (verboseLogging)
            {
                Debug.Log(
                    $"[Projectile] Straight initialized, " +
                    $"direction: {flyDirection}, speed: {speed}"
                );
            }
        }

        /// <summary>
        /// Optionally override which explosion effect this specific projectile
        /// spawns on hit, instead of the prefab's default explosionEffectPrefab.
        /// </summary>
        public void SetExplosionEffectOverride(GameObject effect)
        {
            explosionEffectOverride = effect;
        }

        public void SetRocketExplosionSettings(AudioClip explosionSound, float visualDuration, float soundVolume = 1f)
        {
            rocketExplosionSound = explosionSound;
            rocketExplosionEffectDuration = Mathf.Max(0.1f, visualDuration);
            rocketExplosionVolume = Mathf.Clamp01(soundVolume);
        }

        /// <summary>
        /// Initialize the Sharpshooter projectile.
        /// It travels in one fixed direction at a much higher speed than
        /// the normal projectile. Its damage is calculated from the
        /// distance it actually travelled before impact.
        /// </summary>
        public void InitializeSharpshooter(
            Vector3 direction,
            int baseDamage,
            float projectileSpeed,
            float damagePerMetre,
            int maxDamage
        )
        {
            target = null;
            damage = baseDamage;
            hasHit = false;

            isSharpshooterProjectile = true;
            sharpshooterSpeed = Mathf.Max(0f, projectileSpeed);
            sharpshooterDamagePerMetre = Mathf.Max(0f, damagePerMetre);
            sharpshooterMaxDamage = Mathf.Max(baseDamage, maxDamage);
            projectileStartPosition = transform.position;

            if (direction.sqrMagnitude < 0.0001f)
                direction = transform.forward;
            else
                direction.Normalize();

            flyDirection = direction;
            transform.forward = flyDirection;

            Destroy(gameObject, lifeTime);

            if (verboseLogging)
            {
                Debug.Log(
                    $"[Projectile] Sharpshooter initialized. " +
                    $"Speed = {sharpshooterSpeed}, " +
                    $"Base Damage = {baseDamage}, " +
                    $"Damage/Metre = {sharpshooterDamagePerMetre}"
                );
            }
        }

        /// <summary>Initializes a rocket that travels toward a world-space target position.</summary>
        public void LaunchRocket(
            Vector3 targetPosition,
            int directDamage,
            int explosionDamage,
            float explosionRadius,
            float projectileSpeed)
        {
            target = null;
            damage = Mathf.Max(0, directDamage);
            hasHit = false;
            isSharpshooterProjectile = false;
            isRocketProjectile = true;
            rocketExplosionDamage = Mathf.Max(0, explosionDamage);
            rocketExplosionRadius = Mathf.Max(0.1f, explosionRadius);
            rocketSpeed = Mathf.Max(0.1f, projectileSpeed);

            Vector3 direction = targetPosition - transform.position;
            flyDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : transform.forward;
            ApplyRocketModelRotation();
            Destroy(gameObject, lifeTime);
        }

        /// <summary>Compatibility overload used by older Turret.cs calls.</summary>
        public void InitializeRocket(
            Vector3 direction,
            int directDamage,
            float projectileSpeed,
            float explosionRadius,
            GameObject explosionPrefab)
        {
            target = null;
            damage = Mathf.Max(0, directDamage);
            hasHit = false;
            isSharpshooterProjectile = false;
            isRocketProjectile = true;
            rocketExplosionDamage = damage;
            rocketExplosionRadius = Mathf.Max(0.1f, explosionRadius);
            rocketSpeed = Mathf.Max(0.1f, projectileSpeed);
            explosionEffectOverride = explosionPrefab;
            flyDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : transform.forward;
            ApplyRocketModelRotation();
            Destroy(gameObject, lifeTime);
        }

        /// <summary>Compatibility overload for callers using target + separate splash damage.</summary>
        public void InitializeRocket(
            Vector3 targetPosition,
            int directDamage,
            int explosionDamage,
            float explosionRadius,
            float projectileSpeed)
        {
            LaunchRocket(targetPosition, directDamage, explosionDamage, explosionRadius, projectileSpeed);
        }

        void Update()
        {
            if (hasHit) return;

            if (isRocketProjectile)
            {
                ContinueFlyingRocket();
                return;
            }

            if (isSharpshooterProjectile)
            {
                ContinueFlyingSharpshooter();
                return;
            }

            // Check if target is valid and alive
            bool targetValid = target != null && !target.IsDead;

            if (targetValid)
            {
                // Track and home in on living target
                TrackTarget();
            }
            else
            {
                // Target lost/dead - continue flying straight
                ContinueFlyingStraight();
            }
        }

        private void ContinueFlyingRocket()
        {
            if (flyDirection.sqrMagnitude < 0.0001f)
                flyDirection = Vector3.forward;

            if (TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
                rb.linearVelocity = flyDirection * rocketSpeed;
            else
                transform.position += flyDirection * rocketSpeed * Time.deltaTime;

            // Do not use transform.forward here: it would overwrite the model's
            // 180-degree correction every frame. Movement uses flyDirection only.
            ApplyRocketModelRotation();
        }

        private void ApplyRocketModelRotation()
        {
            if (flyDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion trajectoryRotation =
                Quaternion.LookRotation(flyDirection.normalized, Vector3.up);
            transform.rotation = trajectoryRotation *
                Quaternion.Euler(rocketModelRotationOffset);
        }

        private void ContinueFlyingSharpshooter()
        {
            if (flyDirection == Vector3.zero)
                flyDirection = transform.forward;

            if (TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
            {
                rb.linearVelocity =
                    flyDirection * sharpshooterSpeed;
            }
            else
            {
                transform.position +=
                    flyDirection *
                    sharpshooterSpeed *
                    Time.deltaTime;
            }

            transform.forward = flyDirection;
        }

        private int GetSharpshooterDamage()
        {
            float distanceTravelled =
                Vector3.Distance(
                    projectileStartPosition,
                    transform.position
                );

            float scaledDamage =
                damage +
                distanceTravelled *
                sharpshooterDamagePerMetre;

            return Mathf.Clamp(
                Mathf.RoundToInt(scaledDamage),
                damage,
                sharpshooterMaxDamage
            );
        }

        /// <summary>
        /// Update fly direction based on current target position
        /// </summary>
        private void UpdateFlyDirection()
        {
            if (target != null)
            {
                MonoBehaviour targetMb = target as MonoBehaviour;
                if (targetMb != null)
                {
                    flyDirection = (GetAimPoint(targetMb) - transform.position).normalized;
                    transform.forward = flyDirection;
                    return;
                }
            }

            // Fallback: use current forward if no target
            if (flyDirection == Vector3.zero)
            {
                flyDirection = transform.forward;
            }
        }

        /// <summary>
        /// Aim point on the target. Enemy pivots sit at the feet (ground-level root), so
        /// aiming at transform.position makes every shot dive into the floor at the
        /// target's base. Prefer the collider's bounds center (torso); fall back to a
        /// fixed chest-height offset when the target has no collider.
        /// </summary>
        private Vector3 GetAimPoint(MonoBehaviour targetMb)
        {
            if (targetMb.TryGetComponent(out Collider col))
            {
                return col.bounds.center;
            }
            return targetMb.transform.position + Vector3.up * aimHeight;
        }

        /// <summary>
        /// Continue flying straight when target is lost/dead
        /// </summary>
        private void ContinueFlyingStraight()
        {
            // Safety check: if direction is zero, use forward
            if (flyDirection == Vector3.zero)
            {
                flyDirection = transform.forward;
                Debug.LogWarning("[Projectile] flyDirection was zero, using transform.forward");
            }

            // Only a NON-kinematic Rigidbody responds to velocity writes. A kinematic body
            // ignores linearVelocity entirely (it can only be moved via the transform),
            // which used to leave orphaned projectiles frozen mid-air until lifetime expiry.
            if (TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
            {
                rb.linearVelocity = flyDirection * speed;
            }
            else
            {
                transform.position += flyDirection * speed * Time.deltaTime;
            }
        }

        /// <summary>
        /// Track and home in on living target
        /// </summary>
        private void TrackTarget()
        {
            MonoBehaviour targetMb = target as MonoBehaviour;
            if (targetMb == null)
            {
                ContinueFlyingStraight();
                return;
            }

            // Update direction toward the target's body center, not its feet
            Vector3 targetPosition = GetAimPoint(targetMb);
            flyDirection = (targetPosition - transform.position).normalized;

            // Move toward target
            transform.position += flyDirection * speed * Time.deltaTime;

            // Rotate to face movement direction
            transform.forward = flyDirection;

            // Hit check
            if (Vector3.Distance(transform.position, targetPosition) < hitDistance)
            {
                HitTarget(target);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasHit) return;

            if (collision.gameObject.TryGetComponent(out IDamageable damageable))
            {
                HitTarget(damageable);
            }
            else if (isRocketProjectile)
            {
                ExplodeRocket();
            }
            else
            {
                Debug.Log("[Projectile] Hit obstacle, destroying");
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit || other == null) return;

            // Stasis Field tiles use trigger BoxColliders to detect enemies.
            // They are area-effect volumes, not projectile obstacles.
            if (other.GetComponentInParent<StasisFieldTile>() != null)
                return;

            // Enemy hitboxes can be child colliders, so resolve damageable
            // components from the collider or any parent object.
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                HitTarget(damageable);
            }
            else if (isRocketProjectile)
            {
                ExplodeRocket();
            }
            else
            {
                Debug.Log("[Projectile] Hit trigger obstacle, destroying");
                Destroy(gameObject);
            }
        }

        private void HitTarget(IDamageable damageable)
        {
            if (hasHit) return;

            if (isRocketProjectile)
            {
                hasHit = true;
                if (damageable != null && !damageable.IsDead && damage > 0)
                    damageable.TakeDamage(damage);
                ExplodeRocketDamageAndEffects();
                return;
            }

            if (damageable != null && !damageable.IsDead)
            {
                int damageToDeal =
                    isSharpshooterProjectile
                        ? GetSharpshooterDamage()
                        : damage;

                damageable.TakeDamage(damageToDeal);

                if (verboseLogging)
                {
                    Debug.Log(
                        $"[Projectile] Hit target for {damageToDeal} damage"
                    );
                }
            }
            else
            {
                Debug.Log("[Projectile] Hit corpse, no damage");
            }

            hasHit = true;

            // Spawn the optional explosion burst before the projectile is destroyed. Null-checked so
            // the field can stay unassigned without changing any behaviour.
            GameObject effectToSpawn = explosionEffectOverride != null ? explosionEffectOverride : explosionEffectPrefab;

            if (effectToSpawn != null)
            {
                Instantiate(effectToSpawn, transform.position, Quaternion.identity);
            }

            // W7 Plasma shader hook: flash white on hit (via MaterialPropertyBlock), then let the
            // visual component destroy the object after a short delay so the flash stays visible.
            // If no PlasmaProjectileVisual is attached, keep the original behaviour: destroy at once.
            if (TryGetComponent(out PlasmaProjectileVisual plasmaVisual))
            {
                plasmaVisual.OnHit();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void ExplodeRocket()
        {
            if (hasHit) return;
            hasHit = true;
            ExplodeRocketDamageAndEffects();
        }

        private void ExplodeRocketDamageAndEffects()
        {
            Vector3 explosionPosition = transform.position;

            if (rocketExplosionSound != null && rocketExplosionVolume > 0f)
                AudioSource.PlayClipAtPoint(rocketExplosionSound, explosionPosition, rocketExplosionVolume);

            GameObject effectToSpawn = explosionEffectOverride != null
                ? explosionEffectOverride
                : explosionEffectPrefab;
            if (effectToSpawn != null)
            {
                GameObject effectInstance = Instantiate(
                    effectToSpawn, explosionPosition, Quaternion.identity);

                RocketExplosionEffect effectController =
                    effectInstance.GetComponent<RocketExplosionEffect>();

                if (effectController == null)
                    effectController = effectInstance.AddComponent<RocketExplosionEffect>();

                effectController.Configure(
                    rocketExplosionRadius, rocketExplosionEffectDuration);
            }

            Collider[] colliders = Physics.OverlapSphere(explosionPosition, rocketExplosionRadius);
            System.Collections.Generic.HashSet<IDamageable> damaged =
                new System.Collections.Generic.HashSet<IDamageable>();

            foreach (Collider hit in colliders)
            {
                if (hit == null) continue;
                IDamageable damageable = null;
                if (hit.TryGetComponent(out IDamageable direct))
                    damageable = direct;
                else if (hit.transform.root.TryGetComponent(out IDamageable rootDamageable))
                    damageable = rootDamageable;

                if (damageable == null || damageable.IsDead || !damaged.Add(damageable))
                    continue;
                if (rocketExplosionDamage <= 0)
                    continue;

                // Linear falloff, rounded to integer damage. Damage is strongest at
                // the centre and falls toward the radius edge; inside targets take >= 1.
                Component damageableComponent = damageable as Component;
                Vector3 damagePosition = damageableComponent != null
                    ? damageableComponent.transform.position
                    : hit.bounds.center;
                float distanceRatio = Mathf.Clamp01(
                    Vector3.Distance(explosionPosition, damagePosition) / rocketExplosionRadius);
                int scaledDamage = Mathf.RoundToInt(rocketExplosionDamage * (1f - distanceRatio));
                if (scaledDamage <= 0) scaledDamage = 1;
                damageable.TakeDamage(scaledDamage);
            }
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (!isRocketProjectile || rocketExplosionRadius <= 0f)
                return;

            Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, rocketExplosionRadius);
        }

    }
}
