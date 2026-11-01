using ProjectBreach.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TurretUpgradeCardType
{
    Standard,
    BaseTurret,
    DoubleBarrel,
    Sharpshooter,
    HeliosIncinerator,
    RocketLauncher
}

public class TurretUpgradeCardUI : MonoBehaviour
{
    [Header("Card Type")]
    [SerializeField] private TurretUpgradeCardType cardType;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Cost References")]
    [SerializeField] private TextMeshProUGUI powerCostText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private GameObject upgradeCostObject;

    [Header("Button References")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI buttonLabel;

    [Header("Card Appearance")]
    [SerializeField] private Image cardBackground;

    [Header("Costs")]
    [SerializeField] private int operatingPowerCost = 30;
    [SerializeField] private int permanentUpgradeCost = 0;

    [Header("Text Colours")]
    [SerializeField] private Color labelCyan = new Color32(0, 255, 255, 255);

    [Header("Card Colours")]
    [SerializeField] private Color normalBackground = new Color(0.035f, 0.075f, 0.12f);
    [SerializeField] private Color selectedBackground = new Color(0.05f, 0.22f, 0.16f);

    [Header("Button Colours")]
    [SerializeField] private Color normalButtonColour = new Color(0.051f, 0.110f, 0.169f);
    [SerializeField] private Color highlightedButtonColour = new Color(0.094f, 0.212f, 0.302f);
    [SerializeField] private Color pressedButtonColour = new Color(0f, 0.75f, 1f);
    [SerializeField] private Color disabledButtonColour = new Color(0.149f, 0.149f, 0.169f);

    [Header("Upgrade Text Colours")]
    [SerializeField] private Color availableUpgradeTextColour = new Color32(0, 255, 0, 255);
    [SerializeField] private Color unavailableUpgradeTextColour = new Color32(128, 128, 128, 255);
    [SerializeField] private Color purchasedUpgradeTextColour = new Color32(128, 128, 128, 255);

    private TurretUpgradePanelUI panel;
    private TurretUpgradeController currentController;
    private bool hasTurret;

    public TurretUpgradeCardType CardType => cardType;

    private void Awake()
    {
        if (upgradeButton == null)
            upgradeButton = GetComponentInChildren<Button>();

        if (buttonLabel == null && upgradeButton != null)
            buttonLabel = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

            ColorBlock colors = upgradeButton.colors;
            colors.normalColor = normalButtonColour;
            colors.highlightedColor = highlightedButtonColour;
            colors.pressedColor = pressedButtonColour;
            colors.selectedColor = highlightedButtonColour;
            colors.disabledColor = disabledButtonColour;
            colors.colorMultiplier = 1f;
            upgradeButton.colors = colors;
        }

        Refresh();
    }

    public void Bind(TurretUpgradePanelUI upgradePanel)
    {
        panel = upgradePanel;
    }

    public void Refresh(TurretUpgradeController controller, bool selectedTurretExists)
    {
        currentController = controller;
        hasTurret = selectedTurretExists && controller != null;
        Refresh();
    }

    public void SetTurret(bool selectedTurretExists, TurretUpgradeController controller)
    {
        currentController = controller;
        hasTurret = selectedTurretExists && controller != null;
        Refresh();
    }

    public void Refresh()
    {
        if (titleText != null)
            titleText.color = labelCyan;

        UpdateDescriptionText();
        UpdateCostText();
        UpdateButtonState();
    }

    private void UpdateDescriptionText()
    {
        if (descriptionText == null)
            return;

        descriptionText.text = GetDescription(cardType);
    }

    private string GetDescription(TurretUpgradeCardType type)
    {
        switch (type)
        {
            case TurretUpgradeCardType.DoubleBarrel:
                return "Paired burst shots with a slower fire rate for boosted damage over time.";

            case TurretUpgradeCardType.HeliosIncinerator:
                return "Incinerating beam that ramps up damage by locking onto the highest-HP enemy.";

            case TurretUpgradeCardType.RocketLauncher:
                return "Heavy explosive firepower designed to devastate enemy groups.";

            case TurretUpgradeCardType.Sharpshooter:
                return "Slow-firing precision weapon built for critical damage through long-range targeting.";

            case TurretUpgradeCardType.BaseTurret:
                return "Balanced automatic fire for reliable, sustained damage.";

            default:
                return string.Empty;
        }
    }

