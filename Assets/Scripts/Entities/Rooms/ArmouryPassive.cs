using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Armoury room passive (design doc §2.1): while the room is online, all
    /// turret-upgrade power costs and trap-activation power costs are
    /// discounted by <see cref="DiscountFactor"/> (suggested ×0.85).
    ///
    /// Exposes a static query (<see cref="ApplyDiscount"/>) so the upgrade and
    /// trap flows read the current discount without coupling to a scene object.
    /// The discount auto-disables when the Armoury room falls, because
    /// <see cref="RoomPassive.IsEnabled"/> reflects the room state.
    /// </summary>
    public class ArmouryPassive : RoomPassive
    {
        [Header("Armoury Discount")]
        [Tooltip("Multiplier applied to power costs, e.g. 0.85 = 15% off.")]
        [Range(0.01f, 1f)]
        [SerializeField] private float discountFactor = 0.85f;

        private static ArmouryPassive _activeInstance;

        public float DiscountFactor => discountFactor;

        /// <summary>Whether the discount is currently in effect.</summary>
        public static bool IsDiscountActive =>
            _activeInstance != null && _activeInstance.IsEnabled;

        protected override void Awake()
        {
            base.Awake();
            Register();
        }

        protected override void Update()
        {
            // Static instance only stays authoritative while it is enabled.
            if (_activeInstance == this && !IsEnabled)
                _activeInstance = null;
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            if (_activeInstance == this)
                _activeInstance = null;
        }

        private void Register()
        {
            // Keep the first active ArmouryPassive as the discount source.
            if (_activeInstance == null)
                _activeInstance = this;
        }

        /// <summary>
        /// Applies the Armoury discount to a power cost. Returns the original
        /// cost unchanged when the Armoury room is offline or not present.
        /// </summary>
        public static int ApplyDiscount(int cost)
        {
            if (!IsDiscountActive || cost <= 0)
                return cost;

            return Mathf.Max(0, Mathf.RoundToInt(cost * _activeInstance.discountFactor));
        }
    }
}
