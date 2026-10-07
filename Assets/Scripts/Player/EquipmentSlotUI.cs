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
    [SerializeField] private Color normalColour = new Color(0.051f, 0.110f, 0.169f);
    [SerializeField] private Color highlightedColour = new Color(0.094f, 0.212f, 0.302f);
    [SerializeField] private Color pressedColour = new Color(0f, 0.75f, 1f);
    [SerializeField] private Color selectedColour = new Color(0.059f, 0.259f, 0.388f);
    [SerializeField] private Color disabledColour = new Color(0.149f, 0.149f, 0.169f);

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

        // Prevent Unity from making colours brighter/darker
        colours.colorMultiplier = 1f;

        // Quick transition between button states
        colours.fadeDuration = 0.1f;

        button.colors = colours;
    }

    private void OnClicked()
    {
        if (targetTurret == null)
            return;

        // Destroyed turret cannot be used
        if (targetTurret.IsDead)
            return;

        // =========================
        // TURN OFF
        // =========================

        if (targetTurret.IsActive)
        {
            targetTurret.TurnOff();

            if (PowerManager.Instance != null)
                PowerManager.Instance.Refund(targetTurret.PowerCost);
        }

        // =========================
        // TURN ON
        // =========================

        else
        {
            //if (PowerManager.Instance == null)
            //{
            //    Debug.LogError("[EquipmentSlotUI] PowerManager not found!");
            //    return;
            //}

            //bool success = PowerManager.Instance.TryAllocate(
            //    targetTurret.PowerCost
            //);

            //if (!success)
            //{
            //    if (UIManager.Instance != null)
            //        UIManager.Instance.ShowMessage("Insufficient power!");

            //    return;
            //}

            targetTurret.TurnOn();
        }

        Refresh();

        // Return the button to its normal visual state
        if (button != null)
            button.OnDeselect(null);
    }
    public void Refresh()
    {
        if (button == null)
            return;

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

        button.interactable = true;

        if (slotText != null)
        {
            slotText.text =
                $"<color=#B0C4DE>{equipmentName}</color>\n" +
                "<color=#FF3333>○ OFF</color>\n" +
                $"<color=#55AAFF>POWER {targetTurret.PowerCost}</color>";
        }
    }

    // Allows the Equipment Panel Manager to assign a turret to this slot.
    public void SetTurret(Turret turret)
    {
        targetTurret = turret;
        Refresh();
    }

    // Clears this slot and makes it display EMPTY.
    public void SetEmpty()
    {
        targetTurret = null;
        Refresh();
    }
}