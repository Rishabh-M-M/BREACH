
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>Manages core HP, enemy breaches, breach visuals, and defeat.</summary>
    public class MainControlRoom : MonoBehaviour, IDamageable
    {
        [Header("Core Settings")]
        [SerializeField] private int maxCoreHP = 50;
        [SerializeField] private int coreDamagePerBreach = 10;

        [Header("Breach Visuals")]
        [SerializeField] private BreachController breachController;

        private int currentCoreHP;
        private bool gameOverSent;

        // Keeps track of rooms already counted.
        private HashSet<Room> fallenRooms = new HashSet<Room>();

        public int MaxHP => maxCoreHP;
        public int CurrentHP => currentCoreHP;
        public bool IsDead => currentCoreHP <= 0;

        private void Awake()
        {
            currentCoreHP = maxCoreHP;

            if (breachController == null)
                breachController = FindObjectOfType<BreachController>();

            Debug.Log($"[MainControlRoom] Initialized CoreHP: {currentCoreHP}/{maxCoreHP}");
        }

        private void OnEnable()
        {
            EventRegistry.OnRoomFallen += HandleRoomFallen;
        }

        private void OnDisable()
        {
            EventRegistry.OnRoomFallen -= HandleRoomFallen;
        }

        private void HandleRoomFallen(Room room)
        {
            if (room == null)
                return;

            // Ignore duplicate events for the same room.
            if (!fallenRooms.Add(room))
            {
                Debug.Log($"[MainControlRoom] Room '{room.RoomName}' was already counted.");
                return;
            }

            int newBreachLevel = Mathf.Clamp(fallenRooms.Count, 0, 4);

            Debug.LogWarning(
                $"[MainControlRoom] Room '{room.RoomName}' fell. " +
                $"Permanent breach level: {newBreachLevel}/4"
            );

            if (breachController == null)
                breachController = FindObjectOfType<BreachController>();

            if (breachController != null)
                breachController.SetPermanentBreachLevel(newBreachLevel);
            else
                Debug.LogWarning("[MainControlRoom] BreachController not found.");
        }

        /// <summary>Called when an enemy reaches the main-room breach zone.</summary>
        public void SpawnBreach(Enemy enemy)
        {
            Debug.Log("[MainControlRoom] Enemy breach received.");
            TriggerBreachVisuals();
            TakeDamage(coreDamagePerBreach);
        }

        /// <summary>Called when an enemy damages the main-room shield.</summary>
        public void NotifyMainShieldDamaged()
        {
            TriggerBreachVisuals();
        }

        public void TriggerBreachVisuals()
        {
            if (breachController == null)
                breachController = FindObjectOfType<BreachController>();

            if (breachController != null)
                breachController.StartBreach();
            else
                Debug.LogWarning("[MainControlRoom] BreachController is not assigned or found.");
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            currentCoreHP = Mathf.Max(0, currentCoreHP - amount);

            Debug.Log($"[MainControlRoom] CoreHP: {currentCoreHP}/{maxCoreHP}");

            EventRegistry.BroadcastCoreHPChanged(currentCoreHP, maxCoreHP);

            if (currentCoreHP <= 0)
                Die();
        }

        public void Die()
        {
            if (gameOverSent)
                return;

            gameOverSent = true;

            Debug.Log("[MainControlRoom] Core destroyed! Game over.");

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterCoreDestroyed();
            else
                Debug.Log("[MainControlRoom] GameManager.Instance is missing.");
        }

        /// <summary>Main shield break is an immediate defeat, independent of core HP.</summary>
        public void MainShieldBroken()
        {
            TriggerBreachVisuals();
            Die();
        }
    }
}
