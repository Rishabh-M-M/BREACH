using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Boss enemy with EMP, Jam, and SpawnAdds abilities.
/// Works with team components: EnemyHealth [2], EnemyMover [3], EnemyAttack [1]
/// Uses GameManager as mediator (Indirection) to access other managers.
/// </summary>
[RequireComponent(typeof(EnemyMover))]   // Depends on the teammate-maintained movement component [3]
[RequireComponent(typeof(EnemyHealth))]  // Depends on the teammate-maintained health component [2]
public class Boss : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private int phase = 1;
    [SerializeField] private float phase2HealthPercent = 0.6f;
    [SerializeField] private float phase3HealthPercent = 0.3f;

    [Header("EMP Ability")]
    [SerializeField] private float empCooldown = 15f;
    [SerializeField] private float empDuration = 4f;
    [SerializeField] private float empTimer = 0f;

    [Header("Camera Jam")]
    [SerializeField] private float jamCooldown = 20f;
    [SerializeField] private float jamDuration = 3f;
    [SerializeField] private float jamTimer = 0f;

    [Header("Spawn Adds")]
    [SerializeField] private int addsCount = 3;
    [SerializeField] private float addsCooldown = 25f;
    [SerializeField] private float addsTimer = 0f;

    private EnemyMover mover;      // Team member's component [3]
    private EnemyHealth health;    // Team member's component [2]

    private void Awake()
    {
        mover = GetComponent<EnemyMover>();      // [3]
        health = GetComponent<EnemyHealth>();    // [2]
    }

    private void Update()
    {
        if (health == null || health.IsDead) return;  // Check if dead [2]

        // Update cooldowns
        if (empTimer > 0) empTimer -= Time.deltaTime;
        if (jamTimer > 0) jamTimer -= Time.deltaTime;
        if (addsTimer > 0) addsTimer -= Time.deltaTime;

        // Check phase transitions
        CheckPhaseTransition();

        // Auto-cast abilities when in combat (HasArrived from EnemyMover [3])
        if (mover.HasArrived)
        {
            TryCastEMP();
            TryCastJam();
            TrySpawnAdds();
        }
    }

    /// <summary>
    /// Change behavior based on health percentage
    /// </summary>
    private void CheckPhaseTransition()
    {
        float healthPercent = (float)health.CurrentHP / health.MaxHP;

        if (phase == 1 && healthPercent <= phase2HealthPercent)
        {
            phase = 2;
            OnPhaseChanged(2);
        }
        else if (phase == 2 && healthPercent <= phase3HealthPercent)
        {
            phase = 3;
            OnPhaseChanged(3);
        }
    }

    private void OnPhaseChanged(int newPhase)
    {
        Debug.Log($"[Boss] Phase {newPhase} activated!");

        // Adjust team member's component values
        if (newPhase >= 2)
        {
            // Phase 2: Attack faster (adjust EnemyAttack interval)
            if (TryGetComponent(out EnemyAttack attack))
            {
                // Note: You might need to add a public method to EnemyAttack to change fireInterval
                // Or use reflection (not recommended), or just let it be
            }
        }
    }

    /// <summary>
    /// EMP: Disable room power via GameManager (Indirection)
    /// </summary>
    private void TryCastEMP()
    {
        if (empTimer > 0) return;

        // Find a room (simplified - you might want specific logic)
        Room targetRoom = FindObjectOfType<Room>();  // Or random from list

        if (targetRoom != null)
        {
            GameManager.Instance.RequestEMP(targetRoom.RoomIndex, empDuration);
            empTimer = empCooldown;
            Debug.Log($"[Boss] EMP cast on Room {targetRoom.RoomIndex}");
        }
    }

    /// <summary>
    /// Jam camera via GameManager (Indirection)
    /// </summary>
    private void TryCastJam()
    {
        if (jamTimer > 0) return;

        Room targetRoom = FindObjectOfType<Room>();
        if (targetRoom != null)
        {
            GameManager.Instance.RequestJam(targetRoom.RoomIndex, jamDuration);
            jamTimer = jamCooldown;
        }
    }

    /// <summary>
    /// Spawn additional enemies via GameManager (Indirection)
    /// </summary>
    private void TrySpawnAdds()
    {
        if (addsTimer > 0) return;

        GameManager.Instance.RequestSpawnAdds(addsCount);
        addsTimer = addsCooldown;
    }
}