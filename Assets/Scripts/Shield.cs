using UnityEngine;
using System.Collections;
using ProjectBreach.Core;

public class Shield : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Visual")]
    [Range(0f, 1f)]
    [SerializeField] private float baseOpacity = 0.08f;

    [SerializeField] private float fadeSpeed = 1.5f;

    [Header("Room")]
    [SerializeField] private Room ownerRoom;

    private Material shieldMaterial;
    private bool destroyed;

    // =========================================================
    // PUBLIC ACCESS
    // =========================================================

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public float HealthPercent =>
        maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;

    public bool IsDestroyed => destroyed;

    public Room OwnerRoom => ownerRoom;

    // =========================================================
    // INITIALISATION
    // =========================================================

    private void Awake()
    {
        // Automatically find the room if one wasn't assigned
        // manually in the Inspector.
        if (ownerRoom == null)
        {
            ownerRoom =
                GetComponentInParent<Room>();
        }

        MeshRenderer renderer =
            GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            shieldMaterial = renderer.material;
        }

        Restore();
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void Update()
    {
        // Temporary testing controls.
        //
        // K = deal 10 damage
        // R = restore shield

        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Restore();
        }
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (destroyed)
            return;

        if (damage <= 0)
            return;

        currentHealth =
            Mathf.Max(
                0,
                currentHealth - damage
            );

        UpdateHealthVisual();

        StopCoroutine(nameof(HitFlash));
        StartCoroutine(nameof(HitFlash));

        Debug.Log(
            $"[Shield] {currentHealth}/{maxHealth} HP"
        );

        if (currentHealth <= 0)
        {
            DestroyShield();
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void DestroyShield()
    {
        if (destroyed)
            return;

        destroyed = true;

        Debug.Log(
            "[Shield] Shield destroyed!"
        );

        Collider col =
            GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }

        // IMPORTANT:
        // Tell ONLY this shield's room that it has fallen.
        if (ownerRoom != null)
        {
            ownerRoom.SetFallen();
        }
        else
        {
            Debug.LogWarning(
                $"[Shield] {name} has no OwnerRoom. " +
                "Room structures cannot be disabled."
            );
        }

        StartCoroutine(FadeOut());
    }

    // =========================================================
    // RESTORE
    // =========================================================

    public void Restore()
    {
        StopAllCoroutines();

        destroyed = false;
        currentHealth = maxHealth;

        gameObject.SetActive(true);

        Collider col =
            GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = true;
        }

        UpdateHealthVisual();

        if (shieldMaterial != null)
        {
            shieldMaterial.SetFloat(
                "_Opacity",
                baseOpacity
            );

            shieldMaterial.SetFloat(
                "_HitFlash",
                0f
            );
        }

        // Restore the owning room's gameplay state.
        if (ownerRoom != null &&
            ownerRoom.IsFallen)
        {
            ownerRoom.RestoreRoom();
        }
    }

    // =========================================================
    // VISUAL HEALTH
    // =========================================================

    private void UpdateHealthVisual()
    {
        if (shieldMaterial == null)
            return;

        shieldMaterial.SetFloat(
            "_Health",
            HealthPercent
        );
    }

    // =========================================================
    // HIT FLASH
    // =========================================================

    private IEnumerator HitFlash()
    {
        if (shieldMaterial == null)
            yield break;

        shieldMaterial.SetFloat(
            "_HitFlash",
            1f
        );

        float t = 0f;

        while (t < 0.12f)
        {
            t += Time.deltaTime;

            shieldMaterial.SetFloat(
                "_HitFlash",
                1f - t / 0.12f
            );

            yield return null;
        }

        shieldMaterial.SetFloat(
            "_HitFlash",
            0f
        );
    }

    // =========================================================
    // FADE OUT
    // =========================================================

    private IEnumerator FadeOut()
    {
        if (shieldMaterial == null)
            yield break;

        float alpha = baseOpacity;

        while (alpha > 0f)
        {
            alpha -=
                Time.deltaTime *
                fadeSpeed;

            alpha =
                Mathf.Max(
                    0f,
                    alpha
                );

            shieldMaterial.SetFloat(
                "_Opacity",
                alpha
            );

            yield return null;
        }

        gameObject.SetActive(false);
    }
}