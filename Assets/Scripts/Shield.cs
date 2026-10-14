using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Room shield. Enemy contact damages the shield, never the enemy.
/// Normal room shield break permanently disables the room.
/// Main room shield break triggers Game Over.
/// </summary>
public class Shield : MonoBehaviour, IDamageable
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
    [SerializeField] private Room assignedRoom;
    [SerializeField] private bool isMainRoomShield;
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

    // ============================================================
    // IDAMAGEABLE
    // ============================================================
    public int MaxHP => maxHealth;
    public int CurrentHP => currentHealth;
    public bool IsDead => destroyed;

    public void Die()
    {
        if (destroyed)
            return;

        currentHealth = 0;
        BreakShield();
    }

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Renderer r = GetComponent<Renderer>();
        if (r == null)
            r = GetComponentInChildren<Renderer>();

        if (r != null)
            shieldMaterial = r.material;

        shieldCollider = GetComponent<Collider>();
        if (shieldCollider == null)
            shieldCollider = GetComponentInChildren<Collider>();

        if (assignedRoom == null)
            assignedRoom = GetComponentInParent<Room>();

        if (mainControlRoom == null)
            mainControlRoom = FindObjectOfType<MainControlRoom>();

        destroyed = currentHealth <= 0;
        UpdateShieldVisual();

        if (destroyed)
            BreakShield();
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

        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null || enemy.IsDead)
            return;

        int id = enemy.GetInstanceID();

        if (nextDamageTime.TryGetValue(id, out float next) &&
            Time.time < next)
        {
            return;
        }

        nextDamageTime[id] =
            Time.time + Mathf.Max(0.05f, damageInterval);

        // Trigger breach visuals when the main shield is damaged.
        if (isMainRoomShield && mainControlRoom != null)
            mainControlRoom.NotifyMainShieldDamaged();

        // Only the shield takes damage.
        TakeDamage(damagePerHit);
    }

    public void TakeDamage(int damage)
    {
        if (destroyed || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateShieldVisual();

        if (shieldMaterial != null)
            StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            BreakShield();
    }

    private void UpdateShieldVisual()
    {
        if (shieldMaterial == null)
            return;

        float ratio =
            (float)currentHealth / Mathf.Max(1, maxHealth);

        if (shieldMaterial.HasProperty("_Health"))
            shieldMaterial.SetFloat("_Health", ratio);

        if (shieldMaterial.HasProperty("_Opacity"))
        {
            shieldMaterial.SetFloat(
                "_Opacity",
                destroyed ? 0f : baseOpacity
            );
        }
    }

    private void BreakShield()
    {
        if (breakHandled)
            return;

        breakHandled = true;
        destroyed = true;
        currentHealth = 0;

        UpdateShieldVisual();

        if (shieldCollider != null)
            shieldCollider.enabled = false;

        if (isMainRoomShield)
        {
            if (mainControlRoom != null)
            {
                mainControlRoom.MainShieldBroken();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterCoreDestroyed();
            }
            else
            {
                Debug.LogError(
                    "[Shield] Main shield broke, but no " +
                    "MainControlRoom/GameManager was found."
                );
            }
        }
        else
        {
            if (assignedRoom == null)
                assignedRoom = GetComponentInParent<Room>();

            if (assignedRoom != null)
            {
                assignedRoom.PermanentlyDisableRoom();

                Turret[] turrets =
                    assignedRoom.GetComponentsInChildren<Turret>(true);

                foreach (Turret turret in turrets)
                {
                    if (turret == null)
                        continue;

                    turret.TurnOff();
                }

                Debug.LogWarning(
                    $"[Shield] Room '{assignedRoom.RoomName}' " +
                    "has fallen. Its turrets are switched off."
                );
            }
            else
            {
                Debug.LogError(
                    $"[Shield] No Room assigned to {name}; " +
                    "room turrets were not disabled."
                );
            }
        }

        StartCoroutine(FadeOut());
    }

    private IEnumerator HitFlash()
    {
        if (shieldMaterial == null)
            yield break;

        if (shieldMaterial.HasProperty("_HitFlash"))
            shieldMaterial.SetFloat("_HitFlash", 1f);

        yield return new WaitForSeconds(0.12f);

        if (shieldMaterial != null &&
            shieldMaterial.HasProperty("_HitFlash"))
        {
            shieldMaterial.SetFloat("_HitFlash", 0f);
        }
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

            if (shieldMaterial.HasProperty("_Opacity"))
                shieldMaterial.SetFloat("_Opacity", opacity);

            yield return null;
        }

        gameObject.SetActive(false);
    }
}