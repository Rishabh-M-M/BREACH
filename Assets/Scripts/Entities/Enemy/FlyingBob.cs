using UnityEngine;

/// <summary>
/// Makes a visual child bob up and down in a sine wave as the enemy moves,
/// without affecting the parent's actual logical position — EnemyMover
/// relies on that staying flat for its raycasts and arrival checks. Attach
/// this to a visual-only child object, never to the root with the Rigidbody.
///
/// On death, stops bobbing and smoothly tilts the model forward instead.
/// </summary>
public class FlyingBob : MonoBehaviour
{
    [Header("Bob")]
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Death Tilt")]
    [Tooltip("Degrees to tilt forward (nose down) on death. Flip the sign if it tilts the wrong way for your model's orientation.")]
    [SerializeField] private float deathTiltAngle = 70f;
    [SerializeField] private float deathTiltSpeed = 90f;

    private Vector3 restLocalPosition;
    private float phaseOffset;
    private EnemyHealth health;
    private float currentTilt;

    private void Awake()
    {
        restLocalPosition = transform.localPosition;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        health = GetComponentInParent<EnemyHealth>();
    }

    private void LateUpdate()
    {
        bool isDead = health != null && health.IsDead;

        if (isDead)
        {
            transform.localPosition = restLocalPosition;

            currentTilt = Mathf.MoveTowards(currentTilt, deathTiltAngle, deathTiltSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(currentTilt, 0f, 0f);
            return;
        }

        float offset = Mathf.Sin((Time.time * bobSpeed) + phaseOffset) * bobAmplitude;
        transform.localPosition = restLocalPosition + new Vector3(0f, offset, 0f);
    }
}