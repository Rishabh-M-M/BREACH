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

        private void Awake()
        {
            Debug.Log($"[Projectile] Awake - Speed: {speed}, Damage: {damage}, Direction: {flyDirection}");
        }

        /// <summary>
        /// Initialize projectile (called by Turret or pool)
        /// </summary>
        public void Initialize(IDamageable target, int dmg)
        {
            this.target = target;
            this.damage = dmg;
            hasHit = false;

            // Calculate initial direction toward target
            UpdateFlyDirection();

            // Safety: auto-destroy after lifetime
            Destroy(gameObject, lifeTime);

            Debug.Log($"[Projectile] Initialized, direction: {flyDirection}, speed: {speed}");
        }

        void Update()
        {
            if (hasHit) return;

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
            else
            {
                Debug.Log("[Projectile] Hit obstacle, destroying");
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;

            if (other.TryGetComponent(out IDamageable damageable))
            {
                HitTarget(damageable);
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

            if (!damageable.IsDead)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"[Projectile] Hit target for {damage} damage");
            }
            else
            {
                Debug.Log("[Projectile] Hit corpse, no damage");
            }

            hasHit = true;

            // Spawn the optional explosion burst before the projectile is destroyed. Null-checked so
            // the field can stay unassigned without changing any behaviour.
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
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
    }
}
