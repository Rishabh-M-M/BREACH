using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Safe controller for the rocket explosion prefab.
    ///
    /// IMPORTANT:
    /// This script deliberately does NOT modify ParticleSystem.main.duration,
    /// startLifetime, or startSpeed at runtime. The prefab's own particle
    /// settings control the explosion animation. This avoids Unity's warning
    /// when Play On Awake starts the particles before Configure() is called.
    ///
    /// Configure(radius, duration) keeps the damage radius available for gizmos
    /// and controls how long the spawned effect GameObject is allowed to exist.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RocketExplosionEffect : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showRadiusGizmo = true;
        [SerializeField] private Color radiusGizmoColor =
            new Color(0f, 0.85f, 1f, 0.9f);

        [Header("Defaults")]
        [SerializeField, Min(0.1f)] private float defaultRadius = 3f;
        [SerializeField, Min(0.1f)] private float defaultVisualDuration = 1.5f;

        private float explosionRadius;
        private float visualDuration;
        private bool configured;
        private bool cleanupScheduled;

        private void Awake()
        {
            explosionRadius = Mathf.Max(0.1f, defaultRadius);
            visualDuration = Mathf.Max(0.1f, defaultVisualDuration);
        }

        /// <summary>
        /// Called by Projectile.cs immediately after the effect prefab is spawned.
        /// Does not modify particle modules, so Play On Awake effects remain visible
        /// and Unity's "duration while system is still playing" warning is avoided.
        /// </summary>
        public void Configure(float radius, float duration)
        {
            explosionRadius = Mathf.Max(0.1f, radius);
            visualDuration = Mathf.Max(0.1f, duration);
            configured = true;

            if (!cleanupScheduled)
            {
                cleanupScheduled = true;
                Destroy(gameObject, visualDuration);
            }
        }

        private void OnDrawGizmos()
        {
            if (!showRadiusGizmo)
                return;

            float radius = configured
                ? explosionRadius
                : Mathf.Max(0.1f, defaultRadius);

            Gizmos.color = radiusGizmoColor;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
