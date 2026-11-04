using UnityEngine;
using ProjectBreach.Core;
using System.Collections;
using UnityEngine.SceneManagement;

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

        [Header("Ending Fallback (used only if the controller is missing or refuses)")]
        [SerializeField] private string defeatSceneName = "DefeatEnding";
        [SerializeField] private string victorySceneName = "VictoryEnding";

        protected override void Awake()
        {
            base.Awake();
        }

        public void StartGame()
        {
            // StartGame always begins a fresh run, so it never refuses.
            // The previous run's state is cleared when MainMenu loads.
            State = GameState.Playing;

            mainControlRoom = FindObjectOfType<MainControlRoom>();
            endingController = FindObjectOfType<EndingSequenceController>(true);

            EventRegistry.BroadcastGameStateChanged(State);

            Debug.Log(
                "[GameManager] New game started. Game state reset. " +
                $"EndingController: {(endingController != null ? endingController.name : "NOT FOUND")}"
            );
        }

        /// <summary>
        /// Returns true only if an ending was actually started.
        /// </summary>
        public bool EndGame(bool isWin)
        {
            if (State == GameState.Victory || State == GameState.GameOver)
            {
                Debug.LogWarning(
                    $"[GameManager] EndGame({(isWin ? "Victory" : "Defeat")}) ignored: " +
                    $"state is already {State}."
                );
                return false;
            }

            State = isWin ? GameState.Victory : GameState.GameOver;

            // A faulty or stale subscriber must never be able to block the ending.
            try
            {
                EventRegistry.BroadcastGameStateChanged(State);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            Debug.Log($"[GameManager] Game ended: {(isWin ? "Victory" : "Defeat")}");

            // Find the controller belonging to the current gameplay scene.
            if (endingController == null)
                endingController = FindObjectOfType<EndingSequenceController>(true);

            if (endingController != null && endingController.PlayEnding(isWin))
                return true;

            Debug.LogError(
                "[GameManager] EndingSequenceController missing or refused. " +
                "Loading the ending scene directly."
            );

            return LoadEndingSceneDirectly(isWin);
        }

        private bool LoadEndingSceneDirectly(bool isWin)
        {
            string sceneName = isWin ? victorySceneName : defeatSceneName;

            if (string.IsNullOrEmpty(sceneName) ||
                !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"[GameManager] Scene '{sceneName}' can't be loaded. " +
                    "Check the name and File > Build Settings."
                );
                return false;
            }

            Time.timeScale = 1f;
            AudioListener.pause = false;

            SceneManager.LoadScene(sceneName);
            return true;
        }

        public bool RegisterCoreDestroyed()
        {
            return EndGame(false);
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
            if (mainControlRoom == null)
                mainControlRoom = FindObjectOfType<MainControlRoom>();

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
            {
                ResetRunOnMainMenu();
            }
            else if (scene.name == "Gameplay")
            {
                StartCoroutine(InitialiseNewGame());
            }
        }

        private void ResetRunOnMainMenu()
        {
            Debug.Log("[GameManager] Main Menu loaded. Resetting previous run.");

            State = GameState.MainMenu;
            EventRegistry.BroadcastGameStateChanged(State);

            // Restore normal Unity settings.
            Time.timeScale = 1f;
            AudioListener.pause = false;

            // Reset skip-to-boss state.
            BossSkipIntroController.ResetSkipToBossRun();

            // Clear boss encounter if a manager still exists.
            if (ProjectBreach.Boss.BossJamManager.Instance != null)
            {
                ProjectBreach.Boss.BossJamManager.Instance.DeactivateBossEncounter();
            }

            // Clear references to the previous gameplay scene.
            mainControlRoom = null;
            endingController = null;

            Debug.Log("[GameManager] Previous run reset completed.");
        }

        private IEnumerator InitialiseNewGame()
        {
            // Wait one frame for the new scene to initialise.
            yield return null;

            mainControlRoom = FindObjectOfType<MainControlRoom>();
            endingController = FindObjectOfType<EndingSequenceController>(true);

            Debug.Log("[GameManager] MainControlRoom found: " + (mainControlRoom != null));
            Debug.Log("[GameManager] EndingController found: " + (endingController != null));

            Debug.Log(
                $"[GameManager] NEW RUN INITIALISED | " +
                $"MainControlRoom ID: {(mainControlRoom != null ? mainControlRoom.GetInstanceID() : -1)}"
            );
            StartGame();
        }
    }

}