using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectBreach.Core;

namespace ProjectBreach.UI
{
    /// <summary>
    /// Rest-period countdown with an optional "start next wave now" button.
    ///
    /// GRASP: Controller -- the button does not decide anything itself, it asks
    /// WaveManager.RequestEarlyStart(), which owns the rules.
    ///
    /// Scene setup: drag the countdown text and the button in. Wire the button's
    /// OnClick to OnEarlyStartClicked on this component. Starts hidden; the panel
    /// only shows while a rest period is actually running.
    /// </summary>
    public class RestCountdownHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Button earlyStartButton;

        [Header("Text")]
        [Tooltip("{0} is the whole seconds remaining.")]
        [SerializeField] private string format = "Next wave in {0}s";

        [Tooltip("Shown when early start is switched off in WaveManager.")]
        [SerializeField] private string disabledText = "Preparing next wave...";

        private void OnEnable()
        {
            EventRegistry.OnRestCountdown += HandleRestCountdown;
        }

        private void OnDisable()
        {
            EventRegistry.OnRestCountdown -= HandleRestCountdown;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void Start()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (earlyStartButton != null)
            {
                earlyStartButton.onClick.AddListener(OnEarlyStartClicked);
            }
        }

        private void HandleRestCountdown(float remaining)
        {
            // Negative is the "rest period over" sentinel from WaveManager.
            if (remaining < 0f)
            {
                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }

                return;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (countdownText != null)
            {
                WaveManager wm = WaveManager.Instance;

                if (wm != null && !wm.EarlyStartEnabled)
                {
                    countdownText.text = disabledText;
                }
                else
                {
                    countdownText.text = string.Format(format, Mathf.CeilToInt(remaining));
                }
            }

            if (earlyStartButton != null)
            {
                earlyStartButton.interactable = WaveManager.Instance != null
                                               && WaveManager.Instance.EarlyStartEnabled;
            }
        }

        public void OnEarlyStartClicked()
        {
            // Null guard rather than a hard dependency: the HUD should degrade instead
            // of throwing if WaveManager has not been placed in the scene yet.
            if (WaveManager.Instance == null)
            {
                Debug.LogWarning("[RestCountdownHUD] WaveManager not found in scene.");
                return;
            }

            WaveManager.Instance.RequestEarlyStart();
        }
    }
}
