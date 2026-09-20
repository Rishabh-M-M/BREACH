using UnityEngine;

public class EnemyTargetPoint : MonoBehaviour
{
    public bool IsOccupied { get; private set; }

    public void Claim()
    {
        IsOccupied = true;
    }

    public void Release()
    {
        IsOccupied = false;
    }
}