using UnityEngine;
using UnityEngine.UI;
using ProjectBreach.Core;
using TMPro;

namespace ProjectBreach.Core
{
    /// <summary>
    /// UI button that toggles one specific turret on/off.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TurretToggleButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Turret targetTurret;
        [SerializeField] private TextMeshProUGUI buttonText;

        [Header("Display Settings")]
        [SerializeField] private string turretDisplayName = "Turret";
        [SerializeField] private Color activeColor =
            new Color(0f, 0.8f, 1f);
        [SerializeField] private Color inactiveColor =
            Color.gray;

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
                Debug.LogWarning(
                    "[TurretToggleButton] No turret assigned!");

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
        /// Refresh button label and colour based on turret state.
        /// </summary>
        private void RefreshDisplay()
        {
            if (targetTurret == null)
                return;

            bool isOn = targetTurret.IsActive;

            if (buttonText != null)
            {
                buttonText.text =
                    $"{turretDisplayName}\n" +
                    $"{(isOn ? "ON" : "OFF")}";

                buttonText.color =
                    isOn
                        ? activeColor
                        : inactiveColor;
            }

            button.interactable = true;
        }
    }
}