using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProjectBreach.Core;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class SpikeTrap : MonoBehaviour
{
    [Header("Combat (can be overridden by TrapGroup)")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageInterval = 0.1f;

    [Header("Timing")]
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private float deactivationDelay = 0.3f;

    private Animator animator;
    private bool active;
    private bool canDamage;
    private Coroutine delayRoutine;
    private readonly HashSet<IDamageable> damagingTargets = new HashSet<IDamageable>();
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private static readonly int ActiveHash = Animator.StringToHash("Active");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        animator.SetBool(ActiveHash, false);
        GetComponent<Collider>().isTrigger = true;
        lockedPosition = transform.position;
        lockedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.position = lockedPosition;
        transform.rotation = lockedRotation;
    }

    public void SetCombatSettings(int damagePerTick, float interval)
    {
        damage = Mathf.Max(0, damagePerTick);
        damageInterval = Mathf.Max(0.02f, interval);
    }

    public void SetActive(bool value)
    {
        if (animator == null) animator = GetComponent<Animator>();
        animator.SetBool(ActiveHash, value);

        if (delayRoutine != null)
            StopCoroutine(delayRoutine);

        if (value)
        {
            active = true;
            canDamage = false;
            delayRoutine = StartCoroutine(EnableDamageAfterDelay());
        }
        else
        {
            // Stop damage immediately; visual animation can finish its delay.
            canDamage = false;
            damagingTargets.Clear();
            delayRoutine = StartCoroutine(DisableDamageAfterDelay());
        }
    }

    private IEnumerator EnableDamageAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        canDamage = true;
        delayRoutine = null;
    }

    private IEnumerator DisableDamageAfterDelay()
    {
        yield return new WaitForSeconds(deactivationDelay);
        active = false;
        delayRoutine = null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!active || !canDamage || damage <= 0) return;

        IDamageable target = other.GetComponentInParent(typeof(IDamageable)) as IDamageable;
        if (target == null || target.IsDead) return;

        if (damagingTargets.Add(target))
            StartCoroutine(DamageRoutine(target));
    }

    private void OnTriggerExit(Collider other)
    {
        IDamageable target = other.GetComponentInParent(typeof(IDamageable)) as IDamageable;
        if (target != null)
            damagingTargets.Remove(target);
    }

    private IEnumerator DamageRoutine(IDamageable target)
    {
        while (active && canDamage && target != null &&
               !target.IsDead && damagingTargets.Contains(target))
        {
            target.TakeDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }

        if (target != null)
            damagingTargets.Remove(target);
    }

    private void OnDisable()
    {
        if (delayRoutine != null)
            StopCoroutine(delayRoutine);

        delayRoutine = null;
        active = false;
        canDamage = false;
        damagingTargets.Clear();

        if (animator != null)
            animator.SetBool(ActiveHash, false);
    }
}
