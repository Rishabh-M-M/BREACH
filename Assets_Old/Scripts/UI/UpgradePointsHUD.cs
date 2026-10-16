using UnityEngine;
using TMPro;
using ProjectBreach.Core;

namespace ProjectBreach.UI
{
    /// <summary>
    /// Displays the current upgrade-point balance, and optionally exposes the
    /// "buy" entry point for a selected upgradeable target.
    ///
    /// GRASP: Information Expert -- the balance lives in UpgradeManager, so this
    /// only renders it; and GRASP: Controller -- purchases go through
    /// UpgradeManager.Purchase, never through the HUD.
    ///
    /// Scene setup: drag the text in. To allow buying from here, assign
    /// defaultUpgrade and drag the button's OnClick to OnBuyClicked.
    /// </summary>
    public class UpgradePointsHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private UnityEngine.UI.Button buyButton;

        [Header("Optional purchase")]
        [Tooltip("Drag the turret GameObject here. Declared as MonoBehaviour because " +
                 "Unity does not surface plain interface fields in the Inspector; it is " +
                 "cast to IUpgradable on purchase. Leave empty for a display-only HUD.")]
        [SerializeField] private MonoBehaviour targetObject;

        [SerializeField] private UpgradeData defaultUpgrade;

        [Header("Text")]
        [Tooltip("{0} = points balance.")]
        [SerializeField] private string format = "Points {0}";

        [SerializeField] private bool hideWhenEmpty = true;

        private void OnEnable()
        {
            EventRegistry.OnUpgradePointsChanged += HandlePointsChanged;
        }

        private void OnDisable()
        {
            EventRegistry.OnUpgradePointsChanged -= HandlePointsChanged;
        }

        private void Start()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }

            if (UpgradeManager.Instance != null)
            {
                Refresh(UpgradeManager.Instance.Points);
            }
        }

        private void HandlePointsChanged(int points)
        {
            Refresh(points);
        }

        private void Refresh(int points)
        {
            if (pointsText != null)
            {
                pointsText.text = string.Format(format, points);
            }

            if (buyButton != null && defaultUpgrade != null)
            {
                buyButton.interactable = points >= defaultUpgrade.Cost
                                        && (targetObject is IUpgradable);
            }

            if (hideWhenEmpty && pointsText != null)
            {
                pointsText.gameObject.SetActive(points > 0);
            }
        }

        public void OnBuyClicked()
        {
            if (UpgradeManager.Instance == null)
            {
                Debug.LogWarning("[UpgradePointsHUD] UpgradeManager not found in scene.");
                return;
            }

            if (!(targetObject is IUpgradable target))
            {
                Debug.LogWarning("[UpgradePointsHUD] No IUpgradable target assigned.");
                return;
            }

            UpgradeManager.Instance.Purchase(target, defaultUpgrade);
        }
    }
}
