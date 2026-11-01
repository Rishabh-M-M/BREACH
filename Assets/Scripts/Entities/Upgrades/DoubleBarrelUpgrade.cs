using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Contains only Double Barrel-specific configuration and behaviour.
    /// Changes to the base Turret stats do not overwrite these values.
    /// </summary>
    public class DoubleBarrelUpgrade : TurretUpgrade
    {
        [Header("Double Barrel Stats")]
        [Tooltip("Damage dealt by EACH barrel.")]
        [SerializeField] private int damage = 4;

        [Tooltip("Seconds between Double Barrel firing cycles.")]
        [SerializeField] private float fireRate = 1.1f;

        [Header("Double Barrel Power")]
        [SerializeField] private int permanentPowerCost = 30;
        [SerializeField] private int operatingPowerCost = 50;


        [Header("Double Barrel Visual")]
        [Tooltip("The Double Barrel model/visual only. Do not put another Turret component on it.")]
        [SerializeField] private GameObject visual;

        [Header("Double Barrel Transforms")]
        [Tooltip("The rotating DB head. If empty, the component searches for 'DB head'.")]
        [SerializeField] private Transform turretHead;

        [Tooltip("Left physical projectile spawner.")]
        [SerializeField] private Transform leftSpawner;

        [Tooltip("Right physical projectile spawner.")]
        [SerializeField] private Transform rightSpawner;

        public override int PermanentPowerCost => permanentPowerCost;
        public override int OperatingPowerCost => operatingPowerCost;
        public override string UpgradeInfo =>
            $"Fires two projectiles per shot. " +
            $"Each barrel deals {damage} damage. " +
            $"Firing interval: {fireRate:0.##} seconds.";

        private void Awake()
        {
            if (turretHead == null)
            {
                turretHead = FindChildTransform(transform, "DB head");
            }
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
            // The actual DB shots use the two physical spawners.
            // This fallback keeps Turret rotation/recoil safe.
            return turretHead != null ? turretHead : baseFirePoint;
        }

        public override bool TryFire(
            Turret turret,
            IDamageable target,
            int projectileDamage
        )
        {
            if (leftSpawner == null || rightSpawner == null)
            {
                Debug.LogWarning(
                    $"[DoubleBarrelUpgrade] {name}: Left and Right Spawner must be assigned."
                );
                return false;
            }

            Vector3 leftMuzzle = leftSpawner.position;
            Vector3 rightMuzzle = rightSpawner.position;
            Vector3 muzzleCenter = (leftMuzzle + rightMuzzle) * 0.5f;

            Vector3 targetAimPoint = turret.GetTargetAimPoint(target);
            Vector3 firingDirection = targetAimPoint - muzzleCenter;

            if (firingDirection.sqrMagnitude < 0.0001f)
            {
                firingDirection = turretHead != null
                    ? turretHead.forward
                    : turret.transform.forward;
            }
            else
            {
                firingDirection.Normalize();
            }

            // Both shots use the exact same direction, preserving their
            // physical left/right separation instead of homing together.
            turret.FireStraightProjectile(
                leftMuzzle,
                firingDirection,
                projectileDamage
            );

            turret.FireStraightProjectile(
                rightMuzzle,
                firingDirection,
                projectileDamage
            );

            return true;
        }

        public override void SetVisualActive(bool active)
        {
            if (visual != null)
            {
                visual.SetActive(active);
            }
        }

        public override void DrawGizmos(Turret turret)
        {
            if (leftSpawner == null || rightSpawner == null)
                return;

            Vector3 left = leftSpawner.position;
            Vector3 right = rightSpawner.position;
            Vector3 center = (left + right) * 0.5f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(left, 0.05f);
            Gizmos.DrawSphere(right, 0.05f);
            Gizmos.DrawSphere(center, 0.035f);
            Gizmos.DrawLine(left, right);

            if (turret.CurrentTarget != null)
            {
                Gizmos.DrawLine(
                    center,
                    turret.GetTargetAimPoint(turret.CurrentTarget)
                );
            }
        }

        private Transform FindChildTransform(
            Transform root,
            string childName
        )
        {
            if (root == null)
                return null;

            Transform[] children =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child != root && child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
