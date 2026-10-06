using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Ranged enemy that stops at distance and shoots.
/// Works with team components: EnemyHealth [2], EnemyMover [3], EnemyAttack [1]
/// No dependency on integrated Enemy base class [7].
/// </summary>
[RequireComponent(typeof(EnemyMover))]   // Depends on the teammate-maintained movement component [3]
public class RangedEnemy : MonoBehaviour
{
    [Header("Ranged Settings")]
    [SerializeField] private float attackRange = 12f;     // Stop and shoot at this range
    [SerializeField] private GameObject projectilePrefab; // Projectile to fire
    [SerializeField] private Transform firePoint;         // Where projectiles spawn

    private EnemyMover mover;      // Reference to team member's mover [3]
    private EnemyHealth health;    // Reference to team member's health [2]
    private float fireTimer;

    private void Awake()
    {
        mover = GetComponent<EnemyMover>();      // Get team member's component [3]
        health = GetComponent<EnemyHealth>();    // Get team member's component [2]

        if (mover == null) Debug.LogError("[RangedEnemy] Missing EnemyMover!");
        if (health == null) Debug.LogError("[RangedEnemy] Missing EnemyHealth!");
    }

    private void Update()
    {
        if (health != null && health.IsDead) return;  // Don't act if dead [2]

        // Check if we have a target from EnemyMover's AimPoint [3]
        if (mover.AimPoint == null) return;

        float distance = Vector3.Distance(transform.position, mover.AimPoint.position);

        // If in range, stop moving and shoot
        if (distance <= attackRange)
        {
            mover.enabled = false;  // Stop movement (freeze position)

            fireTimer += Time.deltaTime;
            if (fireTimer >= 1f / 2f)  // Attack speed hardcoded or from stats
            {
                fireTimer = 0f;
                FireProjectile();
            }
        }
        else
        {
            mover.enabled = true;   // Resume movement [3]
        }
    }

    /// <summary>
    /// Fire projectile at the aim point target
    /// </summary>
    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Initialize projectile if it has the Projectile component
        if (proj.TryGetComponent(out Projectile projectile))
        {
            // Get IDamageable from the aim point's parent (like EnemyAttack does) [1]
            IDamageable target = mover.AimPoint.GetComponentInParent<IDamageable>();
            projectile.Initialize(target, 10);  // Damage value, adjust as needed
        }
    }
}