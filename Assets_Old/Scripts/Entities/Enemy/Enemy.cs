using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Enemy (minimal identity component).
    /// Lives on the same GameObject as EnemyHealth [2], EnemyMover [3],
    /// and EnemyAttack [1]. Its only job is to provide the Breach()
    /// entry point so GameManager.NotifyBreach(Enemy) receives the
    /// correct type. All real behavior stays in the team components.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        /// <summary>
        /// Called by BreachZone.OnTriggerEnter when this enemy reaches the exit.
        /// Routes through GameManager (Indirection), then despawns.
        /// </summary>
        public virtual void Breach()
        {
            Debug.Log("[Enemy] Breach initiated!");

            // Broadcast resolution before damage chain (design doc D1: breach counts as resolved)
            EnemyHealth health = GetComponent<EnemyHealth>();
            if (health == null || !health.IsDead)
            {
                EventRegistry.BroadcastOnEnemyResolved();
            }

            GameManager.Instance.NotifyBreach(this);
            gameObject.SetActive(false);
        }
    }
}