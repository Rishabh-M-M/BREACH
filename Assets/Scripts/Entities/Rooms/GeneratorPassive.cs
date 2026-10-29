using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Generator room passive (design doc §2.1): while the room is online,
    /// periodically raises the ship's maximum power capacity.
    ///
    /// Suggested values (design doc §2.2): every 10s, +5% of the current
    /// maxPower, with a +5 floor. Configurable in the Inspector.
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

            int current = pm.MaxPower;
            int amount = Mathf.Max(
                minGrowth,
                Mathf.RoundToInt(current * growthPercent));

            pm.IncreaseMaxPower(amount);
        }
    }
}
