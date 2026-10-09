
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;

public class Shield : MonoBehaviour
{
    [Header("Shield Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Enemy Damage")]
    [SerializeField] private int damagePerHit = 10;
    [SerializeField] private float damageInterval = 1f;

    [Header("Visual")]
    [Range(0f, 1f)]
    [SerializeField] private float baseOpacity = 0.08f;

    [SerializeField] private float fadeSpeed = 1.5f;

    [Header("Room Assignment")]
    [Tooltip("Assign the Room component for this shield's room. " +
             "If empty, the script searches parent objects.")]
    [SerializeField] private Room assignedRoom;

    [Tooltip("Assign this for the main room shield if MainControlRoom " +
             "is not on the same object or a parent.")]
    [SerializeField] private MainControlRoom mainControlRoom;

    private Material shieldMaterial;
    private Collider shieldCollider;
    private bool destroyed;
    private bool breakHandled;

    private readonly Dictionary<int, float> nextDamageTime =
        new Dictionary<int, float>();

    public bool IsDestroyed => destroyed;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Renderer shieldRenderer = GetComponent<Renderer>();

        if (shieldRenderer == null)
            shieldRenderer = GetComponentInChildren<Renderer>();

        if (shieldRenderer != null)
            shieldMaterial = shieldRenderer.material;

        shieldCollider = GetComponent<Collider>();

        if (shieldCollider == null)
            shieldCollider = GetComponentInChildren<Collider>();

        if (assignedRoom == null)
            assignedRoom = GetComponentInParent<Room>();

        if (mainControlRoom == null)
            mainControlRoom = GetComponentInParent<MainControlRoom>();

        UpdateShieldVisual();

        if (currentHealth <= 0)
        {
            BreakShield();
        }
        else
        {
            destroyed = false;
            breakHandled = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryDamageShield(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamageShield(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamageShield(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamageShield(other);
    }

    private void TryDamageShield(Collider other)
    {
        if (destroyed || other == null)
            return;

        // EnemyHealth is the existing enemy component in this project.
        // Resolve it from the collider's parent because enemy colliders
        // may be attached to child objects.
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null || enemy.IsDead)
            return;

        // Each enemy has its own attack cooldown.
        int enemyId = enemy.GetInstanceID();

        if (nextDamageTime.TryGetValue(enemyId, out float nextTime) &&
            Time.time < nextTime)
        {
            return;
        }

        nextDamageTime[enemyId] =
            Time.time + Mathf.Max(0.05f, damageInterval);

        // Damage the shield only. Enemy collision causes no damage
        // to the enemy itself.
        TakeDamage(damagePerHit);
    }

    public void TakeDamage(int damage)
    {
        if (destroyed || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        UpdateShieldVisual();

        if (shieldMaterial != null)
        {
            StopCoroutineIfRunning(nameof(HitFlash));
            StartCoroutine(HitFlash());
        }

        Debug.Log(
            $"[Shield] {gameObject.name}: " +
            $"{currentHealth}/{maxHealth} HP"
        );

        if (currentHealth <= 0)
            BreakShield();
    }

    private void UpdateShieldVisual()
    {
        if (shieldMaterial == null)
            return;

        float healthRatio =
            (float)currentHealth / Mathf.Max(1, maxHealth);

        shieldMaterial.SetFloat("_Health", healthRatio);
        shieldMaterial.SetFloat(
            "_Opacity",
            destroyed ? 0f : baseOpacity
        );
    }

    private void BreakShield()
    {
        if (breakHandled)
            return;

        breakHandled = true;
        destroyed = true;
        currentHealth = 0;

        UpdateShieldVisual();

        Debug.LogWarning(
            $"[Shield] {gameObject.name} has been destroyed!"
        );

        // Stop enemies from continuing to attack this shield.
        if (shieldCollider != null)
            shieldCollider.enabled = false;

        // Main room shield failure ends the game.
        if (mainControlRoom != null)
        {
            Debug.LogError(
                "[Shield] MAIN ROOM SHIELD BROKEN. GAME OVER."
            );

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterCoreDestroyed();
            else
                Debug.LogError(
                    "[Shield] GameManager.Instance is missing."
                );

            StartCoroutine(FadeOut());
            return;
        }

        // A normal room loses all its turrets permanently.
        DisableRoomTurrets();

        StartCoroutine(FadeOut());
    }

    private void DisableRoomTurrets()
    {
        if (assignedRoom == null)
            assignedRoom = GetComponentInParent<Room>();

        if (assignedRoom == null)
        {
            Debug.LogError(
                $"[Shield] No Room assigned to {gameObject.name}. " +
                "Assign the correct Room in the Inspector."
            );
            return;
        }

        // Include inactive turrets so none can remain operational.
        Turret[] turrets =
            assignedRoom.GetComponentsInChildren<Turret>(true);

        foreach (Turret turret in turrets)
        {
            if (turret == null)
                continue;

            // Turn off first so normal shutdown logic runs.
            turret.TurnOff();

            // Prevent normal Update-based behaviour from restarting it.
            turret.enabled = false;

            Debug.LogWarning(
                $"[Shield] Permanently disabled turret: {turret.name}"
            );
        }

        Debug.LogWarning(
            $"[Shield] Room '{assignedRoom.RoomName}' has fallen. " +
            "Its turrets are disabled."
        );
    }

    private IEnumerator HitFlash()
    {
        if (shieldMaterial == null)
            yield break;

        shieldMaterial.SetFloat("_HitFlash", 1f);

        float elapsed = 0f;
        const float flashDuration = 0.12f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;

            shieldMaterial.SetFloat(
                "_HitFlash",
                1f - Mathf.Clamp01(elapsed / flashDuration)
            );

            yield return null;
        }

        shieldMaterial.SetFloat("_HitFlash", 0f);
    }

    private IEnumerator FadeOut()
    {
        if (shieldMaterial == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        float opacity = baseOpacity;

        while (opacity > 0f)
        {
            opacity = Mathf.Max(
                0f,
                opacity - Time.deltaTime * fadeSpeed
            );

            shieldMaterial.SetFloat("_Opacity", opacity);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void StopCoroutineIfRunning(string coroutineName)
    {
        StopCoroutine(coroutineName);
    }

    // Deliberately no automatic shield restoration. Once broken,
    // a shield remains broken for the rest of the run.
}
