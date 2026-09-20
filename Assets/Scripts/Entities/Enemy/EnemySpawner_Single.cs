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

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
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

        GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);

        // Note: removed Claim() call - enemies share the same point now
        // targetPoint.Claim();  // Commented out for shared target

        if (enemy.TryGetComponent(out EnemyMover mover))
        {
            mover.SetTarget(targetPoint.transform, targetPoint);
            mover.SetAimPoint(aimPoint);
        }
    }

    [ContextMenu("Test - Spawn One Enemy")]
    private void TestSpawn()
    {
        SpawnEnemy();
    }
}
