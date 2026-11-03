
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

    [Header("Permanent Breach Level")]
    [Tooltip("0 = no breach, 1-4 = permanently fallen rooms.")]
    [Range(0, 4)]
    public int breachLevel = 0;

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
    }

    private void Update()
    {

        if (breachRunning)
        {
            PulseLights();
        }
    }

    // ====================================================
    // TEST LEVEL CONTROL
    // ====================================================

    private void SetTestBreachLevel(int level)
    {
        breachLevel = Mathf.Clamp(level, 1, 4);

        Debug.Log(
            "[BreachController] TEST BREACH LEVEL: "
            + breachLevel
        );

        if (breachRunning)
        {
            // Update the existing effect immediately.
            ApplyBreachMaterials();
        }
        else
        {
            // Start the temporary breach effect.
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
            GameObject mainRoom =
                GameObject.Find("MainRoom");

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

        List<Renderer> foundScreens =
            new List<Renderer>();

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
            "BreachController found "
            + screenRenderers.Length
            + " screens."
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

            originalMaterials[i] =
                renderer.sharedMaterials;

            breachMaterials[i] =
                new Material[originalMaterials[i].Length];

            for (int j = 0; j < originalMaterials[i].Length; j++)
            {
                Material original =
                    originalMaterials[i][j];

                if (original == null)
                    continue;

                if (breachMaterial == null)
                {
                    Debug.LogError(
                        "BreachController: Breach Material is missing."
                    );

                    continue;
                }

                Material breach =
                    new Material(original);

                breach.shader = breachMaterial.shader;

                // Preserve original texture.
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

            originalLightStates[i] =
                breachLights[i].enabled;

            originalLightIntensities[i] =
                breachLights[i].intensity;

            originalLightColours[i] =
                breachLights[i].color;
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
                    material.SetFloat(
                        "_BreachAmount",
                        breachAmount
                    );

                if (material.HasProperty("_BreachColour"))
                    material.SetColor(
                        "_BreachColour",
                        breachColour
                    );

                if (material.HasProperty("_PulseSpeed"))
                    material.SetFloat(
                        "_PulseSpeed",
                        pulseSpeed
                    );

                if (material.HasProperty("_CorruptionStrength"))
                    material.SetFloat(
                        "_CorruptionStrength",
                        corruptionStrength
                    );

                if (material.HasProperty("_BreachLevel"))
                    material.SetFloat(
                        "_BreachLevel",
                        breachLevel
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

            breachLights[i].enabled =
                originalLightStates[i];

            breachLights[i].intensity =
                originalLightIntensities[i];

            breachLights[i].color =
                originalLightColours[i];
        }
    }


    // ====================================================
    // PERMANENT BREACH LEVEL
    // ====================================================

    public void SetPermanentBreachLevel(int level)
    {
        breachLevel = Mathf.Clamp(level, 0, 4);

        Debug.Log(
            "[BreachController] Permanent breach level updated: "
            + breachLevel + "/4"
        );

        if (breachRunning)
        {
            // Update the currently running shader effect immediately.
            ApplyBreachMaterials();
        }
        else
        {
            // Start the breach effect using the new level.
            StartBreach();
        }
    }

}
