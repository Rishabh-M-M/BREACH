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

        // Enemies damage shields by physically colliding with them.
        // Turrets no longer have health and are never damaged by this attack.
        Shield shield = aim.GetComponentInParent<Shield>();
        if (shield != null) return;
    }
}