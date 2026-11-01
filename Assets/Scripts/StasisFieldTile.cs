using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls one Stasis Field tile.
///
/// Animator setup:
/// - Bool parameter "Powered"
/// - Bool parameter "EnemyDetected"
///
/// Expected transitions:
/// Entry -> ViewHolo
/// ViewHolo -> HoloEnd        (Powered = true)
/// HoloEnd -> Idle            (Powered = true, Has Exit Time)
/// HoloEnd -> ViewHolo        (Powered = false, Has Exit Time)
/// Idle -> ComingHolo         (EnemyDetected = true)
/// ComingHolo -> Idle         (Has Exit Time, Powered = true)
/// Idle -> HoloEnd            (Powered = false)
/// ComingHolo -> HoloEnd      (Powered = false)
///
/// Attach this script to the same GameObject as the Animator and trigger Collider.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class StasisFieldTile : MonoBehaviour
{
    [Header("Stasis Effect")]
    [Tooltip("Enemy speed multiplier while inside the field. 0.5 = 50% speed.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float slowMultiplier = 0.5f;

    [Tooltip("How often the slow effect is refreshed.")]
    [Min(0.05f)]
    [SerializeField] private float slowRefreshInterval = 0.1f;

    [Tooltip("Duration of each refreshed slow application. Keep this longer than the refresh interval.")]
    [Min(0.1f)]
    [SerializeField] private float slowApplicationDuration = 0.25f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask enemyLayers = ~0;

    [Header("Ring Material")]
    [Tooltip("Optional. If empty, all child renderers are used. Assign only the ring renderers if other child materials should not change.")]
    [SerializeField] private Renderer[] ringRenderers;
    [Min(0.01f)]
    [SerializeField] private float materialTransitionSpeed = 2.5f;

    private static readonly int PoweredHash = Animator.StringToHash("Powered");
    private static readonly int EnemyDetectedHash = Animator.StringToHash("EnemyDetected");
    private static readonly int MaterialPoweredHash = Shader.PropertyToID("_Powered");

    // Unity native objects must be created in Awake/Start, not in a
    // MonoBehaviour field initializer or constructor.
    private MaterialPropertyBlock propertyBlock;
    private float materialPowered;

    private Animator animator;
    private bool powered;
    private Coroutine slowRoutine;

    // Track colliders individually so enemies with multiple colliders are handled correctly.
    private readonly Dictionary<Collider, EnemyMover> colliderOwners = new();
    private readonly Dictionary<EnemyMover, int> enemyColliderCounts = new();

    public bool IsPowered => powered;
    public bool HasEnemiesInside => enemyColliderCounts.Count > 0;

    private void Awake()
    {
        // MaterialPropertyBlock wraps a Unity native object. Creating it here
        // avoids UnityException: CreateImpl is not allowed from a constructor
        // or instance field initializer.
        propertyBlock = new MaterialPropertyBlock();

        animator = GetComponent<Animator>();

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
        if (ringRenderers == null)
            return;

        propertyBlock.SetFloat(MaterialPoweredHash, value);

        foreach (Renderer ringRenderer in ringRenderers)
        {
            if (ringRenderer == null)
                continue;

            // Only touch renderers that use the StasisRing shader/property.
            bool hasPoweredProperty = false;
            foreach (Material material in ringRenderer.sharedMaterials)
            {
                if (material != null && material.HasProperty(MaterialPoweredHash))
                {
                    hasPoweredProperty = true;
                    break;
                }
            }

            if (!hasPoweredProperty)
                continue;

            ringRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(MaterialPoweredHash, value);
            ringRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    /// <summary>
    /// Called by the parent Stasis Field group when its power state changes.
    /// Power allocation should be handled by the group/controller, not each tile.
    /// </summary>
    // Compatibility with TrapGroup.Configure(slowAmount, slowRefreshDuration).
    // TrapGroup supplies slowAmount as the fraction of speed to remove:
    // 0.5 means the enemy moves at 50% speed.
    public void Configure(float configuredSlowAmount, float configuredRefreshDuration)
    {
        slowMultiplier = 1f - Mathf.Clamp(configuredSlowAmount, 0f, 0.95f);
        slowApplicationDuration = Mathf.Max(0.1f, configuredRefreshDuration);
    }

    // Compatibility with TrapGroup.SetActive(bool).
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

        if (powered && enemyColliderCounts.Count > 0)
            StartSlowRoutine();
        else
            StopSlowRoutine();
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterEnemyCollider(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Safety net in case the tile becomes powered while an enemy is already inside.
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
            StartSlowRoutine();
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
        }

        UpdateEnemyDetected();

        if (enemyColliderCounts.Count == 0)
            StopSlowRoutine();
    }

    private void UpdateEnemyDetected()
    {
        animator.SetBool(EnemyDetectedHash, enemyColliderCounts.Count > 0);
    }

    private void StartSlowRoutine()
    {
        if (!powered || enemyColliderCounts.Count == 0 || slowRoutine != null)
            return;

        slowRoutine = StartCoroutine(RefreshSlowEffect());
    }

    private void StopSlowRoutine()
    {
        if (slowRoutine == null)
            return;

        StopCoroutine(slowRoutine);
        slowRoutine = null;
    }

    private IEnumerator RefreshSlowEffect()
    {
        while (powered && enemyColliderCounts.Count > 0)
        {
            // Remove destroyed enemies while iterating over a snapshot.
            List<EnemyMover> enemies = new List<EnemyMover>(enemyColliderCounts.Keys);

            foreach (EnemyMover enemy in enemies)
            {
                if (enemy == null)
                {
                    enemyColliderCounts.Remove(enemy);
                    continue;
                }

                enemy.ApplySlow(slowMultiplier, slowApplicationDuration);
            }

            UpdateEnemyDetected();

            if (enemyColliderCounts.Count == 0)
                break;

            yield return new WaitForSeconds(slowRefreshInterval);
        }

        slowRoutine = null;
    }

    private void OnDisable()
    {
        StopSlowRoutine();
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
