using System;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Global event center. Publishers call Broadcast; subscribers use += / -=.
    /// </summary>
    public static class EventRegistry
    {
        public static event Action<int, int> OnPowerChanged;
        public static void BroadcastPowerChanged(int load, int max)
            => OnPowerChanged?.Invoke(load, max);

        public static event Action<int> OnCameraSwitched;
        public static void BroadcastCameraSwitched(int roomIndex)
            => OnCameraSwitched?.Invoke(roomIndex);

        public static event Action<int> OnWaveStart;
        public static void BroadcastWaveStart(int waveIndex)
            => OnWaveStart?.Invoke(waveIndex);

        public static event Action OnAllWavesCleared;
        public static void BroadcastAllWavesCleared()
            => OnAllWavesCleared?.Invoke();

        public static event Action<GameState> OnGameStateChanged;
        public static void BroadcastGameStateChanged(GameState state)
            => OnGameStateChanged?.Invoke(state);

        public static event Action OnEnemyResolved;
        public static void BroadcastOnEnemyResolved()
            => OnEnemyResolved?.Invoke();

        // Fired when a room flips to Fallen. Room.SetFallen is the only publisher.
        public static event Action<Room> OnRoomFallen;
        public static void BroadcastRoomFallen(Room room)
            => OnRoomFallen?.Invoke(room);

        // Fired on an actual kill only. OnEnemyResolved also counts breaches, so
        // the upgrade economy must not reuse it.
        public static event Action OnEnemyKilled;
        public static void BroadcastEnemyKilled()
            => OnEnemyKilled?.Invoke();

        public static event Action<int, int> OnCoreHPChanged;
        public static void BroadcastCoreHPChanged(int current, int max)
            => OnCoreHPChanged?.Invoke(current, max);

        public static event Action<int, TrapState> OnTrapStateChanged;
        public static void BroadcastTrapStateChanged(int roomIndex, TrapState state)
            => OnTrapStateChanged?.Invoke(roomIndex, state);

        // Rest-period countdown. Remaining seconds are passed on every tick, and a
        // negative value marks "no rest period active" so HUDs can hide themselves.
        public static event Action<float> OnRestCountdown;
        public static void BroadcastRestCountdown(float remaining)
            => OnRestCountdown?.Invoke(remaining);

        public static event Action<int> OnUpgradePointsChanged;
        public static void BroadcastUpgradePointsChanged(int points)
            => OnUpgradePointsChanged?.Invoke(points);
    }
}
