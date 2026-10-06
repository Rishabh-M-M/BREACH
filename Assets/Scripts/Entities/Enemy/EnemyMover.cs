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
    [SerializeField] private float arrivalDistance = 0.8f; // 使用了 Combat 分支微调过的 0.8f 距离
    [SerializeField] private Transform aimPoint;

    [Header("Shield")]
    [SerializeField] private LayerMask shieldLayer;
    [SerializeField] private float shieldDetectDistance = 5f;

    public Transform AimPoint => aimPoint;

    private Rigidbody rb;
    private Animator animator;
    private EnemyTargetPoint claimedTarget;

    // 【Combat 护盾系统逻辑变量】
    private Transform turretTarget;
    private Transform turretAimPoint;
    private Shield shieldTarget;

    // 【HEAD 陷阱减速系统逻辑变量】
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
        // 【Combat】护盾被摧毁后，重新把目标设回炮塔/基地
        if (shieldTarget != null && shieldTarget.IsDestroyed)
        {
            shieldTarget = null;
            target = turretTarget;
            aimPoint = turretAimPoint;

            HasArrived = false;
            animator.SetBool("HasArrived", false);
        }

        // 【Combat】检测前方是否有护盾
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

        // 抵达判定
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

        // 移动与旋转计算
        Vector3 desiredDirection = toTarget.normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);

        // 【HEAD】此处合并了陷阱减速逻辑（_speedMultiplier）
        Vector3 newPosition = rb.position + transform.forward * (moveSpeed * _speedMultiplier) * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        float clampedAngle = Mathf.Clamp(angleToTarget, -maxSteerAngle, maxSteerAngle);
        animator.SetFloat("Steer", clampedAngle / maxSteerAngle);
    }

    public void SetTarget(Transform newTarget, EnemyTargetPoint targetPoint)
    {
        target = newTarget;
        turretTarget = newTarget; // 【Combat】保存炮塔目标作为备用
        claimedTarget = targetPoint;
    }

    public void SetAimPoint(Transform newAimPoint)
    {
        aimPoint = newAimPoint;
        turretAimPoint = newAimPoint; // 【Combat】保存炮塔瞄准点作为备用
    }

    // 【HEAD】陷阱减速核心方法
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

        Quaternion targetRotation = Quaternion.LookRotation(toAim.normalized, Vector3.up);
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