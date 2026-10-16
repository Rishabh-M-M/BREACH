
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectBreach.Core;

public class TurretUpgradeOpenButton : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private EquipmentSlotUI equipmentSlot;
    [SerializeField] private EquipmentPanelManager equipmentPanelManager;
    [SerializeField] private TurretUpgradePanelUI upgradePanel;

    [Header("Button Colours")]
    [SerializeField]
    private Color normalColour =
        new Color(0.051f, 0.110f, 0.169f);

    [SerializeField]
    private Color highlightedColour =
        new Color(0.094f, 0.212f, 0.302f);

    [SerializeField]
    private Color pressedColour =
        new Color(0f, 0.75f, 1f);

    [SerializeField]
    private Color selectedColour =
        new Color(0.059f, 0.259f, 0.388f);

    [SerializeField]
    private Color disabledColour =
        new Color(0.149f, 0.149f, 0.169f);

    [Header("Text Colours")]
    [SerializeField]
    private Color availableTextColour =
        new Color(0.2f, 1f, 0.4f);

    [SerializeField]
    private Color unavailableTextColour =
        new Color(0.5f, 0.5f, 0.5f);

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        ApplyButtonColours();
    }

    private void Update()
    {
        RefreshButton();
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

    private Turret GetCurrentTurret()
    {
        if (equipmentSlot == null || equipmentPanelManager == null)
            return null;

        return equipmentPanelManager.GetTurretForSlot(equipmentSlot);
    }

    private void RefreshButton()
    {
        Turret turret = GetCurrentTurret();
        bool available = turret != null && !turret.IsDead;

        if (button != null)
            button.interactable = available;

        if (buttonText != null)
        {
            buttonText.color = available
                ? availableTextColour
                : unavailableTextColour;
        }
    }

    private void OnUpgradeClicked()
    {
        Turret turret = GetCurrentTurret();

        if (turret == null || turret.IsDead)
            return;

        if (upgradePanel == null)
        {
            Debug.LogWarning(
                "[TurretUpgradeOpenButton] Upgrade Panel is not assigned.",
                this
            );
            return;
        }

        upgradePanel.OpenForTurret(turret);

        if (button != null)
            button.OnDeselect(null);
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnUpgradeClicked);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnUpgradeClicked);
    }
}
