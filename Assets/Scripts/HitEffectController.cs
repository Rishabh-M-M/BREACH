using UnityEngine;

public class HitEffectController : MonoBehaviour
{
    private static readonly int HitTimeID = Shader.PropertyToID("_HitTime");

    private Renderer[] targetRenderers;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        targetRenderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void TriggerHit()
    {
        float hitTimeValue = Time.timeSinceLevelLoad;

        foreach (Renderer r in targetRenderers)
        {
            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(HitTimeID, hitTimeValue);
            r.SetPropertyBlock(propertyBlock);
        }
    }
}