    private void UpdateCostText()
    {
        if (powerCostText != null)
        {
            powerCostText.text =
                ColourLabel("POWER COST:") + " " +
                ColourValue(operatingPowerCost.ToString(), Color.white);
        }

        bool showUpgradeCost =
            cardType != TurretUpgradeCardType.Standard &&
            cardType != TurretUpgradeCardType.BaseTurret;

        if (upgradeCostObject != null)
            upgradeCostObject.SetActive(showUpgradeCost);

        if (upgradeCostText != null && showUpgradeCost)
        {
            upgradeCostText.text =
                ColourLabel("UPGRADE COST:") + " " +
                ColourValue(permanentUpgradeCost.ToString(), Color.white);
        }
    }

    private bool IsSelected()
    {
        if (!hasTurret || currentController == null)
            return false;

        switch (cardType)
        {
            case TurretUpgradeCardType.BaseTurret:
                return currentController.IsBase;

            case TurretUpgradeCardType.DoubleBarrel:
                return currentController.IsDoubleBarrel;

            case TurretUpgradeCardType.Sharpshooter:
                return currentController.IsSharpshooter;

            case TurretUpgradeCardType.HeliosIncinerator:
                return currentController.IsHeliosIncinerator;

            case TurretUpgradeCardType.RocketLauncher:
                return currentController.IsRocketLauncher;

            default:
                return false;
        }
    }

    private void UpdateButtonState()
    {
        if (upgradeButton == null)
            return;

        if (cardType == TurretUpgradeCardType.Standard)
        {
            upgradeButton.gameObject.SetActive(false);

            if (cardBackground != null)
                cardBackground.color = normalBackground;

            return;
        }

        upgradeButton.gameObject.SetActive(true);

        if (!hasTurret || currentController == null)
        {
            upgradeButton.interactable = false;
            SetButtonLabel("UNAVAILABLE", unavailableUpgradeTextColour);

            if (cardBackground != null)
                cardBackground.color = normalBackground;

            return;
        }

        // The currently active variant is selected.
        if (IsSelected())
        {
            upgradeButton.interactable = false;
            SetButtonLabel("SELECTED", purchasedUpgradeTextColour);

            if (cardBackground != null)
                cardBackground.color = selectedBackground;

            return;
        }

        // Previously purchased variants remain permanently committed.
        TurretVariant variant = GetVariantForCard();

        if (currentController.IsVariantCommitted(variant))
        {
            upgradeButton.interactable = false;
            SetButtonLabel("COMMITTED", unavailableUpgradeTextColour);

            if (cardBackground != null)
                cardBackground.color = normalBackground;

            return;
        }

        if (cardType == TurretUpgradeCardType.BaseTurret)
        {
            upgradeButton.interactable = false;
            SetButtonLabel("BASE ONLY", unavailableUpgradeTextColour);

            if (cardBackground != null)
                cardBackground.color = normalBackground;

            return;
        }

        // Never-purchased variants remain available.
        upgradeButton.interactable = true;
        SetButtonLabel("UPGRADE", availableUpgradeTextColour);

        if (cardBackground != null)
            cardBackground.color = normalBackground;
    }

    private TurretVariant GetVariantForCard()
    {
        switch (cardType)
        {
            case TurretUpgradeCardType.DoubleBarrel:
                return TurretVariant.DoubleBarrel;

            case TurretUpgradeCardType.Sharpshooter:
                return TurretVariant.Sharpshooter;

            case TurretUpgradeCardType.HeliosIncinerator:
                return TurretVariant.HeliosIncinerator;

            case TurretUpgradeCardType.RocketLauncher:
                return TurretVariant.RocketLauncher;

            default:
                return TurretVariant.Base;
        }
    }


    private void SetButtonLabel(string label, Color colour)
    {
        if (buttonLabel == null)
            return;

        buttonLabel.text = label;
        buttonLabel.color = colour;
    }

    private void OnUpgradeClicked()
    {
        if (cardType == TurretUpgradeCardType.Standard)
            return;

        if (panel == null)
        {
            Debug.LogError(
                "[TurretUpgradeCardUI] Upgrade panel is not bound. " +
                "Add this card to the panel's Cards array.",
                this);
            return;
        }

        panel.TryPurchase(cardType);
    }

    private string ColourLabel(string label)
    {
        string hex = ColorUtility.ToHtmlStringRGBA(labelCyan);
        return $"<color=#{hex}>{label}</color>";
    }

    private string ColourValue(string value, Color colour)
    {
        string hex = ColorUtility.ToHtmlStringRGBA(colour);
        return $"<color=#{hex}>{value}</color>";
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
    }
}