
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BreachLevelProfile
{
    [Header("Main Settings")]
    [Range(0, 10)] public float pulseSpeed = 3f;
    [Range(0, 2)] public float corruptionStrength = 0.5f;

    [Header("Individual Effects")]
    [Range(0, 2)] public float heatDistortion = 0.5f;
    [Range(0, 2)] public float horizontalGlitch = 0.4f;
    [Range(0, 2)] public float scanlines = 0.35f;
    [Range(0, 2)] public float electricalCracks = 0.2f;
    [Range(0, 2)] public float digitalStatic = 0.2f;
    [Range(0, 2)] public float edgeGlow = 0.45f;
    [Range(0, 2)] public float pulseIntensity = 0.6f;
}

public class BreachController : MonoBehaviour
{
    [Header("Breach Settings")]
    public float breachDuration = 5f;

    [Range(0, 1)]
    public float breachAmount = 1f;

    public Color breachColour = Color.red;

    [Range(0, 10)]
    public float pulseSpeed = 3f;

    [Header("Screen Corruption")]
    [Range(0, 2)]
    public float corruptionStrength = 2f;

    [Header("Permanent Breach Level")]
    [Range(0, 4)]
    public int breachLevel = 0;

    [Header("Level 1 - Initial Corruption")]
    public BreachLevelProfile level1 = new BreachLevelProfile
    {
        pulseSpeed = 3.5f,
        corruptionStrength = 0.5f,
        heatDistortion = 0.5f,
        horizontalGlitch = 0.4f,
        scanlines = 0.35f,
        electricalCracks = 0.2f,
        digitalStatic = 0.2f,
        edgeGlow = 0.45f,
        pulseIntensity = 0.6f
    };

    [Header("Level 2 - Increasing Corruption")]
    public BreachLevelProfile level2 = new BreachLevelProfile
    {
        pulseSpeed = 4.5f,
        corruptionStrength = 0.9f,
        heatDistortion = 0.8f,
        horizontalGlitch = 0.8f,
        scanlines = 0.65f,
        electricalCracks = 0.6f,
        digitalStatic = 0.6f,
        edgeGlow = 0.8f,
        pulseIntensity = 0.9f
    };

    [Header("Level 3 - Severe Corruption")]
    public BreachLevelProfile level3 = new BreachLevelProfile
    {
        pulseSpeed = 6f,
        corruptionStrength = 1.4f,
        heatDistortion = 1.3f,
        horizontalGlitch = 1.3f,
        scanlines = 1.1f,
        electricalCracks = 1.1f,
        digitalStatic = 1.2f,
        edgeGlow = 1.3f,
        pulseIntensity = 1.3f
    };

    [Header("Level 4 - Critical Corruption")]
    public BreachLevelProfile level4 = new BreachLevelProfile
    {
        pulseSpeed = 8f,
        corruptionStrength = 2f,
        heatDistortion = 1.8f,
        horizontalGlitch = 2f,
        scanlines = 1.6f,
        electricalCracks = 1.8f,
        digitalStatic = 2f,
        edgeGlow = 2f,
        pulseIntensity = 1.8f
    };

    [Header("Screen Setup")]
    public Transform screenRoot;
    public Material breachMaterial;

    [Header("Breach Lights")]
    public Light[] breachLights;

    [Header("Light Pulse")]
    public float lightPulseSpeed = 6f;
    public float lightMinIntensity = 1f;
    public float lightMaxIntensity = 4f;

    [Header("Light Colour")]
    public Color breachLightColour = Color.red;

    [Header("Camera Effects")]
    public CameraEffects cameraEffects;

    [Header("Audio")]
    public BreachAudioManager audioManager;

    // Active individual effect values sent to the shader.
    [Header("Active Shader Values - Runtime")]
    [Range(0, 2)] public float heatDistortionStrength = 1f;
    [Range(0, 2)] public float glitchStrength = 1f;
    [Range(0, 2)] public float scanlineStrength = 1f;
    [Range(0, 2)] public float crackStrength = 1f;
    [Range(0, 2)] public float staticStrength = 1f;
    [Range(0, 2)] public float edgeGlowStrength = 1f;
    [Range(0, 2)] public float pulseIntensity = 1f;

    private Renderer[] screenRenderers;

    private Material[][] originalMaterials;
    private Material[][] breachMaterials;

    private bool[] originalLightStates;
    private float[] originalLightIntensities;
    private Color[] originalLightColours;

    private bool breachRunning;
    private Coroutine breachCoroutine;

    private void Start()
    {
        FindScreens();
        StoreOriginalMaterials();
        StoreOriginalLights();

        // Apply the profile corresponding to the starting level.
        ApplyLevelProfile(breachLevel);
    }


    private void Update()
    {
        // Temporary keyboard testing: press 1, 2, 3 or 4.
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetTestBreachLevel(1);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetTestBreachLevel(2);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetTestBreachLevel(3);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SetTestBreachLevel(4);

        // Keep your existing breach light pulsing.
        if (breachRunning)
        {
            PulseLights();
        }
    }


