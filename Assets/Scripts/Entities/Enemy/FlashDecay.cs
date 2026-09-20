using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// FlashDecay — fades a Light's intensity to zero over a short duration.
    /// Used by the explosion prefab: the point light gives a bright impact flash on
    /// frame one, then decays so it does not linger while the particles finish.
    /// Cleanup is owned by the Particle System (Stop Action = Destroy), so this
    /// script never destroys anything itself.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class FlashDecay : MonoBehaviour
    {
        [Tooltip("Seconds for the light to fade from full intensity to zero.")]
        [SerializeField] private float decayTime = 0.3f;

        private Light flash;
        private float startIntensity;
        private float elapsed;

        private void Awake()
        {
            flash = GetComponent<Light>();
            startIntensity = flash.intensity;
        }

        private void Update()
        {
            // Normalized 0→1 progress of the fade.
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / decayTime);

            // Quadratic falloff reads more like a real flash than a linear fade:
            // hard punch up front, short dim tail.
            flash.intensity = Mathf.Lerp(startIntensity, 0f, t * t);
        }
    }
}