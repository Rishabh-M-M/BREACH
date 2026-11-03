
using UnityEngine;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    public class GameManager : Singleton<GameManager>
    {
        public GameState State { get; private set; } = GameState.MainMenu;

        [Header("Boss Skills Master Switch")]
        [SerializeField] private bool isBossSkillsEnabled = true;

        [Header("References")]
        [SerializeField] private MainControlRoom mainControlRoom;

        [Header("Ending Sequences")]
        [SerializeField] private EndingSequenceController endingController;

        protected override void Awake()
        {
            base.Awake();
        }

        public void StartGame()
        {
            State = GameState.Playing;
            EventRegistry.BroadcastGameStateChanged(State);
            Debug.Log("[GameManager] Game started");
        }

        public void EndGame(bool isWin)
        {
            if (State == GameState.Victory || State == GameState.GameOver)
                return;

            State = isWin ? GameState.Victory : GameState.GameOver;

            EventRegistry.BroadcastGameStateChanged(State);

            Debug.Log($"[GameManager] Game ended: {(isWin ? "Victory" : "Defeat")}");

            if (endingController != null)
            {
                endingController.PlayEnding(isWin);
            }
            else
            {
                Debug.LogError(
                    "[GameManager] EndingSequenceController is not assigned!"
                );
            }
        }

        public void RegisterCoreDestroyed()
        {
            EndGame(false);
        }

        public void SetWaveClearState()
        {
            State = GameState.WaveClear;
            EventRegistry.BroadcastGameStateChanged(State);
            Debug.Log("[GameManager] Wave clear — rest period");
        }

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

        public void NotifyBreach(Enemy enemy)
        {
            Debug.Log("[GameManager] Breach occurred!");

            if (mainControlRoom != null)
            {
                mainControlRoom.SpawnBreach(enemy);
            }
            else
            {
                Debug.LogError(
                    "[GameManager] MainControlRoom not assigned! " +
                    "Please drag the MainControlRoom object in the Inspector."
                );
            }
        }
    }
}
