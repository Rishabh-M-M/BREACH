using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Detects an enemy entering the Main Control Room breach zone.
/// The breach is routed through Enemy -> GameManager -> MainControlRoom.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BreachZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // GetComponentInParent allows the trigger to detect an Enemy
        // even when the collider belongs to a child object of the enemy.
        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy == null)
            return;

        Debug.Log(
            "[BreachZone] Enemy entered breach zone: "
            + enemy.gameObject.name
        );

        // Keep the team's existing breach flow:
        // Enemy -> GameManager -> MainControlRoom.
        enemy.Breach();
    }
}