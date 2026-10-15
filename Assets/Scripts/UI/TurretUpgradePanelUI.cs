using UnityEngine;
using UnityEngine.UI;
using ProjectBreach.Core;

public class TurretUpgradePanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Upgrade Cards")]
    [SerializeField] private TurretUpgradeCardUI[] cards;

    private Turret selectedTurret;
    private TurretUpgradeController selectedController;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }

        BindCards();
        RefreshCards();
        ClosePanel();
    }

    private void BindCards()
    {
        if (cards == null)
        {
            Debug.LogError(
                "[TurretUpgradePanelUI] Cards array is not assigned.",
                this);
            return;
        }

        foreach (TurretUpgradeCardUI card in cards)
        {
            if (card != null)
                card.Bind(this);
        }
    }

    public void OpenForTurret(Turret turret)
    {
        if (turret == null)
        {
            Debug.LogWarning(
                "[TurretUpgradePanelUI] The supplied turret is null.",
                this);
            return;
        }

        if (turret.IsDead)
        {
            Debug.LogWarning(
                "[TurretUpgradePanelUI] Cannot upgrade a destroyed turret: "
                + turret.name,
                turret);
            return;
        }

        selectedTurret = turret;
        selectedController = FindUpgradeController(turret);

        if (selectedController == null)
        {
            Debug.LogError(
                "[TurretUpgradePanelUI] No TurretUpgradeController found for "
                + turret.name + ".",
                turret);

            selectedTurret = null;
            return;
        }

        BindCards();
        RefreshCards();

        if (panelRoot != null)
            panelRoot.SetActive(true);

        Debug.Log(
            "[TurretUpgradePanelUI] Opened for turret: " + turret.name,
            turret);
    }

    private TurretUpgradeController FindUpgradeController(Turret turret)
    {
        TurretUpgradeController controller =
            turret.GetComponent<TurretUpgradeController>();

        if (controller == null)
        {
            controller =
                turret.GetComponentInChildren<TurretUpgradeController>(true);
        }

        if (controller == null)
        {
            controller =
                turret.GetComponentInParent<TurretUpgradeController>();
        }

        return controller;
    }

    public void TryPurchase(TurretUpgradeCardType type)
    {
        if (selectedTurret == null || selectedTurret.IsDead)
        {
            Debug.LogWarning(
                "[TurretUpgradePanelUI] No valid turret is open.",
                this);
            return;
        }

        if (selectedController == null)
            selectedController = FindUpgradeController(selectedTurret);

        if (selectedController == null)
        {
            Debug.LogError(
                "[TurretUpgradePanelUI] No upgrade controller for "
                + selectedTurret.name,
                selectedTurret);
            return;
        }

        bool purchased;

        switch (type)
        {
            case TurretUpgradeCardType.BaseTurret:
                Debug.Log(
                "[TurretUpgradePanelUI] Returning to Base Turret is not allowed.",
                this);
                return;

            case TurretUpgradeCardType.DoubleBarrel:
                purchased = selectedController.TryUpgradeToDoubleBarrel();
                break;

            case TurretUpgradeCardType.Sharpshooter:
                purchased = selectedController.TryUpgradeToSharpshooter();
                break;

            case TurretUpgradeCardType.HeliosIncinerator:
                purchased = selectedController.TryUpgradeToHeliosIncinerator();
                break;

            case TurretUpgradeCardType.RocketLauncher:
                Debug.Log(
                    "[TurretUpgradePanelUI] Rocket Launcher is not implemented yet.",
                    this);
                return;

            case TurretUpgradeCardType.Standard:
            default:
                return;
        }

        RefreshCards();

        if (purchased)
        {
            Debug.Log(
                "[TurretUpgradePanelUI] Selected variant for "
                + selectedTurret.name + ": " + type,
                selectedTurret);
        }
        else
        {
            Debug.LogWarning(
                "[TurretUpgradePanelUI] Could not switch variant for "
                + selectedTurret.name + ": " + type,
                selectedTurret);
        }
    }

    private void RefreshCards()
    {
        if (cards == null)
            return;

        bool hasTurret =
            selectedTurret != null && !selectedTurret.IsDead;

        foreach (TurretUpgradeCardUI card in cards)
        {
            if (card == null)
                continue;

            card.Bind(this);
            card.Refresh(selectedController, hasTurret);
        }
    }

    public void ClosePanel()
    {
        selectedTurret = null;
        selectedController = null;

        RefreshCards();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
    }
}