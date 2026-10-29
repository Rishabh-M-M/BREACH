
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Enemy spawner that allows multiple enemies to share the same target point.
/// Enemies can share target points, and each spawner has a configurable room
/// index for minimap tracking.
/// </summary>
public class EnemySpawner_Single : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<EnemyTargetPoint> targetPoints;
    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private Transform aimPoint;

    [Header("Spawn Position Variation")]
    [Tooltip("How far left/right of the spawner's own position enemies can spawn.")]
    [SerializeField] private float spawnSideOffset = 1f;

    [Header("Minimap")]
    [Tooltip("Room number for minimap tracking: Room 1 = 1, Room 2 = 2, Room 3 = 3, Room 4 = 4.")]
    [SerializeField, Range(1, 4)] private int roomIndex = 1;

    [Header("Wave System")]
    [Tooltip("When false, this spawner waits for WaveManager to call SpawnWave().")]
    [SerializeField] private bool autoStart = false;

    private void Start()
    {
        if (autoStart)
        {
            StartCoroutine(SpawnRoutine(enemiesToSpawn));
        }
    }

    /// <summary>
    /// Public entry point for WaveManager.
    /// Spawns the requested number of enemies.
    /// </summary>
    public void SpawnWave(int count)
    {
        if (count <= 0)
            return;

        StartCoroutine(SpawnRoutine(count));
    }

    private IEnumerator SpawnRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnEnemy()
    {
        // Use the first configured target point.
        if (targetPoints == null || targetPoints.Count == 0)
        {
            Debug.LogWarning(
                $"[EnemySpawner] {name}: No target points configured.",
                this
            );
            return;
        }

        EnemyTargetPoint targetPoint = targetPoints[0];

        if (targetPoint == null)
        {
            Debug.LogWarning(
                $"[EnemySpawner] {name}: The first target point is empty.",
                this
            );
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                $"[EnemySpawner] {name}: No enemy prefab assigned.",
                this
            );
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Apply the enemy's optional spawn height offset.
        if (enemyPrefab.TryGetComponent(out SpawnHeightOffset heightOffset))
        {
            spawnPosition += Vector3.up * heightOffset.HeightOffset;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPosition,
            transform.rotation
        );

        if (enemy.TryGetComponent(out EnemyMover mover))
        {
            mover.SetTarget(targetPoint.transform, targetPoint);
            mover.SetAimPoint(aimPoint);

            // Assign the manually configured room index.
            // This works even if no Room component exists in the parent hierarchy.
            mover.SetRoomIndex(roomIndex);

            Debug.Log(
                $"[Minimap Debug] Spawner: {name} | " +
                $"Assigned room index: {roomIndex} | " +
                $"Enemy: {enemy.name}",
                this
            );
        }
        else
        {
            Debug.LogWarning(
                $"[EnemySpawner] {name}: Spawned enemy has no EnemyMover component.",
                enemy
            );
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // 0 = centre, 1 = left, 2 = right.
        int choice = Random.Range(0, 3);

        switch (choice)
        {
            case 1:
                return transform.position - transform.right * spawnSideOffset;

            case 2:
                return transform.position + transform.right * spawnSideOffset;

            default:
                return transform.position;
        }
    }

    [ContextMenu("Test - Spawn One Enemy")]
    private void TestSpawn()
    {
        SpawnEnemy();
    }
}
