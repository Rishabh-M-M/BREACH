using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Shield Reactor room passive (design doc §2.1): while the room is online,
    /// regenerates HP on every ship barrier each tick. Destroyed barriers are
    /// never restored.
    ///
    /// Suggested values (design doc §2.2): every 1s, +2 HP per barrier
    /// (each barrier independent).
    /// </summary>
    public class ShieldReactorPassive : RoomPassive
    {
        [Header("Shield Regen")]
        [Tooltip("Seconds between each regen tick.")]
        [SerializeField] private float intervalSeconds = 1f;

        [Tooltip("HP restored to each non-destroyed barrier per tick.")]
        [SerializeField] private int healAmount = 2;

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
                RegenerateShipShields();
            }
        }

        private void RegenerateShipShields()
        {
            foreach (Shield shield in FindObjectsByType<Shield>(FindObjectsSortMode.None))
            {
                if (shield == null || shield.IsDestroyed)
                    continue;

                shield.Heal(healAmount);
            }
        }
    }
}
