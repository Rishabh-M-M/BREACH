using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Visual")]
    [Range(0f, 1f)]
    [SerializeField] private float baseOpacity = 0.08f;

    [SerializeField] private float fadeSpeed = 1.5f;

    private Material shieldMaterial;
    private bool destroyed;

    public bool IsDestroyed => destroyed;

    private void Awake()
    {
        shieldMaterial = GetComponent<MeshRenderer>().material;
        Restore();
    }

    private void Update()
    {
        // Debug controls
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Restore();
        }
    }

    public void TakeDamage(int damage)
    {
        if (destroyed) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        float hp = (float)currentHealth / maxHealth;
        shieldMaterial.SetFloat("_Health", hp);

        StopCoroutine(nameof(HitFlash));
        StartCoroutine(nameof(HitFlash));

        if (currentHealth <= 0)
        {
            destroyed = true;
            GetComponent<Collider>().enabled = false;
            StartCoroutine(FadeOut());
        }
    }

    private void Restore()
    {
        StopAllCoroutines();

        destroyed = false;
        currentHealth = maxHealth;

        gameObject.SetActive(true);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        shieldMaterial.SetFloat("_Health", 1f);
        shieldMaterial.SetFloat("_Opacity", baseOpacity);
        shieldMaterial.SetFloat("_HitFlash", 0f);
    }

    IEnumerator HitFlash()
    {
        shieldMaterial.SetFloat("_HitFlash", 1f);

        float t = 0f;

        while (t < 0.12f)
        {
            t += Time.deltaTime;
            shieldMaterial.SetFloat("_HitFlash", 1f - t / 0.12f);
            yield return null;
        }

        shieldMaterial.SetFloat("_HitFlash", 0f);
    }

    IEnumerator FadeOut()
    {
        float alpha = baseOpacity;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            shieldMaterial.SetFloat("_Opacity", alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}