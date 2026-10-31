using UnityEngine;

/// <summary>
/// One tile of a Stasis Field group. Keep its trigger collider enabled;
/// TrapGroup toggles the effect, not the GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
public class StasisFieldTile : MonoBehaviour
{
    [Header("Effect (configured by TrapGroup)")]
    [Range(0f, 0.95f)] [SerializeField] private float slowAmount = 0.5f;
    [Min(0.05f)] [SerializeField] private float refreshDuration = 0.3f;
    [SerializeField] private bool startsActive;

    private Collider triggerCollider;
    private bool active;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
        active = startsActive;
    }

    public void Configure(float configuredSlowAmount, float configuredRefreshDuration)
    {
        slowAmount = Mathf.Clamp(configuredSlowAmount, 0f, 0.95f);
        refreshDuration = Mathf.Max(0.05f, configuredRefreshDuration);
    }

    public void SetActive(bool value)
    {
        active = value;
        // Keep the collider enabled so entering/exiting remains reliable.
        if (triggerCollider != null)
            triggerCollider.enabled = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!active) return;

        EnemyMover mover = other.GetComponentInParent<EnemyMover>();
        if (mover != null)
            mover.ApplySlow(1f - slowAmount, refreshDuration);
    }
}
