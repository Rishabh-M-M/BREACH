using System.Collections;
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Trap: a defensive structure that fires on contact, then has to cool down
    /// before it can fire again. Unlike a turret it cannot shoot on demand, so
    /// its whole value is the burst.
    ///
    /// Spec values (TeamDoc 6.3):
    ///   trigger radius 3m, damage 20, slow 50% for 2s, cooldown 8s,
    ///   power refunded after it triggers.
    ///
    /// GRASP: Information Expert -- the trap owns its own state machine, so it
    /// also owns the decision of when it is allowed to fire. Power accounting
    /// still belongs to PowerManager, so we only ask it to allocate/refund.
    ///
    /// Scene setup: put this on the trap object with a trigger collider sized to
    /// the intended blast radius. The collider is forced to isTrigger in Awake.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Trap : DefensiveStructure
    {
        [Header("Trap Settings (spec 6.3)")]
        [Tooltip("Blast radius. Every enemy inside this radius takes damage on trigger.")]
        [SerializeField] private float triggerRadius = 3f;

        [SerializeField] private int trapDamage = 20;

        [Range(0f, 1f)]
        [Tooltip("Fraction of the enemy's speed removed while slowed. 0.5 = half speed.")]
        [SerializeField] private float slowAmount = 0.5f;

        [SerializeField] private float slowDuration = 2f;

        [SerializeField] private float cooldownDuration = 8f;

        [Header("Target Filtering")]
        [Tooltip("Trigger and blast only affect these layers (Enemy). Without this the AOE " +
                 "damages friendly turrets standing inside the blast radius.")]
        [SerializeField] private LayerMask targetMask;

        [Header("Behaviour")]
        [Tooltip("Refund this trap's power cost to PowerManager after it triggers, " +
                 "then re-allocate when the cooldown ends. This is what makes the " +
                 "cooldown visible in the power budget instead of a silent timer.")]
        [SerializeField] private bool refundPowerOnTrigger = true;

        [Tooltip("Optional contact sound. Left null-safe on purpose: the project has " +
                 "no audio assets yet, so this must not be a hard dependency.")]
        [SerializeField] private AudioSource triggerSource;

        [SerializeField] private AudioClip triggerClip;

        private TrapState _state = TrapState.Offline;
        private bool _holdingPower;

        // The amount of operating power actually held. May be discounted by the
        // Armoury passive at arming time; refunds must return the same amount.
        private int _activePowerCost;

        public TrapState State => _state;
        public float TriggerRadius => triggerRadius;

        protected override void Awake()
        {
            base.Awake();

            Collider col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[Trap] {name} collider forced to isTrigger.");
            }

            // Safety net: a zero mask silently disables this trap (the layer gate rejects
            // everything) when the Enemy layer is not configured yet. Fall back to Everything;
            // the EnemyHealth guards below still block friendly fire in that case.
            if (targetMask.value == 0)
            {
                targetMask = ~0;
            }
        }

        private void Start()
        {
            // A trap that is switched on at scene load is armed immediately; anything
            // else waits for TurnOn.
            _state = IsActive ? TrapState.Armed : TrapState.Offline;
            BroadcastState();
        }

        public override void TurnOn()
        {
            if (_state == TrapState.Cooling)
            {
                Debug.Log($"[Trap] {name} is cooling down, cannot arm yet.");
                return;
            }

            // Allocate here so the books balance: the trigger refunds exactly what
            // arming took, and the cooldown routine re-takes it. Without this the
            // refund would inflate the budget out of thin air.
            if (refundPowerOnTrigger && !_holdingPower && PowerManager.Instance != null)
            {
                int cost = ArmouryPassive.ApplyDiscount(PowerCost);

                if (!PowerManager.Instance.TryAllocate(cost))
                {
                    UIManager.Instance?.ShowMessage("Not enough power!");
                    return;
                }

                _activePowerCost = cost;
                _holdingPower = true;
            }

            base.TurnOn();
            _state = TrapState.Armed;
            BroadcastState();
        }

        public override void TurnOff()
        {
            if (_holdingPower)
            {
                PowerManager.Instance?.Refund(_activePowerCost);
                _activePowerCost = 0;
                _holdingPower = false;
            }

            base.TurnOff();
            _state = TrapState.Offline;
            BroadcastState();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_state != TrapState.Armed || !IsActive)
            {
                return;
            }

            // Layer gate first: friendly structures inside the trigger volume must not set it off.
            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            // Friendly-fire guard independent of layer setup: only enemies (EnemyHealth in
            // the hierarchy) may set the trap off.
            if (other.GetComponentInParent<EnemyHealth>() == null)
            {
                return;
            }

            // Interfaces are not reliably resolved by the generic GetComponentInParent,
            // so go through the Type overload and cast.
            IDamageable triggerer = other.GetComponentInParent(typeof(IDamageable)) as IDamageable;
            if (triggerer == null || triggerer.IsDead)
            {
                return;
            }

            TriggerTrap();
        }

        /// <summary>
        /// Fire the blast: damage and slow everything in the radius, then cool down.
        /// </summary>
        private void TriggerTrap()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius, targetMask);
            int hitCount = 0;

            foreach (Collider hit in hits)
            {
                // Skip anything that is not an enemy (e.g. friendly turrets caught in the
                // blast), regardless of how the layer mask ended up configured.
                if (hit.GetComponentInParent<EnemyHealth>() == null)
                {
                    continue;
                }

                IDamageable target = hit.GetComponentInParent(typeof(IDamageable)) as IDamageable;
                if (target == null || target.IsDead)
                {
                    continue;
                }

                target.TakeDamage(trapDamage);

                if (hit.GetComponentInParent<EnemyMover>() != null)
                {
                    hit.GetComponentInParent<EnemyMover>()
                       .ApplySlow(1f - slowAmount, slowDuration);
                }

                hitCount++;
            }

            if (triggerSource != null && triggerClip != null)
            {
                triggerSource.PlayOneShot(triggerClip);
            }

            Debug.Log($"[Trap] {name} triggered, {hitCount} target(s) hit for {trapDamage}.");

            if (refundPowerOnTrigger && _holdingPower)
            {
                PowerManager.Instance?.Refund(_activePowerCost);
                _activePowerCost = 0;
                _holdingPower = false;
            }

            _state = TrapState.Cooling;
            BroadcastState();
            StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            yield return new WaitForSeconds(cooldownDuration);

            if (!IsActive)
            {
                // Switched off while cooling: stay offline, the player has to re-arm it.
                _state = TrapState.Offline;
                BroadcastState();
                yield break;
            }

            if (refundPowerOnTrigger)
            {
                int cost = ArmouryPassive.ApplyDiscount(PowerCost);

                if (PowerManager.Instance != null && !PowerManager.Instance.TryAllocate(cost))
                {
                    // Not enough headroom to re-arm right now. Retry until there is.
                    UIManager.Instance?.ShowMessage("Trap offline: not enough power.");
                    _state = TrapState.Offline;
                    BroadcastState();
                    yield break;
                }

                _activePowerCost = cost;
                _holdingPower = true;
            }

            _state = TrapState.Armed;
            BroadcastState();
        }

        private void BroadcastState()
        {
            int roomIndex = OwnerRoom != null ? OwnerRoom.RoomIndex : -1;
            EventRegistry.BroadcastTrapStateChanged(roomIndex, _state);
        }

        [ContextMenu("Test - Trigger Trap")]
        private void TestTrigger()
        {
            TriggerTrap();
        }
    }
}
