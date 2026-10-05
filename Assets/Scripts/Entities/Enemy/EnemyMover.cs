using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 90f;       // degrees per second
    [SerializeField] private float maxSteerAngle = 45f;    // matches your blend tree thresholds
    [SerializeField] private float arrivalDistance = 0.5f;
    [SerializeField] private Transform aimPoint;

    public Transform AimPoint => aimPoint;

    private Rigidbody rb;
    private Animator animator;
    private EnemyTargetPoint claimedTarget;

    // Slow effect support (traps). Kept as a multiplier on moveSpeed so the
    // serialized base speed is never permanently overwritten.
    private float _speedMultiplier = 1f;
    private Coroutine _slowRoutine;

    public bool HasArrived { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (target == null || HasArrived)
        {
            if (HasArrived)
            {
                RotateTowardsAimPoint();
            }
            animator.SetFloat("Steer", 0f);
            return;
        }

        Vector3 toTarget = target.position - rb.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance <= arrivalDistance)
        {
            HasArrived = true;
            animator.SetBool("HasArrived", true);
            animator.SetFloat("Steer", 0f);
            return;
        }

        Vector3 desiredDirection = toTarget.normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);

        Vector3 newPosition = rb.position + transform.forward * (moveSpeed * _speedMultiplier) * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        float clampedAngle = Mathf.Clamp(angleToTarget, -maxSteerAngle, maxSteerAngle);
        animator.SetFloat("Steer", clampedAngle / maxSteerAngle);
    }

    public void SetTarget(Transform newTarget, EnemyTargetPoint targetPoint)
    {
        target = newTarget;
        claimedTarget = targetPoint;
    }

    private void OnDestroy()
    {
        if (claimedTarget != null)
        {
            claimedTarget.Release();
        }
    }

    public void SetAimPoint(Transform newAimPoint)
    {
        aimPoint = newAimPoint;
    }

    /// <summary>
    /// Slow this mover for a duration. <paramref name="multiplier"/> is the speed
    /// fraction to keep, so 0.5 means half speed. The strongest active slow wins
    /// rather than stacking, otherwise two overlapping traps could stall an enemy
    /// completely and desync the waypoint arrival check.
    /// </summary>
    public void ApplySlow(float multiplier, float duration)
    {
        if (multiplier >= 1f || duration <= 0f)
        {
            return;
        }

        multiplier = Mathf.Clamp01(multiplier);

        if (_slowRoutine != null)
        {
            StopCoroutine(_slowRoutine);
        }

        _speedMultiplier = multiplier;
        _slowRoutine = StartCoroutine(SlowRoutine(duration));
    }

    private IEnumerator SlowRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        _speedMultiplier = 1f;
        _slowRoutine = null;
    }

    private void RotateTowardsAimPoint()
    {
        if (aimPoint == null) return;

        Vector3 toAim = aimPoint.position - rb.position;
        toAim.y = 0f;
        if (toAim.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(toAim.normalized, Vector3.up);
        Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);
    }
}
