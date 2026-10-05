using UnityEngine;

namespace ProjectBreach.Core
{
    public class LongRangeTurret : Turret
    {
        [Header("Long Range Shooter")]
        [SerializeField] private float longRange =
            25f;

        [SerializeField] private int closeDamage =
            2;

        [SerializeField] private int longRangeDamage =
            12;

        [SerializeField] private float longFireRate =
            2.5f;

        protected override void Awake()
        {
            base.Awake();

            fireRate = longFireRate;
            attackRange = longRange;
        }

        protected override void Shoot()
        {
            if (projectilePrefab == null ||
                firePoint == null ||
                currentTarget == null)
                return;

            int shotDamage =
                CalculateDamage();

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
                    shotDamage);
            }

            recoilAmount = 1f;
        }

        private int CalculateDamage()
        {
            MonoBehaviour target =
                currentTarget as MonoBehaviour;

            if (target == null)
                return closeDamage;

            float distance =
                Vector3.Distance(
                    transform.position,
                    target.transform.position);

            float normalizedDistance =
                Mathf.Clamp01(
                    distance / attackRange);

            return Mathf.RoundToInt(
                Mathf.Lerp(
                    closeDamage,
                    longRangeDamage,
                    normalizedDistance));
        }
    }
}