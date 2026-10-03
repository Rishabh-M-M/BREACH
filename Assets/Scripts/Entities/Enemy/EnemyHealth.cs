using UnityEngine;
using ProjectBreach.Core;  // Required for the full IDamageable interface

/// <summary>
/// EnemyHealth
/// Handles enemy health, damage intake, and the full death sequence.
///
/// GRASP: Information Expert - this class owns all HP-related data
///        (maxHealth, currentHealth, isDead), so it owns the damage logic.
///
/// Integration note:
/// Implements the FULL ProjectBreach.Core.IDamageable interface
/// (MaxHP / CurrentHP / IsDead / TakeDamage / Die).
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;      // Maximum hit points
    [SerializeField] private float despawnDelay = 3f;  // Seconds to wait before destroying the corpse

    // Runtime state 
    private int currentHealth;
    private Animator animator;
    private bool isDead;

    // ========== IDamageable implementation (full Core interface) ==========
    public int MaxHP => maxHealth;
    public int CurrentHP => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    /// <summary>
    /// IDamageable: Apply incoming damage.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        // Hit-flash hook, integrated from the shader/feedback pass.
        if (TryGetComponent(out HitEffectController hitEffect))
        {
            hitEffect.TriggerHit();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// IDamageable: Full death sequence.
    /// </summary>
    public void Die()
    {
        EventRegistry.BroadcastOnEnemyResolved();

        // Kill-only signal. OnEnemyResolved also counts breaches, and the upgrade
        // economy must not pay out for an enemy that merely reached the core.
        EventRegistry.BroadcastEnemyKilled();

        isDead = true;
        animator.SetTrigger("Die");

        if (TryGetComponent(out EnemyMover mover))
        {
            mover.enabled = false;
        }

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }

        if (TryGetComponent(out EnemyAttack attack))
        {
            attack.enabled = false;
        }

        Destroy(gameObject, despawnDelay);
    }

    /// <summary>
    /// Editor test helper: right-click the component header in Inspector
    /// </summary>
    [ContextMenu("Test - Take 25 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(25);
    }
}
