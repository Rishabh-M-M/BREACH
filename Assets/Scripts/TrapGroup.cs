using UnityEngine;

public class TrapGroup : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.T;

    private SpikeTrap[] traps;
    private bool active;

    private void Awake()
    {
        traps = GetComponentsInChildren<SpikeTrap>(true);

        foreach (SpikeTrap trap in traps)
            trap.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            active = !active;

            foreach (SpikeTrap trap in traps)
                trap.SetActive(active);
        }
    }
}