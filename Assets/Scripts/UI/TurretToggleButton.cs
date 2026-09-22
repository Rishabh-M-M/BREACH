using UnityEngine;
using UnityEngine.UI;
using ProjectBreach.Core;
using TMPro;

namespace ProjectBreach.Core
{
    /// <summary>
    /// TurretToggleButton: UI button that toggles one specific turret on/off.
    /// Turning on allocates power; turning off refunds it.
    /// Attach one per turret slot button in the room status panel
    /// (e.g. Turret 1 and Turret 2 buttons in the Composition scene).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TurretToggleButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Turret targetTurret;          // the turret this button controls
        [SerializeField] private TextMeshProUGUI buttonText;   // optional label for state display

        [Header("Display Settings")]
        [SerializeField] private string turretDisplayName = "Turret";
        [SerializeField] private Color activeColor = new Color(0f, 0.8f, 1f);  // cyan accent when active
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("Power Settings")]
        [SerializeField] private bool usePowerSystem = false;   // 暂时关闭电力依赖，等 PowerBar UI 做好再打开

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            button.onClick.AddListener(OnToggleClicked);
            RefreshDisplay();
        }

        private void OnToggleClicked()
        {
            if (targetTurret == null)
            {
                Debug.LogWarning("[TurretToggleButton] No turret assigned!");
                return;
            }

            // Destroyed turrets cannot be toggled
            if (targetTurret.IsDead)
            {
                RefreshDisplay();
                return;
            }

            if (targetTurret.IsActive)
            {
                TurnOffTurret();
            }
            else
            {
                TryTurnOnTurret();
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Try to allocate power and activate the turret.
        /// Shows "insufficient power" feedback if allocation fails.
        /// </summary>
        private void TryTurnOnTurret()
        {
            if (usePowerSystem)
            {
                if (PowerManager.Instance == null)
                {
                    Debug.LogError("[TurretToggleButton] PowerManager not found!");
                    return;
                }

                bool success = PowerManager.Instance.TryAllocate(targetTurret.PowerCost);
                if (!success)
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowMessage("Insufficient power!");
                    }
                    Debug.Log("[TurretToggleButton] Insufficient power to activate turret");
                    return;
                }
            }

            targetTurret.TurnOn();
            Debug.Log($"[TurretToggleButton] {turretDisplayName} activated (powerSystem={usePowerSystem})");
        }

        /// <summary>
        /// Deactivate the turret and refund its power cost.
        /// </summary>
        private void TurnOffTurret()
        {
            targetTurret.TurnOff();

            if (usePowerSystem && PowerManager.Instance != null)
            {
                PowerManager.Instance.Refund(targetTurret.PowerCost);
            }

            Debug.Log($"[TurretToggleButton] {turretDisplayName} deactivated");
        }

        /// <summary>
        /// Refresh button label and color based on turret state.
        /// </summary>
        private void RefreshDisplay()
        {
            if (targetTurret == null) return;

            if (targetTurret.IsDead)
            {
                if (buttonText != null)
                {
                    buttonText.text = $"{turretDisplayName}\nDESTROYED";
                    buttonText.color = Color.red;
                }
                button.interactable = false;
                return;
            }

            bool isOn = targetTurret.IsActive;
            if (buttonText != null)
            {
                buttonText.text = $"{turretDisplayName}\n{(isOn ? "ON" : "OFF")}";
                buttonText.color = isOn ? activeColor : inactiveColor;
            }
        }
    }
}
