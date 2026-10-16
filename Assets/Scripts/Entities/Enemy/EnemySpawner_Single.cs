using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy spawner that allows multiple enemies to share the same target point.
/// Removed occupancy check for Demo simplification.
/// </summary>
public class EnemySpawner_Single : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<EnemyTargetPoint> targetPoints;
    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private Transform aimPoint;

    [Header("Spawn Position Variation")]
    [Tooltip("How far left/right of the spawner's own position enemies can spawn.")]
    [SerializeField] private float spawnSideOffset = 1f;

    [Header("Wave System")]
    [Tooltip("When false, this spawner waits for WaveManager to call SpawnWave(). Old demo scenes keep autoStart on.")]
    [SerializeField] private bool autoStart = false;

    private void Start()
    {
        if (autoStart)
        {
            StartCoroutine(SpawnRoutine(enemiesToSpawn));
        }
    }

    /// <summary>
    /// Public entry point for WaveManager. Spawns <paramref name="count"/> enemies
    /// through the existing routine/parameters. Fire-and-forget coroutine.
    /// </summary>
    public void SpawnWave(int count)
    {
        if (count <= 0) return;
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
        // Simplified: use first available point, ignore occupancy
        EnemyTargetPoint targetPoint = targetPoints.Count > 0 ? targetPoints[0] : null;

        if (targetPoint == null)
        {
            Debug.Log("[EnemySpawner] No target points configured.");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();

        if (enemyPrefab.TryGetComponent(out SpawnHeightOffset heightOffset))
        {
            spawnPosition += Vector3.up * heightOffset.HeightOffset;
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);

        // Note: removed Claim() call - enemies share the same point now
        // targetPoint.Claim();  // Commented out for shared target

        if (enemy.TryGetComponent(out EnemyMover mover))
        {
            mover.SetTarget(targetPoint.transform, targetPoint);
            mover.SetAimPoint(aimPoint);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        int choice = Random.Range(0, 3); // 0 = centre, 1 = left, 2 = right

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