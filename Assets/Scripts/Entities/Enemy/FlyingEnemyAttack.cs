using UnityEngine;
using ProjectBreach.Core;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(Rigidbody))]
public class FlyingEnemyAttack : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damagePerShot = 10;
    [SerializeField] private float fireCooldown = 1.5f;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("Optional muzzle transform. Falls back to a point above the robot's root if left empty.")]
    [SerializeField] private Transform firePoint;

    [Header("Enemy Bullet Appearance")]
    [SerializeField] private Material enemyBulletMaterial;
    [SerializeField] private Material enemyTrailMaterial;
    [SerializeField] private GameObject enemyExplosionEffectPrefab;

    private EnemyMover mover;
    private EnemyHealth health;
    private Rigidbody rb;
    private float cooldownTimer;

    private void Awake()
    {
        mover = GetComponent<EnemyMover>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead) return;
        if (!mover.HasArrived) return;

        // Hold steady once arrived. Nothing else corrects position after
        // EnemyMover stops actively moving it, so a bump from another enemy
        // would otherwise leave it drifting on leftover velocity forever.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Shield shield = GetCurrentShield();
        if (shield == null || shield.IsDead) return;

        cooldownTimer -= Time.fixedDeltaTime;

        if (cooldownTimer <= 0f)
        {
            FireAtShield(shield);
            cooldownTimer = fireCooldown;
        }
    }

    private Shield GetCurrentShield()
    {
        Transform aim = mover.AimPoint;
        if (aim == null) return null;
        return aim.GetComponentInParent<Shield>();
    }

    private void FireAtShield(Shield shield)
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[FlyingEnemyAttack] No bullet prefab assigned!", this);
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

        bulletObject.layer = LayerMask.NameToLayer("EnemyProjectile");

        if (enemyBulletMaterial != null)
        {
            Renderer bulletRenderer = bulletObject.GetComponentInChildren<Renderer>();
            if (bulletRenderer != null)
            {
                bulletRenderer.sharedMaterial = enemyBulletMaterial;
            }
        }

        TrailRenderer trail = bulletObject.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            if (enemyTrailMaterial != null)
            {
                trail.sharedMaterial = enemyTrailMaterial;
            }

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
            {
                projectile.SetExplosionEffectOverride(enemyExplosionEffectPrefab);
            }

            projectile.InitializeStraight(direction, damagePerShot);
        }
        else
        {
            Debug.LogError("[FlyingEnemyAttack] Bullet prefab has no Projectile component.", this);
        }
    }
}