    // ====================================================
    // APPLY LEVEL PROFILE
    // ====================================================

    private void ApplyLevelProfile(int level)
    {
        BreachLevelProfile profile = null;

        switch (level)
        {
            case 1:
                profile = level1;
                break;

            case 2:
                profile = level2;
                break;

            case 3:
                profile = level3;
                break;

            case 4:
                profile = level4;
                break;
        }

        // Level 0 keeps the normal Inspector values.
        if (profile == null)
            return;

        pulseSpeed = profile.pulseSpeed;
        corruptionStrength = profile.corruptionStrength;

        heatDistortionStrength = profile.heatDistortion;
        glitchStrength = profile.horizontalGlitch;
        scanlineStrength = profile.scanlines;
        crackStrength = profile.electricalCracks;
        staticStrength = profile.digitalStatic;
        edgeGlowStrength = profile.edgeGlow;
        pulseIntensity = profile.pulseIntensity;

        Debug.Log(
            $"[BreachController] Applied Level {level} profile."
        );

        Debug.Log(
            $"Pulse: {pulseSpeed} | Corruption: {corruptionStrength} | " +
            $"Heat: {heatDistortionStrength} | Glitch: {glitchStrength} | " +
            $"Scanlines: {scanlineStrength} | Cracks: {crackStrength} | " +
            $"Static: {staticStrength} | Edge Glow: {edgeGlowStrength} | " +
            $"Pulse Intensity: {pulseIntensity}"
        );
    }

    // ====================================================
    // TEST LEVEL CONTROL
    // ====================================================

    private void SetTestBreachLevel(int level)
    {
        breachLevel = Mathf.Clamp(level, 1, 4);

        ApplyLevelProfile(breachLevel);

        if (breachRunning)
        {
            ApplyBreachMaterials();
        }
        else
        {
            StartBreach();
        }
    }

    // ====================================================
    // FIND SCREENS
    // ====================================================

    private void FindScreens()
    {
        if (screenRoot == null)
        {
            GameObject mainRoom = GameObject.Find("MainRoom");

            if (mainRoom != null)
                screenRoot = mainRoom.transform;
        }

        if (screenRoot == null)
        {
            Debug.LogError(
                "BreachController: MainRoom / screenRoot was not found."
            );

            screenRenderers = new Renderer[0];
            return;
        }

        List<Renderer> foundScreens = new List<Renderer>();

        Renderer[] allRenderers =
            screenRoot.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null)
                continue;

