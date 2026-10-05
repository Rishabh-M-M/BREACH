using UnityEngine;

namespace ProjectBreach.Core
{
    public class MachineLaserTurret : Turret
    {
        [Header("Machine Laser")]

        [SerializeField] private float startingFireRate =
            0.35f;

        [SerializeField] private float minimumFireRate =
            0.05f;

        [SerializeField] private float rampTime =
            3f;

        [SerializeField] private float heatPerShot =
            0.08f;

        [SerializeField] private float heatDecayRate =
            0.15f;

        [SerializeField] private float overheatThreshold =
            1f;

        [SerializeField] private float cooldownTime =
            2f;

        [SerializeField] private int laserDamage =
            2;

        private float heat;
        private float rampTimer;

        private bool overheated;
        private float cooldownTimer;

        protected override void Awake()
        {
            base.Awake();

            damage = laserDamage;
            fireRate = startingFireRate;
        }

        protected override void Update()
        {
            if (overheated)
            {
                UpdateCooldown();

                UpdateActivationVisual();
                UpdateRecoil();

                return;
            }

            UpdateHeat();

            if (isActive && currentTarget != null)
            {
                rampTimer += Time.deltaTime;
            }
            else
            {
                rampTimer =
                    Mathf.Max(
                        0f,
                        rampTimer -
                        Time.deltaTime);
            }

            UpdateFireRate();

            base.Update();
        }

        private void UpdateHeat()
        {
            if (!isActive)
            {
                heat =
                    Mathf.MoveTowards(
                        heat,
                        0f,
                        heatDecayRate *
                        Time.deltaTime);
            }
            else
            {
                heat =
                    Mathf.MoveTowards(
                        heat,
                        0f,
                        heatDecayRate *
                        Time.deltaTime);
            }
        }

        private void UpdateFireRate()
        {
            float ramp =
                Mathf.Clamp01(
                    rampTimer / rampTime);

            fireRate =
                Mathf.Lerp(
                    startingFireRate,
                    minimumFireRate,
                    ramp);
        }

        protected override void Shoot()
        {
            if (projectilePrefab == null ||
                firePoint == null ||
                currentTarget == null)
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

            heat += heatPerShot;

            recoilAmount = 0.2f;

            if (heat >= overheatThreshold)
            {
                StartOverheat();
            }
        }

        private void StartOverheat()
        {
            overheated = true;
            cooldownTimer = cooldownTime;
            heat = overheatThreshold;

            Debug.Log(
                $"[Machine Laser] {name} overheated.");
        }

        private void UpdateCooldown()
        {
            cooldownTimer -= Time.deltaTime;

            heat =
                Mathf.MoveTowards(
                    heat,
                    0f,
                    Time.deltaTime /
                    cooldownTime);

            if (cooldownTimer <= 0f)
            {
                overheated = false;
                rampTimer = 0f;
                fireRate = startingFireRate;

                Debug.Log(
                    $"[Machine Laser] {name} cooled down.");
            }
        }

        public float HeatPercent =>
            Mathf.Clamp01(heat / overheatThreshold);

        public bool IsOverheated =>
            overheated;
    }
}