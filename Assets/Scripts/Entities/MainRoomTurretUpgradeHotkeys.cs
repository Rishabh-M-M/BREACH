
using UnityEngine;
using ProjectBreach.Core;

/// <summary>
/// Main-room turret upgrade hotkeys.
/// 1 = Double Barrel
/// 2 = Sharpshooter
/// 3 = Helios Incinerator
///
/// The main turret is always on and has zero operating power cost.
/// Upgrade purchases still consume their normal permanent power cost.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Turret))]
[RequireComponent(typeof(TurretUpgradeController))]
public class MainRoomTurretUpgradeHotkeys : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Turret turret;
    [SerializeField] private TurretUpgradeController upgradeController;
    [SerializeField] private Room mainRoom;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode doubleBarrelKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode sharpshooterKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode heliosKey = KeyCode.Alpha3;

    [Header("Main Turret Power")]
    [Tooltip("The main turret always operates for free.")]
    [SerializeField] private int operatingPowerCost = 0;

    private void Awake()
    {
        if (turret == null)
            turret = GetComponent<Turret>();

        if (upgradeController == null)
            upgradeController = GetComponent<TurretUpgradeController>();

        if (mainRoom == null)
            mainRoom = GetComponentInParent<Room>();
    }

    private void Start()
    {
        EnsureMainTurretActive();
    }

    private void Update()
    {
        if (turret == null || upgradeController == null)
            return;

        if (mainRoom != null && mainRoom.IsPermanentlyDisabled)
            return;

        if (Input.GetKeyDown(doubleBarrelKey))
        {
            TryBuy(
                "Double Barrel",
                upgradeController.TryUpgradeToDoubleBarrel
            );
        }

        if (Input.GetKeyDown(sharpshooterKey))
        {
            TryBuy(
                "Sharpshooter",
                upgradeController.TryUpgradeToSharpshooter
            );
        }

        if (Input.GetKeyDown(heliosKey))
        {
            TryBuy(
                "Helios Incinerator",
                upgradeController.TryUpgradeToHeliosIncinerator
            );
        }

        // Keep the main turret free and active.
        EnsureMainTurretActive();
    }

    private void TryBuy(string upgradeName, System.Func<bool> purchase)
    {
        if (upgradeController.CurrentVariant != TurretVariant.Base)
        {
            UIManager.Instance?.ShowMessage(
                "This turret already has an upgrade installed."
            );
            return;
        }

        bool success = purchase();

        if (!success)
            return;

        // The upgrade controller may change the operating cost and
        // turn the turret off during the purchase. Restore free operation.
        EnsureMainTurretActive();

        UIManager.Instance?.ShowMessage(
            upgradeName + " installed. Main turret operating for free."
        );

        Debug.Log(
            $"[MainRoomTurretUpgradeHotkeys] Purchased {upgradeName}. " +
            "Operating cost = 0."
        );
    }

    private void EnsureMainTurretActive()
    {
        if (turret == null)
            return;

        if (mainRoom != null && mainRoom.IsPermanentlyDisabled)
            return;

        // Override the operating cost set by TurretUpgradeController.
        turret.SetPowerCost(operatingPowerCost);

        if (!turret.IsActive)
            turret.TurnOn();
    }
}
