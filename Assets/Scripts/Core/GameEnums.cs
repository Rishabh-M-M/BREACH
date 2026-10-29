using UnityEngine;


public enum GameState { MainMenu, Playing, Paused, WaveClear, GameOver, Victory }
public enum RoomState { Active, Fallen }
public enum TurretState { Offline, Active, Destroyed }
public enum TrapState { Offline, Armed, Cooling }
public enum EnemyState { Moving, Attacking, Breaching }
public enum BuffType
{
    GeneratorPowerRegen,
    TurretDamage,
    TurretMaxHP,
    ArmouryDiscount,
    ShieldReactorRegen,
    // Reserved, not active this round (design doc §2.1): Communication passive.
    CommunicationMinimapAlarm
}