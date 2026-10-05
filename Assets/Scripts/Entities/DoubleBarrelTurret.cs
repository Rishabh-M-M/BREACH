using UnityEngine;

namespace ProjectBreach.Core
{
    public class DoubleBarrelTurret : Turret
    {
        [Header("Double Barrel")]
        [SerializeField] private float damageMultiplier = 1.5f;

        protected override void Awake()
        {
            base.Awake();

            damage = Mathf.RoundToInt(
                damage * damageMultiplier);

            fireRate *= 0.85f;
            attackRange *= 0.85f;
        }

        protected override void Shoot()
        {
            if (projectilePrefab == null ||
                firePoint == null ||
                currentTarget == null)
                return;

            // First barrel
            FireProjectile();

            // Second barrel
            FireProjectile();

            recoilAmount = 1f;
        }

        private void FireProjectile()
        {
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
        }
    }
}