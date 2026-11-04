using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Generator room passive (design doc §2.1): while the room is online,
    /// periodically raises the ship's maximum power capacity.
    ///
    /// Suggested values (design doc §2.2): every 10s, +5% of the current
    /// maxPower, with a +5 floor. Configurable in the Inspector.
    ///
    /// This component is the single source of truth for the growth config.
    /// The HUD (UIManager) reads <see cref="IntervalSeconds"/>,
    /// <see cref="Progress"/> and <see cref="NextGrowthAmount"/> instead of
    /// keeping its own serialized copy, so the display can never drift away
    /// from the values that actually drive the growth.
    /// </summary>
    public class GeneratorPassive : RoomPassive
    {
        [Header("Generator Growth")]
        [Tooltip("Seconds between each maxPower increase.")]
        [SerializeField] private float intervalSeconds = 10f;

        [Tooltip("Fraction of current maxPower gained each tick, e.g. 0.05 = +5%.")]
        [Range(0f, 1f)]
        [SerializeField] private float growthPercent = 0.05f;

        [Tooltip("Minimum absolute increase, so tiny capacities still grow.")]
        [SerializeField] private int minGrowth = 5;

        private float _timer;

        /// <summary>
        /// Seconds between each growth tick. Exposed so the UI countdown
        /// always uses the real interval instead of a duplicated value that
        /// could be tuned differently.
        /// </summary>
        public float IntervalSeconds => intervalSeconds;

        /// <summary>
        /// Normalized progress (0..1) of the live tick timer. The UI binds
        /// its progress bar and countdown to this value so it stays in sync
        /// with the actual tick, including the reset when the room goes
        /// offline (<see cref="Update"/> zeroes the timer while disabled).
        /// </summary>
        public float Progress =>
            intervalSeconds > 0f ? Mathf.Clamp01(_timer / intervalSeconds) : 0f;

        /// <summary>
        /// The exact amount the next tick will grant. <see cref="ApplyGrowth"/>
        /// applies this same property, so the number the UI shows is
        /// guaranteed to match what the player actually receives.
        /// Falls back to <see cref="minGrowth"/> when no PowerManager exists
        /// (e.g. the main menu scene), which is the safest display value.
        /// </summary>
        public int NextGrowthAmount
        {
            get
            {
                PowerManager pm = PowerManager.Instance;
                if (pm == null)
                    return minGrowth;

                return Mathf.Max(
                    minGrowth,
                    Mathf.RoundToInt(pm.MaxPower * growthPercent));
            }
        }

        protected override void Update()
        {
            if (!IsEnabled)
            {
                _timer = 0f;
                return;
            }

            _timer += Time.deltaTime;

            if (_timer >= intervalSeconds)
            {
                _timer = 0f;
                ApplyGrowth();
            }
        }

        private void ApplyGrowth()
        {
            PowerManager pm = PowerManager.Instance;
            if (pm == null)
                return;

            // Grant exactly what NextGrowthAmount advertised. Computing the
            // amount in two places (once for the UI, once here) previously
            // allowed the display and the real growth to diverge whenever
            // the two config copies were tuned differently.
            pm.IncreaseMaxPower(NextGrowthAmount);
        }
    }
}