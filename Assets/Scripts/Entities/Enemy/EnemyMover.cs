using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 90f;       // degrees per second
    [SerializeField] private float maxSteerAngle = 45f;    // matches your blend tree thresholds
    [SerializeField] private float arrivalDistance = 0.8f;
    [SerializeField] private Transform aimPoint;

    [Header("Shield")]
    [SerializeField] private LayerMask shieldLayer;
    [SerializeField] private float shieldDetectDistance = 5f;

    public Transform AimPoint => aimPoint;

    private Rigidbody rb;
    private Animator animator;
    private EnemyTargetPoint claimedTarget;

    private Transform turretTarget;
    private Transform turretAimPoint;
    private Shield shieldTarget;

    public bool HasArrived { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        // Continue to turret after shield is destroyed
        if (shieldTarget != null && shieldTarget.IsDestroyed)
        {
            shieldTarget = null;
            target = turretTarget;
            aimPoint = turretAimPoint;

            HasArrived = false;
            animator.SetBool("HasArrived", false);
        }

        // Detect shield directly in front
        if (shieldTarget == null)
        {
            RaycastHit hit;

            if (Physics.Raycast(
                transform.position + Vector3.up * 0.5f,
                transform.forward,
                out hit,
                shieldDetectDistance,
                shieldLayer))
            {
                // IMPORTANT: search parent too
                Shield shield = hit.collider.GetComponentInParent<Shield>();

                if (shield != null && !shield.IsDestroyed)
                {
                    shieldTarget = shield;
                    target = shield.transform;
                    aimPoint = shield.transform;

                    HasArrived = false;
                    animator.SetBool("HasArrived", false);
                }
            }
        }

        if (target == null || HasArrived)
        {
            if (HasArrived)
                RotateTowardsAimPoint();

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

        float angleToTarget =
            Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);

        Quaternion targetRotation =
            Quaternion.LookRotation(desiredDirection, Vector3.up);

        Quaternion newRotation =
            Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(newRotation);

        Vector3 newPosition =
            rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        float clampedAngle =
            Mathf.Clamp(angleToTarget, -maxSteerAngle, maxSteerAngle);

        animator.SetFloat("Steer", clampedAngle / maxSteerAngle);
    }

    public void SetTarget(Transform newTarget, EnemyTargetPoint targetPoint)
    {
        target = newTarget;
        turretTarget = newTarget;
        claimedTarget = targetPoint;
    }

    public void SetAimPoint(Transform newAimPoint)
    {
        aimPoint = newAimPoint;
        turretAimPoint = newAimPoint;
    }

    private void RotateTowardsAimPoint()
    {
        if (aimPoint == null) return;

        Vector3 toAim = aimPoint.position - rb.position;
        toAim.y = 0f;

        if (toAim.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(toAim.normalized, Vector3.up);

        Quaternion newRotation =
            Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(newRotation);
    }

    private void OnDestroy()
    {
        if (claimedTarget != null)
            claimedTarget.Release();
    }
}