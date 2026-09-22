using UnityEngine;
using UnityEngine.UI;
using ProjectBreach.Core;
using TMPro;

namespace ProjectBreach.Core
{
    /// <summary>
    /// UIManager (trimmed): power bar display + message popup only.
    /// Camera switching is owned by the Player team's RoomCameraSwitcher.
    /// Per-turret toggling is handled by TurretToggleButton components on each button.
    /// Converted to Singleton so TurretToggleButton can call UIManager.Instance.ShowMessage.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Power UI")]
        [SerializeField] private Slider powerBar;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI availablePowerText;

        [Header("Message UI")]
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float messageDuration = 2f;

        /// <summary>
        /// Register the singleton instance. Must call base.Awake().
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            // Subscribe to power change events
            EventRegistry.OnPowerChanged += RefreshPowerBar;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            EventRegistry.OnPowerChanged -= RefreshPowerBar;
        }

        private void Start()
        {
            InitializePowerUI();
            HideMessage();
        }

        /// <summary>
        /// Initialize power bar with current PowerManager values.
        /// </summary>
        private void InitializePowerUI()
        {
            if (PowerManager.Instance != null)
            {
                RefreshPowerBar(PowerManager.Instance.CurrentLoad, PowerManager.Instance.MaxPower);
            }
            else
            {
                Debug.LogWarning("[UIManager] PowerManager not found!");
            }
        }

        /// <summary>
        /// Refresh power bar display (called when power changes).
        /// </summary>
        private void RefreshPowerBar(int load, int max)
        {
            if (powerBar != null)
            {
                powerBar.maxValue = max;
                powerBar.value = load;
            }

            if (powerText != null)
            {
                powerText.text = $"Power: {load}/{max}";
            }

            if (availablePowerText != null)
            {
                availablePowerText.text = $"Available: {max - load}";
            }
        }

        /// <summary>
        /// Show a message popup (T03: insufficient power).
        /// Public so TurretToggleButton and other UI components can reuse it.
        /// </summary>
        public void ShowMessage(string message)
        {
            if (messagePanel == null || messageText == null) return;

            messageText.text = message;
            messagePanel.SetActive(true);

            // Auto-hide after duration
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), messageDuration);
        }

        /// <summary>
        /// Hide the message popup.
        /// </summary>
        private void HideMessage()
        {
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Test helper: Right-click to test message popup.
        /// </summary>
        [ContextMenu("Test - Show Message")]
        private void TestShowMessage()
        {
            ShowMessage("Test message!");
        }
    }
}
