
using ProjectBreach.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TurretUpgradeCardType
{
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

    [Header("Text Colours")]
    [SerializeField]
    private Color labelCyan = new Color32(0, 255, 255, 255);

    [SerializeField]
    private Color infoTextColour = new Color32(80, 125, 165, 255);

    [SerializeField]
    private Color betterColour = new Color32(50, 230, 130, 255);

    [SerializeField]
    private Color worseColour = new Color32(255, 75, 75, 255);

    [SerializeField]
    private Color equalColour = new Color32(255, 255, 255, 255);

    [Header("Card Colours")]
    [SerializeField]
    private Color normalBackground = new Color(0.035f, 0.075f, 0.12f);

    [SerializeField]
    private Color selectedBackground = new Color(0.05f, 0.22f, 0.16f);

    [Header("Button Colours")]
    [SerializeField]
    private Color normalButtonColour = new Color(0.051f, 0.110f, 0.169f);

    [SerializeField]
    private Color highlightedButtonColour = new Color(0.094f, 0.212f, 0.302f);

    [SerializeField]
    private Color pressedButtonColour = new Color(0f, 0.75f, 1f);

    [SerializeField]
    private Color disabledButtonColour = new Color(0.149f, 0.149f, 0.169f);

    [Header("Upgrade Text Colours")]
    [SerializeField]
    private Color availableUpgradeTextColour = new Color32(0, 255, 0, 255);

    [SerializeField]
    private Color unavailableUpgradeTextColour = new Color32(128, 128, 128, 255);

    [SerializeField]
    private Color purchasedUpgradeTextColour = new Color32(128, 128, 128, 255);

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

    private void Update()
    {
        if (hasTurret && currentController != null)
        {
            UpdateDescriptionText();
            UpdateCostText();
            UpdateButtonState();
        }
    }

    public void Bind(TurretUpgradePanelUI upgradePanel)
    {
        panel = upgradePanel;
    }

    public void Refresh(
        TurretUpgradeController controller,
        bool selectedTurretExists)
    {
        currentController = controller;
        hasTurret = selectedTurretExists && controller != null;
        Refresh();
    }

    public void SetTurret(
        bool selectedTurretExists,
        TurretUpgradeController controller)
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

        descriptionText.color = infoTextColour;

        if (currentController == null)
        {
            descriptionText.text = GetFallbackInfo();
            return;
        }

        descriptionText.text =
            currentController.GetUpgradeInfo(GetVariantForCard());
    }

    private string GetFallbackInfo()
    {
        switch (cardType)
        {
            case TurretUpgradeCardType.BaseTurret:
                return "Balanced automatic fire for reliable, sustained damage.";

            default:
                return "Select a turret to view upgrade information.";
        }
    }

    private void UpdateCostText()
    {
        bool showUpgradeCost =
            cardType != TurretUpgradeCardType.BaseTurret;

        if (upgradeCostObject != null)
            upgradeCostObject.SetActive(showUpgradeCost);

        if (currentController == null)
        {
            if (powerCostText != null)
                powerCostText.text = ColourLabel("POWER COST:") + " --";

            if (upgradeCostText != null && showUpgradeCost)
                upgradeCostText.text = ColourLabel("UPGRADE COST:") + " --";

            return;
        }

        TurretVariant variant = GetVariantForCard();

        int operatingCost =
            currentController.GetOperatingPowerCostForVariant(variant);

        int baseOperatingCost =
            currentController.GetOperatingPowerCostForVariant(TurretVariant.Base);

        if (powerCostText != null)
        {
            Color powerColour =
                GetComparisonColour(operatingCost, baseOperatingCost);

            powerCostText.text =
                ColourLabel("POWER COST:") + " " +
                ColourValue(operatingCost.ToString(), powerColour);
        }

        if (upgradeCostText != null && showUpgradeCost)
        {
            int purchaseCost =
                currentController.GetUpgradePurchaseCost(variant);

            upgradeCostText.text =
                ColourLabel("UPGRADE COST:") + " " +
                ColourValue(purchaseCost.ToString(), Color.white);
        }
    }

    private Color GetComparisonColour(int value, int normalValue)
    {
        if (value < normalValue)
            return betterColour;

        if (value > normalValue)
            return worseColour;

        return equalColour;
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

        upgradeButton.gameObject.SetActive(true);

        if (!hasTurret || currentController == null)
        {
            upgradeButton.interactable = false;
            SetButtonLabel("UNAVAILABLE", unavailableUpgradeTextColour);

            SetCardBackground(normalBackground);
            return;
        }

        // BASE TURRET CARD
        // Green by default; blue after any upgrade.
        if (cardType == TurretUpgradeCardType.BaseTurret)
        {
            bool baseIsActive = currentController.IsBase;

            SetCardBackground(
                baseIsActive ? selectedBackground : normalBackground
            );

            upgradeButton.interactable = false;
            SetButtonLabel("BASE TURRET", unavailableUpgradeTextColour);
            return;
        }

        // ACTIVE UPGRADE CARD
        if (IsSelected())
        {
            SetCardBackground(selectedBackground);

            upgradeButton.interactable = false;
            SetButtonLabel("SELECTED", purchasedUpgradeTextColour);
            return;
        }

        // ALREADY COMMITTED UPGRADE
        TurretVariant variant = GetVariantForCard();

        if (currentController.IsVariantCommitted(variant))
        {
            SetCardBackground(normalBackground);

            upgradeButton.interactable = false;
            SetButtonLabel("COMMITTED", unavailableUpgradeTextColour);
            return;
        }

        // AVAILABLE UPGRADE
        SetCardBackground(normalBackground);

        upgradeButton.interactable = true;
        SetButtonLabel("UPGRADE", availableUpgradeTextColour);
    }

    private void SetCardBackground(Color colour)
    {
        if (cardBackground != null)
        {
            cardBackground.color = colour;
        }
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
