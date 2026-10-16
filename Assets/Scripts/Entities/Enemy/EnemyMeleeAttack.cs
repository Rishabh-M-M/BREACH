using UnityEngine;
using ProjectBreach.Core;

[RequireComponent(typeof(EnemyMover))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damagePerHit = 5;

    [Header("Dash Attack")]
    [SerializeField] private float dashDistance = 1.2f;
    [SerializeField] private float dashOutTime = 0.15f;
    [SerializeField] private float dashBackTime = 0.2f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Optional Bite Animation")]
    [Tooltip("Leave empty until the bite animation is wired into the Animator. If set, this trigger fires at the start of each dash.")]
    [SerializeField] private string biteTriggerName;

    private EnemyMover mover;
    private Rigidbody rb;
    private Animator animator;
    private EnemyHealth health;
    private EnemyAudio audioPlayer;

    private enum DashState { Idle, Out, Back, Cooldown }
    private DashState state = DashState.Idle;
    private float stateTimer;
    private Vector3 restPosition;
    private Vector3 dashTargetPosition;
    private bool hitAppliedThisDash;

    private void Awake()
    {
        mover = GetComponent<EnemyMover>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        audioPlayer = GetComponent<EnemyAudio>();
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            state = DashState.Idle;
            return;
        }

        Shield shield = GetCurrentShield();

        if (shield == null || shield.IsDead || !mover.HasArrived)
        {
            state = DashState.Idle;
            return;
        }

        switch (state)
        {
            case DashState.Idle:
                restPosition = rb.position;

                Vector3 direction = shield.transform.position - restPosition;
                direction.y = 0f;
                direction = direction.sqrMagnitude > 0.0001f
                    ? direction.normalized
                    : transform.forward;

                dashTargetPosition = restPosition + direction * dashDistance;
                hitAppliedThisDash = false;
                stateTimer = 0f;
                state = DashState.Out;

                if (!string.IsNullOrEmpty(biteTriggerName))
                {
                    animator.SetTrigger(biteTriggerName);
                }

                if (audioPlayer != null)
                {
                    audioPlayer.PlayAttack();
                }
                break;

            case DashState.Out:
                stateTimer += Time.fixedDeltaTime;
                float tOut = Mathf.Clamp01(stateTimer / dashOutTime);
                rb.MovePosition(Vector3.Lerp(restPosition, dashTargetPosition, tOut));

                if (!hitAppliedThisDash && tOut >= 1f)
                {
                    shield.TakeDamage(damagePerHit);
                    hitAppliedThisDash = true;
                }

                if (tOut >= 1f)
                {
                    stateTimer = 0f;
                    state = DashState.Back;
                }
                break;

            case DashState.Back:
                stateTimer += Time.fixedDeltaTime;
                float tBack = Mathf.Clamp01(stateTimer / dashBackTime);
                rb.MovePosition(Vector3.Lerp(dashTargetPosition, restPosition, tBack));

                if (tBack >= 1f)
                {
                    stateTimer = 0f;
                    state = DashState.Cooldown;
                }
                break;

            case DashState.Cooldown:
                stateTimer += Time.fixedDeltaTime;
                if (stateTimer >= attackCooldown)
                {
                    state = DashState.Idle;
                }
                break;
        }
    }

    private Shield GetCurrentShield()
    {
        Transform aim = mover.AimPoint;
        if (aim == null) return null;

        return aim.GetComponentInParent<Shield>();
    }
}