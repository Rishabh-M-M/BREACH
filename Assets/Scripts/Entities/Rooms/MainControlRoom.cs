using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// MainControlRoom:
    /// - Manages Core HP
    /// - Handles enemy breaches
    /// - Starts breach visuals
    /// - Ends the game when Core HP reaches zero
    /// </summary>
    public class MainControlRoom : MonoBehaviour, IDamageable
    {
        [Header("Core Settings")]
        [SerializeField] private int maxCoreHP = 50;
        [SerializeField] private int coreDamagePerBreach = 10;

        // Added to connect the team's breach event to the custom
        // visual effects created for the graphics component.
        [Header("Breach Visuals")]
        [SerializeField] private BreachController breachController;

        private int currentCoreHP;

        public int MaxHP => maxCoreHP;

        public int CurrentHP => currentCoreHP;

        public bool IsDead => currentCoreHP <= 0;

        private void Awake()
        {
            currentCoreHP = maxCoreHP;

            Debug.Log(
                $"[MainControlRoom] Initialized CoreHP: " +
                $"{currentCoreHP}/{maxCoreHP}"
            );
        }

        /// <summary>
        /// Called by GameManager when an enemy successfully breaches.
        /// </summary>
        public void SpawnBreach(Enemy enemy)
        {
            Debug.Log("[MainControlRoom] Enemy breach received.");

            // Added: trigger the custom breach visual effect at the same
            // event where the main room receives the enemy breach.
            // This starts the screen shader, lights and camera shake.
            if (breachController != null)
            {
                breachController.StartBreach();
            }
            else
            {
                Debug.LogWarning(
                    "[MainControlRoom] BreachController is not assigned!"
                );
            }

            // Existing team gameplay logic: reduce the Core HP.
            TakeDamage(coreDamagePerBreach);

            Debug.Log(
                $"[MainControlRoom] Enemy breached! " +
                $"CoreHP -{coreDamagePerBreach}, " +
                $"remaining {currentCoreHP}/{maxCoreHP}"
            );
        }

        public void TakeDamage(int amount)
        {
            if (IsDead)
                return;

            currentCoreHP -= amount;

            if (currentCoreHP < 0)
                currentCoreHP = 0;

            Debug.Log(
                $"[MainControlRoom] CoreHP: " +
                $"{currentCoreHP}/{maxCoreHP}"
            );

            if (currentCoreHP <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            Debug.Log(
                "[MainControlRoom] Core destroyed! Game over."
            );

            OnCoreDestroyed();
        }

        private void OnCoreDestroyed()
        {
            GameManager.Instance.RegisterCoreDestroyed();
        }
    }
}