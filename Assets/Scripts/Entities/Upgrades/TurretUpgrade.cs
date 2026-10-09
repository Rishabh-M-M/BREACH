using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Base class for one turret upgrade's unique behaviour.
    /// Common turret behaviour stays in Turret.cs.
    /// Upgrade-specific behaviour belongs in the derived upgrade component.
    /// </summary>
    public abstract class TurretUpgrade : MonoBehaviour
    {
        public abstract int GetDamage(int baseDamage);

        public abstract float GetFireRate(float baseFireRate);

        public virtual float GetAttackRange(float baseRange)
        {
            return baseRange;
        }

        public abstract bool TryFire(
            Turret turret,
            IDamageable target,
            int projectileDamage
        );

        public virtual Transform GetTurretHead(Transform baseHead)
        {
            return baseHead;
        }

        public virtual Transform GetFirePoint(Transform baseFirePoint)
        {
            return baseFirePoint;
        }

        public virtual bool PrefersFurthestTarget()
        {
            return false;
        }

        public virtual float GetRecoilDistance(float baseDistance)
        {
            return baseDistance;
        }

        public virtual float GetRecoilRecoverySpeed(float baseRecoverySpeed)
        {
            return baseRecoverySpeed;
        }

        public virtual Vector3 GetRecoilLocalDirection(Vector3 baseDirection)
        {
            return baseDirection;
        }

        public virtual AudioClip GetFireSound()
        {
            return null;
        }

        // Rotation audio is shared by all turret variants and is handled by Turret.cs.
        // Kept for compatibility with existing upgrade components.
        public virtual AudioClip GetRotationSound()
        {
            return null;
        }

        public virtual void SetVisualActive(bool active)
        {
        }

        public virtual void DrawGizmos(Turret turret)
        {
        }
    }
}
