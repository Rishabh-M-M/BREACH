using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
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
        EnemyTargetPoint freeTarget = targetPoints.Find(t => !t.IsOccupied);
        if (freeTarget == null)
        {
            Debug.Log("No free target points available.");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        freeTarget.Claim();

        if (enemy.TryGetComponent(out EnemyMover mover))
        {
            mover.SetTarget(freeTarget.transform, freeTarget);
            mover.SetAimPoint(aimPoint);
        }
    }

    [ContextMenu("Test - Spawn One Enemy")]
    private void TestSpawn()
    {
        SpawnEnemy();
    }
}