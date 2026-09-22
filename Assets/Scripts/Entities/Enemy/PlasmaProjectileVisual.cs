using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectBreach.Core
{
    /// <summary>
    /// PlasmaProjectileVisual — companion visual component for the W7 plasma shader.
    ///
    /// Attach this to the projectile prefab (it sits next to <see cref="Projectile"/>).
    /// It is responsible for three things:
    /// 1) Hit flash: <see cref="Projectile.HitTarget"/> calls <see cref="OnHit"/>, which pushes
    ///    the shader property _HitFlash to 1 through a MaterialPropertyBlock (the graph then
    ///    lerps the emission colour to HDR white), and delays destruction by flashDuration
    ///    seconds so the flash is actually visible for a few frames.
    /// 2) Pulse desync: Awake applies a random +/- jitter to _PulseSpeed, so dozens of
    ///    projectiles on screen do not "breathe" in perfect lockstep.
    /// 3) Optional trail: automatically adds and configures a TrailRenderer.
    ///
    /// Key design rule: everything goes through MaterialPropertyBlock and never touches
    /// renderer.material. A MaterialPropertyBlock overrides values for one renderer only,
    /// so no material instances are ever created (no memory leak) and every projectile keeps
    /// sharing a single material, which keeps the SRP Batcher batching.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class PlasmaProjectileVisual : MonoBehaviour
    {
        [Header("Hit Flash (MaterialPropertyBlock)")]
        [Tooltip("How long (in seconds) the hit flash stays visible before the projectile is destroyed.")]
        [SerializeField] private float flashDuration = 0.08f;

        [Header("Pulse Desync")]
        [SerializeField] private bool randomizePulse = true;
        [Tooltip("Random jitter applied to the pulse speed, as a +/- fraction of the base value.")]
        [SerializeField] [Range(0f, 0.9f)] private float pulseSpeedJitter = 0.25f;

        [Header("Trail (optional)")]
        [SerializeField] private bool addTrail = true;
        [SerializeField] private float trailTime = 0.25f;
        [SerializeField] private float trailStartWidth = 0.25f;
        [Tooltip("Trail material. Recommended: Legacy Shaders/Particles/Additive with a cyan tint - " +
                 "particle shaders multiply by the vertex colour, which lets the trail fade out via " +
                 "the TrailRenderer's alpha gradient. If left empty, the plasma material is reused " +
                 "and _FresnelPower is forced to 0 (otherwise the trail would be invisible).")]
        [SerializeField] private Material trailMaterial;

        // Cached shader property IDs - avoids re-hashing the property name strings on every call.
        private static readonly int HitFlashID = Shader.PropertyToID("_HitFlash");
        private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");
        private static readonly int FresnelPowerID = Shader.PropertyToID("_FresnelPower");

        private Renderer rend;
        private MaterialPropertyBlock mpb;
        private bool flashing;

        private void Awake()
        {
            rend = GetComponent<Renderer>();
            mpb = new MaterialPropertyBlock();

            if (randomizePulse)
            {
                // Read the shared material's default pulse speed as the base value...
                float baseSpeed = 3f;
                if (rend.sharedMaterial != null && rend.sharedMaterial.HasFloat(PulseSpeedID))
                {
                    baseSpeed = rend.sharedMaterial.GetFloat(PulseSpeedID);
                }
                // ...then override it per instance, so every projectile pulses at its own rate.
                mpb.SetFloat(PulseSpeedID,
                    baseSpeed * Random.Range(1f - pulseSpeedJitter, 1f + pulseSpeedJitter));
                rend.SetPropertyBlock(mpb);
            }

            if (addTrail && GetComponent<TrailRenderer>() == null)
            {
                SetupTrail();
            }
        }

        /// <summary>
        /// Builds and configures the trail at runtime, keeping the prefab itself minimal.
        /// </summary>
        private void SetupTrail()
        {
            var trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = trailTime;
            trail.startWidth = trailStartWidth;
            trail.endWidth = 0f;                 // taper: full width at the head, zero at the tail
            trail.minVertexDistance = 0.05f;
            trail.numCornerVertices = 2;
            trail.numCapVertices = 2;
            trail.alignment = LineAlignment.View;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;

            // Bright head -> transparent tail alpha gradient. Particle-type shaders multiply the
            // fragment colour by the vertex colour, so this gradient makes the tail fade out.
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0f, 0.71f, 1f), 0f),
                    new GradientColorKey(new Color(0f, 0.35f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            trail.colorGradient = gradient;

            if (trailMaterial != null)
            {
                trail.sharedMaterial = trailMaterial;
            }
            else
            {
                // Fallback: reuse the plasma material. Gotcha - a TrailRenderer's normals always
                // face the camera (N approximately equals V), so Fresnel = (1 - dot(N,V))^2.5
                // evaluates to ~0 and the whole trail would render black/invisible. Forcing
                // _FresnelPower to 0 via MPB makes pow(x, 0) = 1, so the trail glows uniformly.
                // The plasma shader does not read vertex colours, so with this fallback the tail
                // pops out instead of fading smoothly; assign trailMaterial for the smooth fade.
                trail.sharedMaterial = rend.sharedMaterial;
                var trailMpb = new MaterialPropertyBlock();
                trailMpb.SetFloat(FresnelPowerID, 0f);
                trail.SetPropertyBlock(trailMpb);
            }
        }

        /// <summary>
        /// Called by <see cref="Projectile.HitTarget"/> right before destruction:
        /// flashes the material white, then destroys the object after a short delay
        /// so the flash is actually visible.
        /// </summary>
        public void OnHit()
        {
            if (flashing) return;
            flashing = true;

            mpb.SetFloat(HitFlashID, 1f);
            rend.SetPropertyBlock(mpb);

            // Disable the collider during the flash window so the same projectile
            // cannot trigger the hit logic a second time.
            if (TryGetComponent(out Collider col))
            {
                col.enabled = false;
            }

            Destroy(gameObject, flashDuration);
        }
    }
}
