using UnityEngine;
using ProjectBreach.Core;

public class EquipmentPanelManager : MonoBehaviour
{
    [Header("Turret UI Slots")]
    [SerializeField] private EquipmentSlotUI turretSlot1;
    [SerializeField] private EquipmentSlotUI turretSlot2;
    [SerializeField] private EquipmentSlotUI turretSlot3;
    [SerializeField] private EquipmentSlotUI turretSlot4;

    [Header("Room 1 Turrets")]
    [SerializeField] private Turret room1Turret1;
    [SerializeField] private Turret room1Turret2;

    [Header("Room 2 Turrets")]
    [SerializeField] private Turret room2Turret1;
    [SerializeField] private Turret room2Turret2;
    [SerializeField] private Turret room2Turret3;

    [Header("Room 3 Turrets")]
    [SerializeField] private Turret room3Turret1;
    [SerializeField] private Turret room3Turret2;

    [Header("Room 4 Turrets")]
    [SerializeField] private Turret room4Turret1;
    [SerializeField] private Turret room4Turret2;
    [SerializeField] private Turret room4Turret3;
    [SerializeField] private Turret room4Turret4;


    public void ShowRoom1()
    {
        SetTurrets(
            room1Turret1,
            room1Turret2,
            null,
            null
        );
    }

    public void ShowRoom2()
    {
        SetTurrets(
            room2Turret1,
            room2Turret2,
            room2Turret3,
            null
        );
    }

    public void ShowRoom3()
    {
        SetTurrets(
            room3Turret1,
            room3Turret2,
            null,
            null
        );
    }

    public void ShowRoom4()
    {
        SetTurrets(
            room4Turret1,
            room4Turret2,
            room4Turret3,
            room4Turret4
        );
    }


    private void SetTurrets(
        Turret turret1,
        Turret turret2,
        Turret turret3,
        Turret turret4)
    {
        SetSlot(turretSlot1, turret1);
        SetSlot(turretSlot2, turret2);
        SetSlot(turretSlot3, turret3);
        SetSlot(turretSlot4, turret4);
    }


    private void SetSlot(EquipmentSlotUI slot, Turret turret)
    {
        if (slot == null)
            return;

        if (turret == null)
            slot.SetEmpty();
        else
            slot.SetTurret(turret);
    }
}