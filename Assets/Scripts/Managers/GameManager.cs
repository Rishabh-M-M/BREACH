using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Global Controller (GRASP: Controller + Facade + Indirection)
    /// W2: Wired MainControlRoom to complete the Breach → Core damage → Game over chain.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public GameState State { get; private set; } = GameState.MainMenu;

        [Header("Boss Skills Master Switch (kill switch)")]
        [SerializeField] private bool isBossSkillsEnabled = true;

        [Header("References")]
        [SerializeField] private MainControlRoom mainControlRoom;  // W2: Main control room reference

        protected override void Awake()
        {
            base.Awake();  // Must be called!
        }

        public void StartGame()
        {
            State = GameState.Playing;
            EventRegistry.BroadcastGameStateChanged(State);
            Debug.Log("[GameManager] Game started");
        }

        public void EndGame(bool isWin)
        {
            State = isWin ? GameState.Victory : GameState.GameOver;
            EventRegistry.BroadcastGameStateChanged(State);
            Debug.Log($"[GameManager] Game ended: {(isWin ? "Victory" : "Defeat")}");
        }

        public void RegisterCoreDestroyed()
        {
            EndGame(isWin: false);
        }

        /// <summary>
        /// Reset the state machine to MainMenu. Called by GameOverScreen before
        /// loading the start scene, so the next run never inherits a stale
        /// GameOver/Victory state from the previous one.
        /// </summary>
        public void ReturnToMainMenu()
        {
            State = GameState.MainMenu;
            EventRegistry.BroadcastGameStateChanged(State);
            Debug.Log("[GameManager] Returned to main menu");
        }

        public void RequestEMP(int roomIndex, float duration)
        {
            if (!isBossSkillsEnabled) return;
            Debug.Log($"[GameManager] Boss EMP → Room {roomIndex}");
        }

        public void RequestJam(int roomIndex, float duration)
        {
            if (!isBossSkillsEnabled) return;
            Debug.Log($"[GameManager] Boss jam camera → Room {roomIndex}");
        }

        public void RequestSpawnAdds(int count)
        {
            if (!isBossSkillsEnabled) return;
            Debug.Log($"[GameManager] Boss spawns {count} adds");
        }

        /// <summary>
        /// Called when an enemy breaches (from Enemy.Breach).
        /// W2: Wired MainControlRoom.SpawnBreach to complete T05 acceptance.
        /// </summary>
        public void NotifyBreach(Enemy enemy)
        {
            Debug.Log("[GameManager] Breach occurred!");

            if (mainControlRoom != null)
            {
                mainControlRoom.SpawnBreach(enemy);  // W2: Removed TODO, wired
            }
            else
            {
                Debug.LogError("[GameManager] MainControlRoom not assigned! Please drag the MainControlRoom object in the Inspector.");
            }
        }
    }
}
