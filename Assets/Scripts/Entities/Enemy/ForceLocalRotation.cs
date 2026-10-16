using UnityEngine;

/// <summary>
/// Forces a fixed local rotation on this object every frame, after the
/// Animator has applied its pose for that frame — correcting a model's
/// visual facing direction without needing a separate non-animated wrapper.
/// </summary>
public class ForceLocalRotation : MonoBehaviour
{
    [SerializeField] private Vector3 rotationOffset;

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.Euler(rotationOffset);
    }
}