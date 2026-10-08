using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Sharpshooter (SS):
    /// - One very fast straight projectile
    /// - Very low fire rate
    /// - Prefers the furthest enemy inside its range
    /// - Projectile damage increases with distance travelled
    /// - Lower operating power than the Base turret
    /// - Strong downward + backward recoil
    /// </summary>
    public class SharpshooterUpgrade : TurretUpgrade
    {
        [Header("Sharpshooter Stats")]
        [Tooltip("Base damage before distance scaling is added.")]
        [SerializeField] private int damage = 10;

        [Tooltip("Seconds between Sharpshooter shots. Higher = slower firing.")]
        [SerializeField] private float fireRate = 3.5f;

        [Tooltip("Projectile speed. Base Projectile speed is 12 by default, so this is intentionally much faster.")]
        [SerializeField] private float projectileSpeed = 45f;

        [Header("Sharpshooter Range")]
        [Tooltip("Attack range of the Sharpshooter. Base turret range is 8 by default.")]
        [SerializeField] private float attackRange = 18f;

        [Tooltip("Additional damage gained for every metre the projectile travels.")]
        [SerializeField] private float damagePerMetre = 2f;

        [Tooltip("Maximum damage a single Sharpshooter projectile can deal.")]
        [SerializeField] private int maxDamage = 80;

        [Header("Sharpshooter Power")]
        [SerializeField] private int permanentPowerCost = 20;
        [SerializeField] private int operatingPowerCost = 20;

        [Header("Sharpshooter Audio")]
        [Tooltip("Sound played when the Sharpshooter fires.")]
        [SerializeField] private AudioClip fireSound;

        [Tooltip("Looping mechanical sound while the Sharpshooter head is rotating.")]
        [SerializeField] private AudioClip rotationSound;

        [Header("Sharpshooter Visual")]
        [Tooltip("The complete Sharpshooter model/visual. Do not put another Turret component on it.")]
        [SerializeField] private GameObject visual;

        [Header("Sharpshooter Transforms")]
        [Tooltip("The rotating Sharpshooter head.")]
        [SerializeField] private Transform turretHead;

        [Tooltip("The physical Sharpshooter muzzle / projectile spawn point.")]
        [SerializeField] private Transform firePoint;

        [Header("Sharpshooter Recoil")]
        [Tooltip("Total recoil distance. SS is intentionally stronger than Base.")]
        [SerializeField] private float recoilDistance = 0.28f;

        [Tooltip("How quickly SS returns from recoil.")]
        [SerializeField] private float recoilRecoverySpeed = 8f;

        [Tooltip("Recoil direction in the turret head parent space. SS moves downward and backward.")]
        [SerializeField] private Vector3 recoilLocalDirection =
            new Vector3(0f, -1f, -1f);

        public int PermanentPowerCost => permanentPowerCost;
        public int OperatingPowerCost => operatingPowerCost;

        public override AudioClip GetFireSound()
        {
            return fireSound;
        }

        public override AudioClip GetRotationSound()
        {
            return rotationSound;
        }

        private void Awake()
        {
            if (turretHead == null)
                turretHead = FindChildTransform(transform, "SS head");

            if (turretHead == null)
                turretHead = FindChildTransform(transform, "Sharpshooter head");

            if (firePoint == null)
                firePoint = FindChildTransform(transform, "SS spawner");

            if (firePoint == null)
                firePoint = FindChildTransform(transform, "SS fire point");
        }

        public override int GetDamage(int baseDamage)
        {
            return damage;
        }

        public override float GetFireRate(float baseFireRate)
        {
            return fireRate;
        }

        public override Transform GetTurretHead(Transform baseHead)
        {
            return turretHead != null ? turretHead : baseHead;
        }

        public override Transform GetFirePoint(Transform baseFirePoint)
        {
            return firePoint != null
                ? firePoint
                : (turretHead != null ? turretHead : baseFirePoint);
        }

        public override float GetAttackRange(float baseRange)
        {
            return attackRange;
        }

        public override bool PrefersFurthestTarget()
        {
            return true;
        }

        public override float GetRecoilDistance(float baseDistance)
        {
            return recoilDistance;
        }

        public override float GetRecoilRecoverySpeed(float baseRecoverySpeed)
        {
            return recoilRecoverySpeed;
        }

        public override Vector3 GetRecoilLocalDirection(Vector3 baseDirection)
        {
            return recoilLocalDirection;
        }

        public override bool TryFire(
            Turret turret,
            IDamageable target,
            int projectileDamage
        )
        {
            if (turret == null || target == null)
                return false;

            Transform muzzle = GetFirePoint(null);

            if (muzzle == null)
            {
                Debug.LogWarning(
                    $"[SharpshooterUpgrade] {name}: No Sharpshooter fire point assigned."
                );
                return false;
            }

            Vector3 targetAimPoint = turret.GetTargetAimPoint(target);
            Vector3 firingDirection = targetAimPoint - muzzle.position;

            if (firingDirection.sqrMagnitude < 0.0001f)
            {
                firingDirection = muzzle.forward;
            }
            else
            {
                firingDirection.Normalize();
            }

            // Exactly ONE projectile per firing cycle.
            turret.FireSharpshooterProjectile(
                muzzle.position,
                firingDirection,
                damage,
                projectileSpeed,
                damagePerMetre,
                maxDamage
            );

            return true;
        }

        public override void SetVisualActive(bool active)
        {
            if (visual != null)
                visual.SetActive(active);
        }

        public override void DrawGizmos(Turret turret)
        {
            if (turretHead == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(turretHead.position, 0.05f);

            if (turret.CurrentTarget != null)
            {
                Gizmos.DrawLine(
                    turretHead.position,
                    turret.GetTargetAimPoint(turret.CurrentTarget)
                );
            }
        }

        private Transform FindChildTransform(Transform root, string childName)
        {
            if (root == null)
                return null;

            Transform[] children =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child != root && child.name == childName)
                    return child;
            }

            return null;
        }
    }
}
