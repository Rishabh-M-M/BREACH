using UnityEngine;

/// <summary>
/// Optional per-prefab vertical spawn offset. Attach to an enemy prefab that
/// should spawn higher or lower than the spawner's own position (e.g. a flying
/// enemy). Enemies without this component spawn at the spawner's exact height.
/// </summary>
public class SpawnHeightOffset : MonoBehaviour
{
    [SerializeField] private float heightOffset = 0f;

    public float HeightOffset => heightOffset;
}