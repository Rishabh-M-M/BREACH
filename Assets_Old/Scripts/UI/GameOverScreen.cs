using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectBreach.Core;
using TMPro;

namespace ProjectBreach.Core
{
    /// <summary>
    /// GameOverScreen: end-of-run panel driven entirely by EventRegistry.
    /// Listens for GameState.GameOver / Victory, freezes gameplay (hit-stop),
    /// then pops the panel after a short real-time delay.
    ///
    /// Setup notes (read before wiring the scene):
    /// - Put this component on an ALWAYS-ACTIVE object (e.g. the Canvas root),
    ///   NOT on the panel itself. The panel starts inactive, and a component on
    ///   an inactive object never runs OnEnable, so it would never see the event.
    /// - Make the panel the LAST child of the Canvas so it renders on top and
    ///   its background Image blocks raycasts to the HUD buttons underneath.
    /// - Buttons are wired in code (Start), so no Inspector onClick setup is
    ///   needed - just drag the references into the fields below.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [Header("Panel References")]
        [Tooltip("Root panel object. Starts inactive; shown on game over or victory.")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Buttons (wired in code, no Inspector onClick needed)")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Copy")]
        [SerializeField] private string defeatTitle = "GAME OVER";
        [SerializeField] private string defeatSubtitle = "The Main Control Room has been destroyed.";
        [SerializeField] private string victoryTitle = "VICTORY";
        [SerializeField] private string victorySubtitle = "All waves repelled. The ship holds.";

        [Header("Flow")]
        [Tooltip("Seconds of frozen hit-stop before the panel pops. Real-time, because timeScale is already 0.")]
        [SerializeField] private float panelShowDelay = 0.75f;
        [Tooltip("Path of the start/menu scene exactly as listed in Build Settings.")]
        [SerializeField] private string mainMenuScenePath = "Assets/StartScene.unity";

        [Header("Behavior")]
        [SerializeField] private bool freezeTimeOnShow = true;
        [SerializeField] private bool pauseAudioOnShow = true;

        // True only while THIS screen has locked the flow, so RestoreFlow()
        // stays idempotent and never fights other systems over timeScale/cursor.
        private bool flowLocked;

        // Guards against double-clicks queueing two LoadScene calls in one frame.
        private bool sceneLoadRequested;

        // Stored so the cursor can be put back exactly as gameplay left it.
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;

        private Coroutine showRoutine;

        private void OnEnable()
        {
            // EventRegistry is static: without a matching unsubscribe in
            // OnDisable, a screen from a previous scene load would leak and
            // receive every broadcast twice.
            EventRegistry.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventRegistry.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            HidePanelInstant();

            if (gameOverPanel == null)
            {
                Debug.LogError("[GameOverScreen] gameOverPanel is not assigned. Drag the panel object into the Inspector.");
            }

            // Wiring clicks in code keeps scene setup to plain reference drags
            // and keeps the click behavior readable in one place.
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(Restart);
                // Keep the existing restart flow but present it as the requested retry action.
                TMP_Text restartLabel = restartButton.GetComponentInChildren<TMP_Text>(true);
                if (restartLabel != null) restartLabel.text = "TRY AGAIN";
            }
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.GameOver:
                    BeginEndFlow(isVictory: false);
                    break;
                case GameState.Victory:
                    BeginEndFlow(isVictory: true);
                    break;
                case GameState.Playing:
                    // Covers the Restart path: GameManager.StartGame() broadcasts
                    // Playing, so the screen tears itself down without the caller
                    // needing to know about panels, timeScale or cursor state.
                    HidePanelInstant();
                    RestoreFlow();
                    break;
                    // MainMenu / Paused / WaveClear are owned by other systems;
                    // this screen only reacts to end-of-run states.
            }
        }

        private void BeginEndFlow(bool isVictory)
        {
            // Ignore duplicate end broadcasts (e.g. a late breach landing in
            // the same frame as the killing blow).
            if (flowLocked) return;

            LockFlow();

            if (titleText != null) titleText.text = isVictory ? victoryTitle : defeatTitle;
            if (subtitleText != null) subtitleText.text = isVictory ? victorySubtitle : defeatSubtitle;

            if (panelShowDelay > 0f)
            {
                showRoutine = StartCoroutine(ShowPanelAfterDelay(panelShowDelay));
            }
            else
            {
                ShowPanelNow();
            }
        }

        private IEnumerator ShowPanelAfterDelay(float delay)
        {
            // Must be real-time: timeScale is already 0, so a scaled
            // WaitForSeconds would never complete and the panel would never pop.
            yield return new WaitForSecondsRealtime(delay);
            ShowPanelNow();
        }

        private void ShowPanelNow()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        private void HidePanelInstant()
        {
            if (showRoutine != null)
            {
                // A restart during the hit-stop window must cancel the pending
                // pop, otherwise the panel would appear inside the fresh scene.
                StopCoroutine(showRoutine);
                showRoutine = null;
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void LockFlow()
        {
            flowLocked = true;

            // Freezing timeScale stops enemy movement, spawner coroutines and
            // attack timers in one shot, because all of them run on scaled time.
            if (freezeTimeOnShow) Time.timeScale = 0f;

            // Mutes ongoing SFX underneath the end screen instead of letting
            // them ring out over the frozen frame.
            if (pauseAudioOnShow) AudioListener.pause = true;

            // The player controller may have locked the cursor for gameplay;
            // UI buttons need it free. Stored so Restart can put it back.
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void RestoreFlow()
        {
            if (!flowLocked) return;
            flowLocked = false;

            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }

        /// <summary>
        /// Restart button: reset the state machine, then reload the active scene.
        /// </summary>
        public void Restart()
        {
            if (sceneLoadRequested) return;

            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex < 0)
            {
                // Scenes missing from Build Settings cannot be loaded at runtime,
                // so fail loudly here instead of throwing mid-click.
                Debug.LogError("[GameOverScreen] Active scene is not in Build Settings. Add it via File > Build Settings.");
                return;
            }

            sceneLoadRequested = true;
            RestoreFlow();

            // Reset state so the fresh scene starts as Playing instead of
            // inheriting a stale GameOver from the DontDestroyOnLoad GameManager.
            if (GameManager.Instance != null) GameManager.Instance.StartGame();

            SceneManager.LoadScene(sceneIndex);
        }

        /// <summary>
        /// Main Menu button: reset the state machine, then load the start scene.
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (sceneLoadRequested) return;

            if (SceneUtility.GetBuildIndexByScenePath(mainMenuScenePath) < 0)
            {
                Debug.LogError($"[GameOverScreen] '{mainMenuScenePath}' is not in Build Settings. Add it via File > Build Settings.");
                return;
            }

            sceneLoadRequested = true;
            RestoreFlow();

            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();

            SceneManager.LoadScene(mainMenuScenePath);
        }

        /// <summary>
        /// Test helper: goes through GameManager (not the panel directly) so the
        /// test exercises the same event path as a real core destruction.
        /// </summary>
        [ContextMenu("Test - Trigger Game Over")]
        private void TestGameOver()
        {
            if (GameManager.Instance != null) GameManager.Instance.EndGame(false);
            else Debug.LogWarning("[GameOverScreen] No GameManager instance. Enter Play mode first.");
        }

        [ContextMenu("Test - Trigger Victory")]
        private void TestVictory()
        {
            if (GameManager.Instance != null) GameManager.Instance.EndGame(true);
            else Debug.LogWarning("[GameOverScreen] No GameManager instance. Enter Play mode first.");
        }
    }
}
