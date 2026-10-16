using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Room exit zone: teleports enemies that reach it to the Main Control Room
/// spawn area, then retargets them toward the core breach point.
/// Distinct from the Main Control Room's BreachZone, which damages the core.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomExitZone : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform destinationPoint;     // MCR spawn area
    [SerializeField] private Transform newTarget;            // core breach target (walk destination after teleport)
    [SerializeField] private EnemyTargetPoint newTargetPoint; // optional occupancy tracking
    [SerializeField] private Transform newAimPoint;          // optional new aim point (the core)

    [Tooltip("Room index the enemy belongs to after teleporting here, using the same convention as Turret/Room (0 = Main Control Room). This zone sends enemies to the Main Control Room, so it defaults to 0.")]
    [SerializeField] private int destinationRoomIndex = 0;

    [Header("Spawn Spread")]
    [SerializeField] private float spreadRadius = 1.5f;      // random offset so enemies don't stack

    private void OnTriggerEnter(Collider other)
    {
        // GetComponentInParent (not TryGetComponent) so the trigger still detects the
        // Enemy when the collider belongs to a child object of the enemy.
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;
        if (destinationPoint == null)
        {
            Debug.LogWarning("[RoomExitZone] destinationPoint not assigned!");
            return;
        }

        Teleport(enemy);
    }

    private void Teleport(Enemy enemy)
    {
        // Random offset in a flat circle so multiple enemies spread out
        Vector2 offset = Random.insideUnitCircle * spreadRadius;
        Vector3 destination = destinationPoint.position + new Vector3(offset.x, 0f, offset.y);

        // Teleport: set Rigidbody position directly (safe for kinematic bodies)
        if (enemy.TryGetComponent(out Rigidbody rb))
        {
            rb.position = destination;
        }
        enemy.transform.position = destination;

        // Retarget mover toward the core breach point
        if (enemy.TryGetComponent(out EnemyMover mover))
        {
            mover.SetTarget(newTarget != null ? newTarget : destinationPoint, newTargetPoint);
            if (newAimPoint != null)
            {
                mover.SetAimPoint(newAimPoint);
            }

            // EnemyAudio's camera-visibility check (same policy as
            // TurretAudioController) needs to know this enemy is now in
            // the Main Control Room, not its original spawn room.
            mover.SetRoomIndex(destinationRoomIndex);
        }

        Debug.Log($"[RoomExitZone] Teleported {enemy.name} to Main Control Room spawn area");
    }
}