            if (renderer.gameObject.name.StartsWith("Screen "))
            {
                foundScreens.Add(renderer);
            }
        }

        screenRenderers = foundScreens.ToArray();

        Debug.Log(
            "BreachController found " +
            screenRenderers.Length +
            " screens."
        );
    }

    // ====================================================
    // STORE ORIGINAL MATERIALS
    // ====================================================

    private void StoreOriginalMaterials()
    {
        originalMaterials =
            new Material[screenRenderers.Length][];

        breachMaterials =
            new Material[screenRenderers.Length][];

        for (int i = 0; i < screenRenderers.Length; i++)
        {
            Renderer renderer = screenRenderers[i];

            if (renderer == null)
                continue;

            originalMaterials[i] = renderer.sharedMaterials;

            breachMaterials[i] =
                new Material[originalMaterials[i].Length];

            for (int j = 0; j < originalMaterials[i].Length; j++)
            {
                Material original = originalMaterials[i][j];

                if (original == null)
                    continue;

                if (breachMaterial == null)
                {
                    Debug.LogError(
                        "BreachController: Breach Material is missing."
                    );

                    continue;
                }

                Material breach = new Material(original);

                breach.shader = breachMaterial.shader;

                if (original.HasProperty("_MainTex") &&
                    breach.HasProperty("_MainTex"))
                {
                    breach.SetTexture(
                        "_MainTex",
                        original.GetTexture("_MainTex")
                    );
                }
                else if (original.HasProperty("_BaseMap") &&
                         breach.HasProperty("_MainTex"))
                {
                    breach.SetTexture(
                        "_MainTex",
                        original.GetTexture("_BaseMap")
                    );
                }

                breachMaterials[i][j] = breach;
            }
        }
    }

    // ====================================================
    // STORE ORIGINAL LIGHTS
    // ====================================================

    private void StoreOriginalLights()
    {
        originalLightStates =
            new bool[breachLights.Length];

        originalLightIntensities =
            new float[breachLights.Length];

        originalLightColours =
            new Color[breachLights.Length];

        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null)
                continue;

            originalLightStates[i] = breachLights[i].enabled;
            originalLightIntensities[i] = breachLights[i].intensity;
            originalLightColours[i] = breachLights[i].color;
        }
    }

    // ====================================================
    // START BREACH
    // ====================================================

    public void StartBreach()
    {
        if (breachRunning)
            return;

        if (breachMaterial == null)
        {
            Debug.LogError(
                "BreachController: Breach Material is missing."
            );

            return;
        }

        breachCoroutine = StartCoroutine(BreachSequence());
    }

    private IEnumerator BreachSequence()
    {
        breachRunning = true;

        Debug.Log("BREACH STARTED");

        if (audioManager != null)
            audioManager.PlayBreach();

        ApplyBreachMaterials();
        StartBreachLights();

        if (cameraEffects != null)
        {
            cameraEffects.SetMainRoomShake();
            cameraEffects.StartBreachEffect();
        }

        yield return new WaitForSeconds(breachDuration);

        RestoreOriginalMaterials();
        RestoreLights();

        if (cameraEffects != null)
            cameraEffects.StopBreachEffect();

        breachRunning = false;
        breachCoroutine = null;

        Debug.Log("BREACH ENDED");
    }

    // ====================================================
    // APPLY BREACH MATERIALS
    // ====================================================

    private void ApplyBreachMaterials()
    {
        for (int i = 0; i < screenRenderers.Length; i++)
        {
            Renderer renderer = screenRenderers[i];

            if (renderer == null)
                continue;

            Material[] materials = breachMaterials[i];

            if (materials == null)
                continue;

            for (int j = 0; j < materials.Length; j++)
            {
                Material material = materials[j];

                if (material == null)
                    continue;

                if (material.HasProperty("_BreachAmount"))
                    material.SetFloat("_BreachAmount", breachAmount);

                if (material.HasProperty("_BreachColour"))
                    material.SetColor("_BreachColour", breachColour);

                if (material.HasProperty("_PulseSpeed"))
                    material.SetFloat("_PulseSpeed", pulseSpeed);

                if (material.HasProperty("_CorruptionStrength"))
                    material.SetFloat(
                        "_CorruptionStrength",
                        corruptionStrength
                    );

                if (material.HasProperty("_BreachLevel"))
                    material.SetFloat("_BreachLevel", breachLevel);

                if (material.HasProperty("_HeatDistortionStrength"))
                    material.SetFloat(
                        "_HeatDistortionStrength",
                        heatDistortionStrength
                    );

                if (material.HasProperty("_GlitchStrength"))
                    material.SetFloat(
                        "_GlitchStrength",
                        glitchStrength
                    );

                if (material.HasProperty("_ScanlineStrength"))
                    material.SetFloat(
                        "_ScanlineStrength",
                        scanlineStrength
                    );

                if (material.HasProperty("_CrackStrength"))
                    material.SetFloat(
                        "_CrackStrength",
                        crackStrength
                    );

                if (material.HasProperty("_StaticStrength"))
                    material.SetFloat(
                        "_StaticStrength",
                        staticStrength
                    );

                if (material.HasProperty("_EdgeGlowStrength"))
                    material.SetFloat(
                        "_EdgeGlowStrength",
                        edgeGlowStrength
                    );

                if (material.HasProperty("_PulseIntensity"))
                    material.SetFloat(
                        "_PulseIntensity",
                        pulseIntensity
                    );
            }

            renderer.materials = materials;
        }
    }

    // ====================================================
    // RESTORE ORIGINAL MATERIALS
    // ====================================================

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < screenRenderers.Length; i++)
        {
            if (screenRenderers[i] == null)
                continue;

            if (originalMaterials[i] == null)
                continue;

            screenRenderers[i].sharedMaterials =
                originalMaterials[i];
        }
    }

    // ====================================================
    // BREACH LIGHTS
    // ====================================================

    private void StartBreachLights()
    {
        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null)
                continue;

            breachLights[i].enabled = true;
            breachLights[i].color = breachLightColour;
        }
    }

    private void PulseLights()
    {
        float pulse =
            (Mathf.Sin(Time.time * lightPulseSpeed) + 1f) * 0.5f;

        float intensity =
            Mathf.Lerp(
                lightMinIntensity,
                lightMaxIntensity,
                pulse
            );

        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null)
                continue;

            breachLights[i].intensity = intensity;
            breachLights[i].color = breachLightColour;
        }
    }

    private void RestoreLights()
    {
        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null)
                continue;

            breachLights[i].enabled = originalLightStates[i];
            breachLights[i].intensity = originalLightIntensities[i];
            breachLights[i].color = originalLightColours[i];
        }
    }

    // ====================================================
    // PERMANENT BREACH LEVEL
    // ====================================================

    public void SetPermanentBreachLevel(int level)
    {
        breachLevel = Mathf.Clamp(level, 0, 4);

        ApplyLevelProfile(breachLevel);

        Debug.Log(
            "[BreachController] Permanent breach level updated: " +
            breachLevel + "/4"
        );

        if (breachRunning)
        {
            ApplyBreachMaterials();
        }
        else
        {
            StartBreach();
        }
    }
}
