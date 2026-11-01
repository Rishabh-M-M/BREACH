
using UnityEngine;
using ProjectBreach.Core;

public class EquipmentPanelManager : MonoBehaviour
{
    [Header("Turret UI Slots")]
    [SerializeField] private EquipmentSlotUI turretSlot1;
    [SerializeField] private EquipmentSlotUI turretSlot2;
    [SerializeField] private EquipmentSlotUI turretSlot3;
    [SerializeField] private EquipmentSlotUI turretSlot4;

    [Header("Trap UI Slots")]
    [SerializeField] private TrapEquipmentSlotUI trapSlot1;
    [SerializeField] private TrapEquipmentSlotUI trapSlot2;

    [Header("Room 1 Traps")]
    [SerializeField] private TrapGroup room1Trap1;
    [SerializeField] private TrapGroup room1Trap2;

    [Header("Room 2 Traps")]
    [SerializeField] private TrapGroup room2Trap1;
    [SerializeField] private TrapGroup room2Trap2;

    [Header("Room 3 Traps")]
    [SerializeField] private TrapGroup room3Trap1;
    [SerializeField] private TrapGroup room3Trap2;

    [Header("Room 4 Traps")]
    [SerializeField] private TrapGroup room4Trap1;
    [SerializeField] private TrapGroup room4Trap2;

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
        SetTurrets(room1Turret1, room1Turret2, null, null);
        SetTraps(room1Trap1, room1Trap2);
    }

    public void ShowRoom2()
    {
        SetTurrets(room2Turret1, room2Turret2, room2Turret3, null);
        SetTraps(room2Trap1, room2Trap2);
    }

    public void ShowRoom3()
    {
        SetTurrets(room3Turret1, room3Turret2, null, null);
        SetTraps(room3Trap1, room3Trap2);
    }

    public void ShowRoom4()
    {
        SetTurrets(room4Turret1, room4Turret2, room4Turret3, room4Turret4);
        SetTraps(room4Trap1, room4Trap2);
    }

    private void SetTraps(TrapGroup trap1, TrapGroup trap2)
    {
        if (trapSlot1 != null)
        {
            if (trap1 != null) trapSlot1.SetTrap(trap1, "TRAP 1");
            else trapSlot1.SetEmpty("TRAP 1");
        }

        if (trapSlot2 != null)
        {
            if (trap2 != null) trapSlot2.SetTrap(trap2, "TRAP 2");
            else trapSlot2.SetEmpty("TRAP 2");
        }
    }


    private void SetTurrets(
        Turret turret1,
        Turret turret2,
        Turret turret3,
        Turret turret4)
    {
        // Remember which turrets belong to the current room.
        roomTurretForSlot1 = turret1;
        roomTurretForSlot2 = turret2;
        roomTurretForSlot3 = turret3;
        roomTurretForSlot4 = turret4;

        // Update the four activation slots.
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

    // Returns the turret currently assigned to the requested slot.
    public Turret GetTurretForSlot(EquipmentSlotUI slot)
    {
        if (slot == turretSlot1)
            return roomTurretForSlot1;

        if (slot == turretSlot2)
            return roomTurretForSlot2;

        if (slot == turretSlot3)
            return roomTurretForSlot3;

        if (slot == turretSlot4)
            return roomTurretForSlot4;

        return null;
    }

    private Turret roomTurretForSlot1;
    private Turret roomTurretForSlot2;
    private Turret roomTurretForSlot3;
    private Turret roomTurretForSlot4;
}
