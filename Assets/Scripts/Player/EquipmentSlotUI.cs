using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectBreach.Core;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI slotText;

    [Header("Turret")]
    [SerializeField] private Turret targetTurret;

    [Header("Display")]
    [SerializeField] private string equipmentName = "TURRET 1";

    [Header("Button Colour Tint")]
    [SerializeField] private Color normalColour =
        new Color(0.051f, 0.110f, 0.169f);

    [SerializeField] private Color highlightedColour =
        new Color(0.094f, 0.212f, 0.302f);

    [SerializeField] private Color pressedColour =
        new Color(0f, 0.75f, 1f);

    [SerializeField] private Color selectedColour =
        new Color(0.059f, 0.259f, 0.388f);

    [SerializeField] private Color disabledColour =
        new Color(0.149f, 0.149f, 0.169f);

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        ApplyButtonColours();
    }

    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnClicked);

        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void ApplyButtonColours()
    {
        if (button == null)
            return;

        ColorBlock colours = button.colors;

        colours.normalColor = normalColour;
        colours.highlightedColor = highlightedColour;
        colours.pressedColor = pressedColour;
        colours.selectedColor = selectedColour;
        colours.disabledColor = disabledColour;

        colours.colorMultiplier = 1f;
        colours.fadeDuration = 0.1f;

        button.colors = colours;
    }

    private void OnClicked()
    {
        if (targetTurret == null)
            return;

<<<<<<< Updated upstream
        // =========================
=======
        // Destroyed turret cannot be used.
        if (targetTurret.IsDead)
            return;

        // ============================================================
>>>>>>> Stashed changes
        // TURN OFF
        // ============================================================

        if (targetTurret.IsActive)
        {
            // IMPORTANT:
            // Turret.cs is responsible for returning its own operating
            // power allocation through ReconcilePowerState().
            //
            // Do NOT call PowerManager.Refund() here.
            targetTurret.TurnOff();
<<<<<<< Updated upstream

            if (PowerManager.Instance != null)
            {
                PowerManager.Instance.Refund(
                    targetTurret.PowerCost);
            }
=======
>>>>>>> Stashed changes
        }

        // ============================================================
        // TURN ON
        // ============================================================

        else
        {
<<<<<<< Updated upstream
            if (PowerManager.Instance == null)
            {
                Debug.LogError(
                    "[EquipmentSlotUI] PowerManager not found!");

                return;
            }

            bool success =
                PowerManager.Instance.TryAllocate(
                    targetTurret.PowerCost);

            if (!success)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage(
                        "Insufficient power!");
                }

                return;
            }

=======
            // Turret.cs is responsible for requesting its own operating
            // power allocation through ReconcilePowerState().
            //
            // Do NOT call PowerManager.TryAllocate() here.
>>>>>>> Stashed changes
            targetTurret.TurnOn();
        }

        Refresh();

        // Return the button to its normal visual state.
        if (button != null)
            button.OnDeselect(null);
    }

    public void Refresh()
    {
        if (button == null)
            return;

        // =========================
        // EMPTY
        // =========================

        if (targetTurret == null)
        {
            button.interactable = false;

            if (slotText != null)
            {
                slotText.text =
                    $"<color=#B0C4DE>{equipmentName}</color>\n" +
                    "<color=#777777>— EMPTY —</color>";
            }

            return;
        }

<<<<<<< Updated upstream
        // =========================
        // ACTIVE
        // =========================
=======
        // ============================================================
        // DESTROYED
        // ============================================================

        if (targetTurret.IsDead)
        {
            button.interactable = false;

            if (slotText != null)
            {
                slotText.text =
                    $"<color=#B0C4DE>{equipmentName}</color>\n" +
                    "<color=#FF3333>✕ DESTROYED</color>";
            }

            return;
        }
>>>>>>> Stashed changes

        // ============================================================
        // ACTIVE
        // ============================================================

        if (targetTurret.IsActive)
        {
            button.interactable = true;

            if (slotText != null)
            {
                slotText.text =
                    $"<color=#B0C4DE>{equipmentName}</color>\n" +
                    "<color=#33FF66>● ACTIVE</color>\n" +
                    $"<color=#55AAFF>POWER {targetTurret.PowerCost}</color>";
            }

            return;
        }

<<<<<<< Updated upstream
        // =========================
        // OFF
        // =========================
=======
        // ============================================================
        // OFF
        // ============================================================
>>>>>>> Stashed changes

        button.interactable = true;

        if (slotText != null)
        {
            slotText.text =
                $"<color=#B0C4DE>{equipmentName}</color>\n" +
                "<color=#FF3333>○ OFF</color>\n" +
                $"<color=#55AAFF>POWER {targetTurret.PowerCost}</color>";
        }
    }
<<<<<<< Updated upstream
=======

    /// <summary>
    /// Assigns an actual individual turret to this UI slot.
    /// The UI stores a reference to the scene turret itself,
    /// not a prefab.
    /// </summary>
    public void SetTurret(Turret turret)
    {
        targetTurret = turret;
        Refresh();
    }

    /// <summary>
    /// Clears this slot.
    /// </summary>
    public void SetEmpty()
    {
        targetTurret = null;
        Refresh();
    }
>>>>>>> Stashed changes
}