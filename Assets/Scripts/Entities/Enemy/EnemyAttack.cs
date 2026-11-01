
using UnityEngine;
using ProjectBreach.Core;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(Animator))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int damagePerShot = 10;

    [Min(0.1f)]
    [SerializeField] private float attackInterval = 2f;

    [Header("Projectile Settings")]
    [Tooltip("1 = original size, 2 = twice as large, 0.5 = half size.")]
    [Min(0.1f)]
    [SerializeField] private float projectileSizeMultiplier = 1f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Enemy Bullet Appearance")]
    [SerializeField] private Material enemyBulletMaterial;
    [SerializeField] private Material enemyTrailMaterial;
    [SerializeField] private GameObject enemyExplosionEffectPrefab;

    private EnemyMover mover;
    private Animator animator;
    private EnemyAudio audioPlayer;

    private float nextAttackTime;

    private void Awake()
    {
        mover = GetComponent<EnemyMover>();
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<EnemyAudio>();
    }

    /// <summary>
    /// Called via an Animation Event placed on the Shoot clip,
    /// at the exact frame the shot should actually fire.
    /// </summary>
    public void FireAtShield()
    {
        if (Time.time < nextAttackTime) return;
        if (!mover.HasArrived) return;

        Transform aim = mover.AimPoint;
        if (aim == null) return;

        Shield shield = aim.GetComponentInParent<Shield>();
        if (shield == null || shield.IsDead) return;

        if (bulletPrefab == null)
        {
            Debug.LogWarning("[EnemyAttack] No bullet prefab assigned!", this);
            return;
        }

        Vector3 spawnPosition = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 direction = shield.transform.position - spawnPosition;
        direction.y = 0f;

        direction = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : transform.forward;

        GameObject bulletObject = Instantiate(
            bulletPrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        // Apply adjustable projectile size.
        bulletObject.transform.localScale *= projectileSizeMultiplier;

        bulletObject.layer = LayerMask.NameToLayer("EnemyProjectile");

        if (enemyBulletMaterial != null)
        {
            Renderer bulletRenderer = bulletObject.GetComponentInChildren<Renderer>();
            if (bulletRenderer != null)
                bulletRenderer.sharedMaterial = enemyBulletMaterial;
        }

        TrailRenderer trail = bulletObject.GetComponentInChildren<TrailRenderer>();

        if (trail != null)
        {
            if (enemyTrailMaterial != null)
                trail.sharedMaterial = enemyTrailMaterial;

            Gradient redGradient = new Gradient();

            redGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.15f, 0f), 0f),
                    new GradientColorKey(new Color(0.6f, 0f, 0f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );

            trail.colorGradient = redGradient;
        }

        if (bulletObject.TryGetComponent(out Projectile projectile))
        {
            if (enemyExplosionEffectPrefab != null)
                projectile.SetExplosionEffectOverride(enemyExplosionEffectPrefab);

            projectile.InitializeStraight(direction, damagePerShot);
        }
        else
        {
            Debug.LogError("[EnemyAttack] Bullet prefab has no Projectile component.", this);
            Destroy(bulletObject);
            return;
        }

        nextAttackTime = Time.time + attackInterval;

        if (audioPlayer != null)
            audioPlayer.PlayAttack();
    }
}
