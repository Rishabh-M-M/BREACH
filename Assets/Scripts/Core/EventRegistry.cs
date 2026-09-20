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

        // TODO: Add back the OnRoomFallen event after finishing Room.cs in Step 7
        // public static event Action<Room> OnRoomFallen;
        // public static void BroadcastRoomFallen(Room room)
        //     => OnRoomFallen?.Invoke(room);
    }
}