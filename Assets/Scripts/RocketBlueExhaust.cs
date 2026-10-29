using UnityEngine;

/// <summary>
/// Adds blue emission and exhaust to a rocket. Attach to the projectile root.
/// Assign Rocket Visual Root to the child containing the rocket mesh and its
/// 180-degree model correction; the exhaust is parented to that transform.
/// </summary>
[DisallowMultipleComponent]
public class RocketBlueExhaustRotated : MonoBehaviour
{
    [Header("Rocket Orientation")]
    [Tooltip("Child transform containing the rocket mesh and its model rotation correction.")]
    [SerializeField] private Transform rocketVisualRoot;
    [Tooltip("Exhaust emitter offset in the rocket visual's local space.")]
    [SerializeField] private Vector3 exhaustLocalPosition = new Vector3(0f, 0f, -0.35f);
    [Tooltip("For a rocket mesh facing local +Z, Y=180 emits particles behind it.")]
    [SerializeField] private Vector3 exhaustLocalRotation = new Vector3(0f, 180f, 0f);

    [Header("Rocket Glow")]
    [SerializeField, Min(0f)] private float emissionIntensity = 2.8f;
    [SerializeField] private Color emissionColor = new Color(0.05f, 0.55f, 1f, 1f);
    [Tooltip("Leave empty to affect all child renderers.")]
    [SerializeField] private Renderer[] rocketRenderers;

    [Header("Blue Exhaust")]
    [SerializeField] private bool createExhaustAutomatically = true;
    [SerializeField] private Color exhaustCoreColor = new Color(0.35f, 0.9f, 1f, 1f);
    [SerializeField] private Color exhaustOuterColor = new Color(0.02f, 0.22f, 1f, 0.15f);
    [SerializeField, Min(1f)] private float particlesPerSecond = 75f;
    [SerializeField, Min(0.05f)] private float exhaustLength = 1.1f;
    [SerializeField, Min(0.01f)] private float particleLifetime = 0.28f;
    [SerializeField, Min(0.01f)] private float particleSize = 0.16f;
    [SerializeField, Min(0f)] private float particleSpeed = 2.2f;
    [SerializeField, Range(0f, 45f)] private float coneAngle = 12f;

    private Material exhaustMaterial;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        ResolveVisualRoot();
        ApplyRocketGlow();
        if (createExhaustAutomatically) CreateExhaust();
    }

    private void ResolveVisualRoot()
    {
        if (rocketVisualRoot != null) return;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            if (rend != null && rend.transform != transform)
            {
                rocketVisualRoot = rend.transform;
                return;
            }
        }

        rocketVisualRoot = transform;
    }

    private void ApplyRocketGlow()
    {
        if (rocketRenderers == null || rocketRenderers.Length == 0)
            rocketRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in rocketRenderers)
        {
            if (rend == null) continue;

            foreach (Material mat in rend.materials)
            {
                if (mat == null || !mat.HasProperty(EmissionColorId)) continue;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor(EmissionColorId, emissionColor * emissionIntensity);
            }
        }
    }

    private void CreateExhaust()
    {
        Transform parent = rocketVisualRoot != null ? rocketVisualRoot : transform;
        Transform existing = parent.Find("Blue Rocket Exhaust");

        GameObject exhaustObject;
        ParticleSystem ps;

        if (existing != null && existing.TryGetComponent(out ps))
        {
            exhaustObject = existing.gameObject;
        }
        else
        {
            exhaustObject = new GameObject("Blue Rocket Exhaust");
            exhaustObject.transform.SetParent(parent, false);
            ps = exhaustObject.AddComponent<ParticleSystem>();
        }

        exhaustObject.transform.localPosition = exhaustLocalPosition;
        exhaustObject.transform.localRotation = Quaternion.Euler(exhaustLocalRotation);
        exhaustObject.transform.localScale = Vector3.one;
        ConfigureExhaust(ps);
    }

    private void ConfigureExhaust(ParticleSystem ps)
    {
        // A ParticleSystem can begin playing as soon as it is added or
        // instantiated. Stop and clear it BEFORE changing MainModule settings.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.startLifetime = new ParticleSystem.MinMaxCurve(particleLifetime * 0.65f, particleLifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed * 0.65f, particleSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.55f, particleSize);
        main.startColor = new ParticleSystem.MinMaxGradient(exhaustCoreColor);
        main.maxParticles = 500;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = particlesPerSecond;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle;
        shape.radius = 0.055f;
        shape.length = exhaustLength;
        shape.rotation = Vector3.zero;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(exhaustCoreColor, 0f),
                new GradientColorKey(new Color(0.03f, 0.35f, 1f, 1f), 0.45f),
                new GradientColorKey(exhaustOuterColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.95f, 0f),
                new GradientAlphaKey(0.65f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = new ParticleSystem.MinMaxGradient(gradient);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(
                new Keyframe(0f, 0.55f),
                new Keyframe(0.25f, 1f),
                new Keyframe(1f, 0.05f)));

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.18f;
        noise.frequency = 1.8f;
        noise.scrollSpeed = 0.5f;

        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.alignment = ParticleSystemRenderSpace.View;
        psRenderer.material = GetExhaustMaterial();

        // Start only after every particle module has been configured.
        ps.Play(true);
    }

    private Material GetExhaustMaterial()
    {
        if (exhaustMaterial != null) return exhaustMaterial;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
            Shader.Find("Particles/Standard Unlit") ??
            Shader.Find("Legacy Shaders/Particles/Additive") ??
            Shader.Find("Unlit/Color");

        if (shader == null)
        {
            Debug.LogWarning("[RocketBlueExhaustRotated] No supported particle shader found. Assign a transparent/additive particle material.", this);
            return null;
        }

        exhaustMaterial = new Material(shader);
        exhaustMaterial.name = "Runtime Blue Rocket Exhaust (Instance)";

        if (exhaustMaterial.HasProperty(BaseColorId))
            exhaustMaterial.SetColor(BaseColorId, exhaustCoreColor);
        if (exhaustMaterial.HasProperty(ColorId))
            exhaustMaterial.SetColor(ColorId, exhaustCoreColor);
        if (exhaustMaterial.HasProperty(EmissionColorId))
        {
            exhaustMaterial.EnableKeyword("_EMISSION");
            exhaustMaterial.SetColor(EmissionColorId, exhaustCoreColor * 2.5f);
        }
        if (exhaustMaterial.HasProperty("_Surface"))
            exhaustMaterial.SetFloat("_Surface", 1f);
        if (exhaustMaterial.HasProperty("_Mode"))
            exhaustMaterial.SetFloat("_Mode", 2f);

        return exhaustMaterial;
    }

    private void OnDestroy()
    {
        if (exhaustMaterial != null) Destroy(exhaustMaterial);
    }
}
