using UnityEngine;
using ProjectBreach.Core;

[RequireComponent(typeof(EnemyMover))]
public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private int damagePerShot = 10;
    [SerializeField] private float fireInterval = 1.5f;

    private EnemyMover mover;
    private float fireTimer;

    private void Awake()
    {
        mover = GetComponent<EnemyMover>();
    }

    private void Update()
    {
        if (!mover.HasArrived) return;

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            TryDealDamage();
        }
    }

    private void TryDealDamage()
    {
        Transform aim = mover.AimPoint;
        if (aim == null) return;

        // Damage shield
        Shield shield = aim.GetComponent<Shield>();
        if (shield != null)
        {
            shield.TakeDamage(damagePerShot);
            return;
        }

        // Damage turret
        IDamageable damageable = aim.GetComponentInParent<IDamageable>();
        damageable?.TakeDamage(damagePerShot);
    }
}