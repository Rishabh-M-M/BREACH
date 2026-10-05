using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("0 = none, 1 = low, 2 = high.")]
    [Range(0, 2)]
    public float corruptionStrength = 2f;

    [Header("Screen Setup")]
    [Tooltip("MainRoom containing Screen 1-12.")]
    public Transform screenRoot;

    [Tooltip("Material using the Custom/BreachRoom shader.")]
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

    private Renderer[] screenRenderers;

    // Store the original materials so the screens can be restored
    // after the breach effect ends.
    private Material[][] originalMaterials;

    // Separate material instances are created for the breach effect
    // so the original screen materials are not permanently changed.
    private Material[][] breachMaterials;

    private bool[] originalLightStates;
    private float[] originalLightIntensities;
    private Color[] originalLightColours;

    private bool breachRunning;

    void Start()
    {
        FindScreens();
        StoreOriginalMaterials();
        StoreOriginalLights();
    }

    void Update()
    {
#if UNITY_EDITOR
        // Manual test key kept editor-only so a shipped build can never trigger it.
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartBreach();
        }
#endif

        if (breachRunning)
        {
            PulseLights();
        }
    }

    void FindScreens()
    {
        // If MainRoom was not manually assigned, find it by name.
        if (screenRoot == null)
        {
            GameObject mainRoom = GameObject.Find("MainRoom");

            if (mainRoom != null)
            {
                screenRoot = mainRoom.transform;
            }
        }

        if (screenRoot == null)
        {
            Debug.LogError("BreachController: MainRoom / screenRoot was not found.");
            screenRenderers = new Renderer[0];
            return;
        }

        List<Renderer> foundScreens = new List<Renderer>();
        Renderer[] allRenderers = screenRoot.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null) continue;

            // Only objects named Screen 1, Screen 2, etc. are affected.
            if (renderer.gameObject.name.StartsWith("Screen "))
            {
                foundScreens.Add(renderer);
            }
        }

        screenRenderers = foundScreens.ToArray();
        Debug.Log("BreachController found " + screenRenderers.Length + " screens.");
    }

    void StoreOriginalMaterials()
    {
        originalMaterials = new Material[screenRenderers.Length][];
        breachMaterials = new Material[screenRenderers.Length][];

        for (int i = 0; i < screenRenderers.Length; i++)
        {
            Renderer renderer = screenRenderers[i];

            if (renderer == null) continue;

            // Store the original materials so they can be restored
            // after the breach effect ends.
            originalMaterials[i] = renderer.sharedMaterials;
            breachMaterials[i] = new Material[originalMaterials[i].Length];

            for (int j = 0; j < originalMaterials[i].Length; j++)
            {
                Material original = originalMaterials[i][j];

                if (original == null) continue;

                // Create a copy of the original material.
                // This keeps the original screen material unchanged.
                Material breach = new Material(original);

                // Replace the shader with the custom breach shader.
                breach.shader = breachMaterial.shader;

                // Preserve the original texture.
                if (original.HasProperty("_MainTex") && breach.HasProperty("_MainTex"))
                {
                    breach.SetTexture("_MainTex", original.GetTexture("_MainTex"));
                }
                else if (original.HasProperty("_BaseMap") && breach.HasProperty("_MainTex"))
                {
                    breach.SetTexture("_MainTex", original.GetTexture("_BaseMap"));
                }

                breachMaterials[i][j] = breach;
            }
        }
    }

    void StoreOriginalLights()
    {
        originalLightStates = new bool[breachLights.Length];
        originalLightIntensities = new float[breachLights.Length];
        originalLightColours = new Color[breachLights.Length];

        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null) continue;

            originalLightStates[i] = breachLights[i].enabled;
            originalLightIntensities[i] = breachLights[i].intensity;
            originalLightColours[i] = breachLights[i].color;
        }
    }

    public void StartBreach()
    {
        // Prevent multiple breach coroutines from running simultaneously.
        if (breachRunning) return;

        if (breachMaterial == null)
        {
            Debug.LogError("BreachController: Breach Material is missing.");
            return;
        }

        StartCoroutine(BreachSequence());
    }

    IEnumerator BreachSequence()
    {
        breachRunning = true;
        Debug.Log("BREACH STARTED");

        // Play the breach sound at the exact moment the breach effect begins.
        if (audioManager != null)
        {
            audioManager.PlayBreach();
        }

        ApplyBreachMaterials();
        StartBreachLights();

        // Start the camera shake/distortion at the same time
        if (cameraEffects != null)
        {
            cameraEffects.SetMainRoomShake();
            cameraEffects.StartBreachEffect();
        }

        yield return new WaitForSeconds(breachDuration);

        // Restore everything after the breach duration.
        RestoreOriginalMaterials();
        RestoreLights();

        if (cameraEffects != null)
        {
            cameraEffects.StopBreachEffect();
        }

        breachRunning = false;
        Debug.Log("BREACH ENDED");
    }

    void ApplyBreachMaterials()
    {
        for (int i = 0; i < screenRenderers.Length; i++)
        {
            Renderer renderer = screenRenderers[i];
            if (renderer == null) continue;

            Material[] materials = breachMaterials[i];
            if (materials == null) continue;

            for (int j = 0; j < materials.Length; j++)
            {
                Material material = materials[j];
                if (material == null) continue;

                // Apply the values from the BreachController Inspector.
                if (material.HasProperty("_BreachAmount"))
                    material.SetFloat("_BreachAmount", breachAmount);

                if (material.HasProperty("_BreachColour"))
                    material.SetColor("_BreachColour", breachColour);

                if (material.HasProperty("_PulseSpeed"))
                    material.SetFloat("_PulseSpeed", pulseSpeed);

                if (material.HasProperty("_CorruptionStrength"))
                    material.SetFloat("_CorruptionStrength", corruptionStrength);
            }

            renderer.materials = materials;
            Debug.Log("Breach material applied to: " + renderer.gameObject.name);
        }
    }

    void RestoreOriginalMaterials()
    {
        for (int i = 0; i < screenRenderers.Length; i++)
        {
            if (screenRenderers[i] == null) continue;
            if (originalMaterials[i] == null) continue;

            rendererRestore(screenRenderers[i], originalMaterials[i]);
        }
    }

    // Restores the exact original material array.
    private void rendererRestore(Renderer renderer, Material[] materials)
    {
        renderer.sharedMaterials = materials;
    }

    void StartBreachLights()
    {
        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null) continue;

            breachLights[i].enabled = true;
            breachLights[i].color = breachLightColour;
        }
    }

    void PulseLights()
    {
        // Convert the sine wave into a 0-1 value.
        float pulse = (Mathf.Sin(Time.time * lightPulseSpeed) + 1f) * 0.5f;

        // Use that value to smoothly vary the light intensity.
        float intensity = Mathf.Lerp(lightMinIntensity, lightMaxIntensity, pulse);

        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null) continue;

            breachLights[i].intensity = intensity;
            breachLights[i].color = breachLightColour;
        }
    }

    void RestoreLights()
    {
        for (int i = 0; i < breachLights.Length; i++)
        {
            if (breachLights[i] == null) continue;

            breachLights[i].enabled = originalLightStates[i];
            breachLights[i].intensity = originalLightIntensities[i];
            breachLights[i].color = originalLightColours[i];
        }
    }
}