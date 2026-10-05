using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectBreach.Core;

namespace ProjectBreach.UI
{
    /// <summary>
    /// Main menu entry point: Start / Options (volume) / Quit.
    ///
    /// GRASP: Controller -- scene transitions are decided here and nowhere else.
    ///
    /// Scene setup: put this on a root object in the menu scene and wire the button
    /// OnClick slots to the public methods below, or use the buttons assigned here
    /// (they are wired in Start). The target scene MUST be in Build Settings or
    /// LoadScene will fail, which is why the build index is checked first.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("Scene")]
        [Tooltip("Asset path of the scene to load on Start. Must be in Build Settings.")]
        [SerializeField] private string gameScenePath = "Assets/Scenes/Compose.unity";

        [Header("Buttons (optional, wire OnClick yourself if left empty)")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        [Header("Options panel")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Tooltip("Hide the menu root when the options panel opens.")]
        [SerializeField] private bool hideMenuWhenOptionsOpen = true;

        [Tooltip("Quit is a no-op in a WebGL build -- the browser tab cannot be closed " +
                 "from script. Kept so the button still does something sensible in the editor.")]
        [SerializeField] private bool quitButtonEnabled = true;

        private void Start()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(OnOptionsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }

            // Seed the slider FROM the manager (was: the other way round, which reset
            // the live volume to the slider default on every menu open).
            if (AudioManager.Instance != null && masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
            }
        }

        // ========== Public entries for Button.OnClick ==========

        public void OnStartClicked()
        {
            int index = SceneUtility.GetBuildIndexByScenePath(gameScenePath);

            if (index < 0)
            {
                Debug.LogError($"[MainMenu] '{gameScenePath}' is not in Build Settings.");
                return;
            }

            SceneManager.LoadScene(index);
        }

        public void OnOptionsClicked()
        {
            if (optionsPanel == null)
            {
                return;
            }

            bool open = !optionsPanel.activeSelf;
            optionsPanel.SetActive(open);

            if (hideMenuWhenOptionsOpen)
            {
                if (startButton != null)
                {
                    startButton.gameObject.SetActive(!open);
                }

                if (quitButton != null)
                {
                    quitButton.gameObject.SetActive(!open);
                }
            }
        }

        public void OnQuitClicked()
        {
            if (!quitButtonEnabled)
            {
                return;
            }

            Application.Quit();
            // In the editor the play mode just keeps running, so say so rather than
            // looking like the button is broken.
            Debug.Log("[MainMenu] Quit requested (no-op in editor / WebGL).");
        }

        public void OnMasterVolumeChanged(float value)
        {
            AudioManager.Instance?.SetMasterVolume(value);
        }

        public void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance?.SetMusicVolume(value);
        }

        public void OnSfxVolumeChanged(float value)
        {
            AudioManager.Instance?.SetSfxVolume(value);
        }
    }
}
