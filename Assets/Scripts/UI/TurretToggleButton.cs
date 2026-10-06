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
        [SerializeField] private bool usePowerSystem = false;   // DEPRECATED: power accounting now lives in Turret itself (per-frame reconciliation). Kept only for scene serialization compatibility.

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
        /// Activate the turret. Power accounting is owned by the turret itself (it
        /// reconciles IsActive with the ledger every frame), so the button only
        /// toggles -- an insufficient-power refusal is shown by the turret.
        /// </summary>
        private void TryTurnOnTurret()
        {
            targetTurret.TurnOn();
            Debug.Log($"[TurretToggleButton] {turretDisplayName} activate requested");
        }

        /// <summary>
        /// Deactivate the turret. The turret refunds its power itself.
        /// </summary>
        private void TurnOffTurret()
        {
            targetTurret.TurnOff();
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
