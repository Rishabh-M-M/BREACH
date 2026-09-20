using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProjectBreach.Core;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class SpikeTrap : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageInterval = 0.1f;

    [Header("Timing")]
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private float deactivationDelay = 0.3f;

    private Animator animator;
    private bool active;
    private bool canDamage;
    private Coroutine delayRoutine;

    // One damage coroutine per enemy
    private readonly HashSet<IDamageable> damagingTargets = new();

    // Lock the trap in place
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;

    private readonly int ActiveHash = Animator.StringToHash("Active");

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
        // Prevent the animation from moving the whole trap
        transform.position = lockedPosition;
        transform.rotation = lockedRotation;
    }

    public void SetActive(bool value)
    {
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
            delayRoutine = StartCoroutine(DisableDamageAfterDelay());
        }
    }

    private IEnumerator EnableDamageAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        canDamage = true;
    }

    private IEnumerator DisableDamageAfterDelay()
    {
        yield return new WaitForSeconds(deactivationDelay);
        canDamage = false;
        active = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!active || !canDamage) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null || target.IsDead) return;

        // Start one coroutine per enemy
        if (damagingTargets.Add(target))
        {
            StartCoroutine(DamageRoutine(target));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            damagingTargets.Remove(target);
        }
    }

    private IEnumerator DamageRoutine(IDamageable target)
    {
        while (active &&
               canDamage &&
               target != null &&
               !target.IsDead &&
               damagingTargets.Contains(target))
        {
            target.TakeDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }

        if (target != null)
            damagingTargets.Remove(target);
    }
}