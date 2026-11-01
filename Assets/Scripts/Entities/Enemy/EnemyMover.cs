using System.Collections;
using UnityEngine;
using ProjectBreach.Core;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyMover : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private bool isAntiGravity = false;

    [Space]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float maxSteerAngle = 45f;
    [SerializeField] private float arrivalDistance = 0.8f;
    [SerializeField] private Transform aimPoint;

    [Header("Shield")]
    [SerializeField] private LayerMask shieldLayer;
    [SerializeField] private float shieldDetectDistance = 5f;
    [SerializeField] private float shieldStopDistance = 2.5f;

    [Header("Stasis Field Pull")]
    [Tooltip("Current attraction origin, refreshed by StasisFieldTile while inside its trigger.")]
    [SerializeField] private float pullSpeed = 0f;

    public Transform AimPoint => aimPoint;

    private Rigidbody rb;
    private Animator animator;
    private EnemyTargetPoint claimedTarget;

    private Transform turretTarget;
    private Transform turretAimPoint;
    private Shield shieldTarget;

    private float _speedMultiplier = 1f;
    private Coroutine _slowRoutine;

    private int currentRoomIndex;
    private string currentRoomName;
    private Room spawnRoom;

    private Vector3 pullOrigin;
    private float pullTimeRemaining;

    public bool HasArrived { get; private set; }

    public int CurrentRoomIndex => currentRoomIndex;

    /// <summary>
    /// The room this enemy spawned in (strong reference). Preferred over
    /// index/name matching because scene roomIndex values are not unique and
    /// room names can be empty/duplicated.
    /// </summary>
    public Room SpawnRoom => spawnRoom;

    /// <summary>
    /// Name of the room this enemy spawned in. Kept for legacy consumers;
    /// prefer <see cref="SpawnRoom"/> for reliable attribution.
    /// </summary>
    public string CurrentRoomName => currentRoomName;

    private Vector3 UpDirection => isAntiGravity ? Vector3.down : Vector3.up;

    public void SetRoomIndex(int roomIndex)
    {
        currentRoomIndex = roomIndex;
    }

    public void SetRoomName(string roomName)
    {
        currentRoomName = roomName;
    }

    public void SetSpawnRoom(Room room)
    {
        spawnRoom = room;
        if (room != null)
        {
            currentRoomName = room.RoomName;
            currentRoomIndex = room.RoomIndex;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (pullTimeRemaining > 0f)
            pullTimeRemaining -= Time.fixedDeltaTime;

        if (shieldTarget != null && shieldTarget.IsDestroyed)
        {
            shieldTarget = null;
            target = turretTarget;
            aimPoint = turretAimPoint;

            HasArrived = false;
            animator.SetBool("HasArrived", false);
        }

        if (shieldTarget == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(
                transform.position + UpDirection * 0.5f,
                transform.forward,
                out hit,
                shieldDetectDistance,
                shieldLayer))
            {
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

        Vector3 pullStep = GetPullStep();

        if (target == null || HasArrived)
        {
            if (HasArrived)
                RotateTowardsAimPoint();

            animator.SetFloat("Steer", 0f);

            if (pullStep.sqrMagnitude > 0f)
                rb.MovePosition(rb.position + pullStep);

            return;
        }

        Vector3 toTarget = target.position - rb.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        float effectiveArrivalDistance = shieldTarget != null ? shieldStopDistance : arrivalDistance;

        if (distance <= effectiveArrivalDistance)
        {
            HasArrived = true;
            animator.SetBool("HasArrived", true);
            animator.SetFloat("Steer", 0f);

            if (pullStep.sqrMagnitude > 0f)
                rb.MovePosition(rb.position + pullStep);

            return;
        }

        Vector3 desiredDirection = toTarget.normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, desiredDirection, UpDirection);

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, UpDirection);
        Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);

        Vector3 forwardStep = transform.forward * (moveSpeed * _speedMultiplier) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardStep + pullStep);

        float clampedAngle = Mathf.Clamp(angleToTarget, -maxSteerAngle, maxSteerAngle);
        animator.SetFloat("Steer", clampedAngle / maxSteerAngle);
    }

    private Vector3 GetPullStep()
    {
        if (pullTimeRemaining <= 0f || pullSpeed <= 0f)
            return Vector3.zero;

        Vector3 direction = pullOrigin - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return direction.normalized * pullSpeed * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Apply an attraction towards the stasis field's centre.
    /// Called repeatedly by StasisFieldTile while the enemy is inside.
    /// </summary>
    public void ApplyPull(Vector3 origin, float speed, float duration)
    {
        if (speed <= 0f || duration <= 0f)
            return;

        pullOrigin = origin;
        pullSpeed = speed;
        pullTimeRemaining = Mathf.Max(pullTimeRemaining, duration);
    }

    /// <summary>
    /// Immediately cancels any active stasis-field pull.
    /// Called during the release phase or when an enemy leaves the field.
    /// </summary>
    public void ClearPull()
    {
        pullTimeRemaining = 0f;
        pullSpeed = 0f;
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

    // Retained for compatibility with other systems that may still apply slows.
    public void ApplySlow(float multiplier, float duration)
    {
        if (multiplier >= 1f || duration <= 0f) return;

        multiplier = Mathf.Clamp01(multiplier);

        if (_slowRoutine != null) StopCoroutine(_slowRoutine);

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

        Quaternion targetRotation = Quaternion.LookRotation(toAim.normalized, UpDirection);
        Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);
    }

    private void OnDestroy()
    {
        if (claimedTarget != null)
        {
            claimedTarget.Release();
        }
    }
}
