
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Base class for turret upgrade behaviour, information and power costs.
    /// </summary>
    public abstract class TurretUpgrade : MonoBehaviour
    {
        [Header("Power Costs")]
        public virtual int PermanentPowerCost => 0;
        public virtual int OperatingPowerCost => 0;

        // The UI reads this directly from the active upgrade component.
        public virtual string UpgradeInfo => string.Empty;

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