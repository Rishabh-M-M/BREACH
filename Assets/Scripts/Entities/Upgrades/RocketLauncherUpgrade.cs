using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Rocket Launcher variant. Owns its model/head, rocket prefab, eight
    /// firing points, magazine, reload, damage and range.
    /// </summary>
    public class RocketLauncherUpgrade : TurretUpgrade
    {
        [Header("Rocket Launcher Visual")]
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Transform turretHead;

        [Header("Rocket Projectile")]
        [SerializeField] private GameObject rocketProjectilePrefab;
        [SerializeField] private GameObject blueExplosionPrefab;
        [SerializeField] private Transform[] firingPoints = new Transform[8];
        [Tooltip("Separate empty-tube visuals. Never assign the firing-point GameObjects here.")]
        [SerializeField] private GameObject[] spentTubeVisuals = new GameObject[8];

        [Header("Magazine")]
        [SerializeField, Min(1)] private int magazineSize = 8;
        [SerializeField, Min(0.1f)] private float reloadDuration = 5f;
        [SerializeField, Min(0.01f)] private float timeBetweenRockets = 0.25f;

        [Header("Rocket Stats")]
        [SerializeField, Min(0)] private int directDamage = 12;
        [SerializeField, Min(0)] private int explosionDamage = 75;
        [SerializeField, Min(0.1f)] private float explosionRadius = 3f;
        [SerializeField, Min(0.1f)] private float rocketSpeed = 15f;
        [SerializeField, Min(0.1f)] private float attackRange = 100f;

        [Header("Power Costs")]
        [SerializeField, Min(0)] private int permanentPowerCost = 40;
        [SerializeField, Min(0)] private int operatingPowerCost = 40;

        [Header("Audio")]
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip rotationSound;
        [SerializeField] private AudioClip explosionSound;
        [SerializeField, Range(0f, 1f)] private float fireVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;

        [Header("Explosion Visual Timing")]
        [Tooltip("Seconds for the explosion visual to expand toward the damage radius.")]
        [SerializeField, Min(0.1f)] private float explosionEffectDuration = 1.5f;

        private int rocketsRemaining;
        private int nextFiringPoint;
        private float nextShotTime;
        private float reloadCompleteTime;
        private bool isReloading;

        public override int PermanentPowerCost => permanentPowerCost;
        public override int OperatingPowerCost => operatingPowerCost;
        public int RocketsRemaining => rocketsRemaining;
        public bool IsReloading => isReloading;
        public float FireVolume => fireVolume;
        public float ExplosionVolume => explosionVolume;
        public float ExplosionEffectDuration => explosionEffectDuration;

        public override string UpgradeInfo =>
            $"Launches {magazineSize} explosive rockets before reloading. " +
            $"Direct damage: {directDamage}. " +
            $"Explosion damage: {explosionDamage}. " +
            $"Explosion radius: {explosionRadius:0.##}. " +
            $"Reload: {reloadDuration:0.##} seconds. " +
            $"Range: {attackRange:0.##}.";
        private void Awake()
        {
            magazineSize = Mathf.Max(1, magazineSize);
            rocketsRemaining = magazineSize;

            if (turretHead == null)
                turretHead = FindChildTransform(transform, "RL head");
            if (turretHead == null)
                turretHead = FindChildTransform(transform, "Rocket Launcher head");
            if (turretHead == null)
                turretHead = FindChildTransform(transform, "RocketLauncher head");

            ValidateReferences();
            ResetSpentTubeVisuals();
        }

        private void Update()
        {
            if (isReloading && Time.time >= reloadCompleteTime)
                FinishReload();
        }

        public override int GetDamage(int baseDamage) => directDamage;
        public override float GetFireRate(float baseFireRate) => timeBetweenRockets;
        public override float GetAttackRange(float baseRange) => attackRange;
        public override AudioClip GetFireSound()
        {
            if (fireSound != null)
                return fireSound;

            return TurretAudioController.Instance != null
                ? TurretAudioController.Instance.GetFireSound(TurretVariant.RocketLauncher)
                : null;
        }
        public override AudioClip GetRotationSound() => rotationSound;

        public override Transform GetTurretHead(Transform baseHead)
        {
            return turretHead != null ? turretHead : baseHead;
        }

        public override Transform GetFirePoint(Transform baseFirePoint)
        {
            if (firingPoints != null)
            {
                for (int i = 0; i < firingPoints.Length; i++)
                    if (firingPoints[i] != null)
                        return firingPoints[i];
            }
            return turretHead != null ? turretHead : baseFirePoint;
        }

        public override void SetVisualActive(bool active)
        {
            if (visualRoot != null)
                visualRoot.SetActive(active);

            if (active)
                RefreshSpentTubeVisuals();
        }

        public override bool TryFire(Turret turret, IDamageable target, int projectileDamage)
        {
            if (turret == null || target == null || target.IsDead)
                return false;

            return TryFireRocket(turret.GetTargetAimPoint(target));
        }

        private bool TryFireRocket(Vector3 targetPosition)
        {
            if (!isActiveAndEnabled || isReloading || Time.time < nextShotTime)
                return false;

            if (rocketsRemaining <= 0)
            {
                BeginReload();
                return false;
            }

            if (rocketProjectilePrefab == null)
            {
                Debug.LogError("[RocketLauncherUpgrade] Rocket Projectile Prefab is not assigned.", this);
                return false;
            }

            if (firingPoints == null || firingPoints.Length < magazineSize)
            {
                Debug.LogError($"[RocketLauncherUpgrade] Assign {magazineSize} firing points in this component.", this);
                return false;
            }

            int index = nextFiringPoint;
            Transform muzzle = firingPoints[index];
            if (muzzle == null)
            {
                Debug.LogError($"[RocketLauncherUpgrade] Firing Point {index} is not assigned.", this);
                return false;
            }

            Vector3 direction = targetPosition - muzzle.position;
            if (direction.sqrMagnitude < 0.0001f)
                direction = muzzle.forward;
            direction.Normalize();

            GameObject rocketObject = Instantiate(
                rocketProjectilePrefab,
                muzzle.position,
                Quaternion.LookRotation(direction, Vector3.up));

            if (!rocketObject.TryGetComponent(out Projectile projectile))
            {
                Debug.LogError("[RocketLauncherUpgrade] Rocket prefab must have a Projectile component.", rocketObject);
                Destroy(rocketObject);
                return false;
            }

            projectile.SetExplosionEffectOverride(blueExplosionPrefab);

            AudioClip resolvedExplosionSound = explosionSound;
            float resolvedExplosionVolume = explosionVolume;
            if (resolvedExplosionSound == null && TurretAudioController.Instance != null)
            {
                resolvedExplosionSound = TurretAudioController.Instance.RocketLauncherExplosionSound;
                resolvedExplosionVolume = TurretAudioController.Instance.RocketLauncherExplosionVolume;
            }

            projectile.SetRocketExplosionSettings(
                resolvedExplosionSound,
                explosionEffectDuration,
                resolvedExplosionVolume);

            // The turret owns launch audio playback after TryFire succeeds.
            // Do not call PlayClipAtPoint here or the launch sound may double-play.
            projectile.LaunchRocket(
                targetPosition,
                directDamage,
                explosionDamage,
                explosionRadius,
                rocketSpeed);

            if (spentTubeVisuals != null && index < spentTubeVisuals.Length &&
                spentTubeVisuals[index] != null && !IsFiringPointOrAncestor(spentTubeVisuals[index].transform))
            {
                spentTubeVisuals[index].SetActive(true);
            }

            nextFiringPoint++;
            rocketsRemaining--;
            nextShotTime = Time.time + timeBetweenRockets;

            if (rocketsRemaining <= 0)
                BeginReload();

            return true;
        }

        private void BeginReload()
        {
            if (isReloading)
                return;
            isReloading = true;
            reloadCompleteTime = Time.time + reloadDuration;
        }

        private void FinishReload()
        {
            isReloading = false;
            rocketsRemaining = magazineSize;
            nextFiringPoint = 0;
            ResetSpentTubeVisuals();
        }

        private void ResetSpentTubeVisuals()
        {
            if (spentTubeVisuals == null)
                return;
            foreach (GameObject visual in spentTubeVisuals)
            {
                if (visual != null && !IsFiringPointOrAncestor(visual.transform))
                    visual.SetActive(false);
            }
        }

        private void RefreshSpentTubeVisuals()
        {
            if (spentTubeVisuals == null)
                return;
            int firedCount = magazineSize - rocketsRemaining;
            for (int i = 0; i < spentTubeVisuals.Length; i++)
            {
                GameObject visual = spentTubeVisuals[i];
                if (visual != null && !IsFiringPointOrAncestor(visual.transform))
                    visual.SetActive(i < firedCount);
            }
        }

        private bool IsFiringPointOrAncestor(Transform candidate)
        {
            if (candidate == null || firingPoints == null)
                return false;
            foreach (Transform muzzle in firingPoints)
            {
                if (muzzle != null && (muzzle == candidate || muzzle.IsChildOf(candidate)))
                    return true;
            }
            return false;
        }

        private void ValidateReferences()
        {
            if (rocketProjectilePrefab == null)
                Debug.LogWarning("[RocketLauncherUpgrade] Assign Rocket Projectile Prefab.", this);
            if (firingPoints == null || firingPoints.Length < magazineSize)
                Debug.LogWarning($"[RocketLauncherUpgrade] Assign {magazineSize} firing points.", this);
            if (turretHead == null)
                Debug.LogWarning("[RocketLauncherUpgrade] RL turret head was not found. Assign it manually for rotation.", this);
        }

        private Transform FindChildTransform(Transform root, string childName)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
                if (child != root && child.name == childName)
                    return child;
            return null;
        }
    }
}
