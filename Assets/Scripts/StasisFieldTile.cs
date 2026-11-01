using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One tile in a Stasis Field group.
///
/// Animator bools:
/// - Powered
/// - EnemyDetected
///
/// The field waits after activation, then repeatedly pulls for a short burst
/// and releases. Enemies continue their normal movement during release phases.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class StasisFieldTile : MonoBehaviour
{
    [Header("Pull Effect")]
    [Tooltip("Pull speed in world units per second, configured by TrapGroup.")]
    [Min(0f)]
    [SerializeField] private float pullSpeed = 4f;

    [Tooltip("Delay after powering on before the first pull cycle.")]
    [Min(0f)]
    [SerializeField] private float startupDelay = 0.8f;

    [Tooltip("How long each pull burst lasts.")]
    [Min(0.05f)]
    [SerializeField] private float pullDuration = 0.3f;

    [Tooltip("How long the field releases enemies between pull bursts.")]
    [Min(0f)]
    [SerializeField] private float releaseDuration = 0.8f;

    [Tooltip("How often the pull is refreshed during a burst.")]
    [Min(0.02f)]
    [SerializeField] private float pullRefreshInterval = 0.08f;

    [Tooltip("Each refresh remains active slightly longer than the refresh interval.")]
    [Min(0.05f)]
    [SerializeField] private float pullApplicationDuration = 0.2f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask enemyLayers = ~0;

    [Header("Ring Material")]
    [Tooltip("Assign only ring renderers if other child materials should not change.")]
    [SerializeField] private Renderer[] ringRenderers;
    [Min(0.01f)]
    [SerializeField] private float materialTransitionSpeed = 2.5f;

    private static readonly int PoweredHash = Animator.StringToHash("Powered");
    private static readonly int EnemyDetectedHash = Animator.StringToHash("EnemyDetected");
    private static readonly int MaterialPoweredHash = Shader.PropertyToID("_Powered");

    private MaterialPropertyBlock propertyBlock;
    private Animator animator;
    private bool powered;
    private float materialPowered;
    private Coroutine pullCycleRoutine;

    private readonly Dictionary<Collider, EnemyMover> colliderOwners = new();
    private readonly Dictionary<EnemyMover, int> enemyColliderCounts = new();

    public bool IsPowered => powered;
    public bool HasEnemiesInside => enemyColliderCounts.Count > 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        propertyBlock = new MaterialPropertyBlock();

        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;

        powered = false;
        animator.SetBool(PoweredHash, false);
        animator.SetBool(EnemyDetectedHash, false);

        if (ringRenderers == null || ringRenderers.Length == 0)
            ringRenderers = GetComponentsInChildren<Renderer>(true);

        materialPowered = 0f;
        UpdateRingMaterial(0f);
    }

    private void Update()
    {
        float target = powered ? 1f : 0f;
        materialPowered = Mathf.MoveTowards(
            materialPowered,
            target,
            materialTransitionSpeed * Time.deltaTime
        );

        UpdateRingMaterial(materialPowered);
    }

    private void UpdateRingMaterial(float value)
    {
        if (ringRenderers == null || propertyBlock == null)
            return;

        foreach (Renderer ringRenderer in ringRenderers)
        {
            if (ringRenderer == null)
                continue;

            bool usesStasisShader = false;
            foreach (Material material in ringRenderer.sharedMaterials)
            {
                if (material != null && material.HasProperty(MaterialPoweredHash))
                {
                    usesStasisShader = true;
                    break;
                }
            }

            if (!usesStasisShader)
                continue;

            ringRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(MaterialPoweredHash, value);
            ringRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    /// <summary>
    /// Called by TrapGroup. The second value is the duration of each pull burst.
    /// Startup delay and release interval are configured per tile in the Inspector.
    /// </summary>
    public void Configure(float configuredPullSpeed, float configuredPullDuration)
    {
        pullSpeed = Mathf.Max(0f, configuredPullSpeed);
        pullDuration = Mathf.Max(0.05f, configuredPullDuration);
    }

    // Compatibility with TrapGroup.SetTilesActive(bool).
    public void SetActive(bool value)
    {
        SetPowered(value);
    }

    public void SetPowered(bool value)
    {
        if (powered == value)
            return;

        powered = value;
        animator.SetBool(PoweredHash, powered);

        if (powered)
        {
            if (enemyColliderCounts.Count > 0)
                StartPullCycle();
        }
        else
        {
            StopPullCycleAndReleaseEnemies();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterEnemyCollider(other);
    }

    private void OnTriggerStay(Collider other)
    {
        RegisterEnemyCollider(other);
    }

    private void OnTriggerExit(Collider other)
    {
        UnregisterEnemyCollider(other);
    }

    private void RegisterEnemyCollider(Collider other)
    {
        if (other == null || colliderOwners.ContainsKey(other))
            return;

        if ((enemyLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        EnemyMover enemy = other.GetComponentInParent<EnemyMover>();
        if (enemy == null)
            return;

        colliderOwners.Add(other, enemy);

        if (enemyColliderCounts.TryGetValue(enemy, out int count))
            enemyColliderCounts[enemy] = count + 1;
        else
            enemyColliderCounts.Add(enemy, 1);

        UpdateEnemyDetected();

        if (powered)
            StartPullCycle();
    }

    private void UnregisterEnemyCollider(Collider other)
    {
        if (other == null || !colliderOwners.TryGetValue(other, out EnemyMover enemy))
            return;

        colliderOwners.Remove(other);

        if (enemy != null && enemyColliderCounts.TryGetValue(enemy, out int count))
        {
            if (count <= 1)
                enemyColliderCounts.Remove(enemy);
            else
                enemyColliderCounts[enemy] = count - 1;

            if (!enemyColliderCounts.ContainsKey(enemy))
                enemy.ClearPull();
        }

        UpdateEnemyDetected();

        if (enemyColliderCounts.Count == 0)
            StopPullCycle();
    }

    private void UpdateEnemyDetected()
    {
        if (animator != null)
            animator.SetBool(EnemyDetectedHash, enemyColliderCounts.Count > 0);
    }

    private void StartPullCycle()
    {
        if (!powered || enemyColliderCounts.Count == 0 || pullCycleRoutine != null)
            return;

        pullCycleRoutine = StartCoroutine(PullCycle());
    }

    private void StopPullCycle()
    {
        if (pullCycleRoutine != null)
        {
            StopCoroutine(pullCycleRoutine);
            pullCycleRoutine = null;
        }
    }

    private void StopPullCycleAndReleaseEnemies()
    {
        StopPullCycle();

        foreach (EnemyMover enemy in new List<EnemyMover>(enemyColliderCounts.Keys))
        {
            if (enemy != null)
                enemy.ClearPull();
        }
    }

    private IEnumerator PullCycle()
    {
        // Delay the first pull after the field powers on / first detects an enemy.
        if (startupDelay > 0f)
            yield return new WaitForSeconds(startupDelay);

        while (powered && enemyColliderCounts.Count > 0)
        {
            // PULL PHASE: refresh the pull while the burst is active.
            float elapsed = 0f;
            while (powered && enemyColliderCounts.Count > 0 && elapsed < pullDuration)
            {
                ApplyPullToCurrentEnemies();
                yield return new WaitForSeconds(pullRefreshInterval);
                elapsed += pullRefreshInterval;
            }

            // RELEASE PHASE: cancel the pull so normal forward movement resumes.
            ClearPullFromCurrentEnemies();

            if (!powered || enemyColliderCounts.Count == 0)
                break;

            if (releaseDuration > 0f)
                yield return new WaitForSeconds(releaseDuration);
        }

        pullCycleRoutine = null;
    }

    private void ApplyPullToCurrentEnemies()
    {
        List<EnemyMover> enemies = new List<EnemyMover>(enemyColliderCounts.Keys);

        foreach (EnemyMover enemy in enemies)
        {
            if (enemy == null)
            {
                enemyColliderCounts.Remove(enemy);
                continue;
            }

            enemy.ApplyPull(transform.position, pullSpeed, pullApplicationDuration);
        }

        UpdateEnemyDetected();
    }

    private void ClearPullFromCurrentEnemies()
    {
        foreach (EnemyMover enemy in new List<EnemyMover>(enemyColliderCounts.Keys))
        {
            if (enemy != null)
                enemy.ClearPull();
        }
    }

    private void OnDisable()
    {
        StopPullCycleAndReleaseEnemies();
        powered = false;
        materialPowered = 0f;
        UpdateRingMaterial(0f);

        if (animator != null)
        {
            animator.SetBool(PoweredHash, false);
            animator.SetBool(EnemyDetectedHash, false);
        }

        colliderOwners.Clear();
        enemyColliderCounts.Clear();
    }
}
