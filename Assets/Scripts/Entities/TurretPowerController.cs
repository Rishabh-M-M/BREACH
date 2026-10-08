using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Handles operating power for one individual turret.
    /// Permanent upgrade power remains owned by TurretUpgradeController.
    /// </summary>
    public class TurretPowerController : MonoBehaviour
    {
        [SerializeField] private Turret turret;

        private bool holdingPower;

        private void Awake()
        {
            if (turret == null)
            {
                turret = GetComponent<Turret>();
            }
        }

        private void Start()
        {
            Reconcile();
        }

        public void Reconcile()
        {
            if (turret == null || turret.IsDead)
                return;

            if (turret.IsActive && !holdingPower)
            {
                if (PowerManager.Instance == null)
                {
                    // Allows isolated turret testing without a PowerManager.
                    holdingPower = true;
                    return;
                }

                if (PowerManager.Instance.TryAllocate(turret.PowerCost))
                {
                    holdingPower = true;
                }
                else
                {
                    turret.TurnOff();
                    UIManager.Instance?.ShowMessage("Insufficient power!");
                }
            }
            else if (!turret.IsActive && holdingPower)
            {
                Release();
            }
        }

        public void Release()
        {
            if (!holdingPower)
                return;

            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.Refund(turret.PowerCost);
            }

            holdingPower = false;
        }

        public bool IsHoldingPower => holdingPower;
    }
}
