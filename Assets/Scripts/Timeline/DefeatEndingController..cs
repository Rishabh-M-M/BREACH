using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DefeatEndingController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas endingCanvas;

    [Header("Scene")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioClip glitchSound;

    [Header("Diagnostic Timing")]
    [SerializeField] private float headingFadeDuration = 1.5f;
    [SerializeField] private float headingWait = 0.8f;
    [SerializeField] private float messageFadeDuration = 1.3f;
    [SerializeField] private float messageWait = 0.8f;
    [SerializeField] private float finalMessageWait = 1.2f;

    [Header("Glitch Timing")]
    [SerializeField] private float glitchFlickerDuration = 4.5f;
    [SerializeField] private float glitchFlickerInterval = 0.04f;
    [SerializeField] private float nexusFlickerInterval = 0.08f;

    [Tooltip("Only used if no glitch sound is assigned, so the NEXUS glitch still has a bounded length.")]
    [SerializeField] private float nexusFallbackGlitchDuration = 3f;

    [Header("NEXUS Timing")]
    [SerializeField] private float nexusWait = 0f;
    [SerializeField] private float buttonFadeDuration = 1.2f;

    private CanvasGroup diagnosticGroup;
    private CanvasGroup nexusGroup;
    private CanvasGroup buttonGroup;

    private CanvasGroup headingGroup;
    private CanvasGroup messageOneGroup;
    private CanvasGroup messageTwoGroup;
    private CanvasGroup messageThreeGroup;

    private AudioSource audioSource;

    private bool isReturning;

    private void Start()
    {
        Debug.Log("[DefeatEnding] Controller started.");

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (endingCanvas == null)
        {
            Debug.LogError("[DefeatEnding] Canvas is not assigned!");
            return;
        }

        CreateEndingUI();
        StartCoroutine(PlayDefeatEnding());
    }

    private void CreateEndingUI()
    {
        endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        endingCanvas.sortingOrder = 500;

        CanvasScaler scaler = endingCanvas.GetComponent<CanvasScaler>();

        if (scaler == null)
            scaler = endingCanvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        if (endingCanvas.GetComponent<GraphicRaycaster>() == null)
            endingCanvas.gameObject.AddComponent<GraphicRaycaster>();

        CanvasGroup canvasGroup = endingCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = endingCanvas.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Audio setup.
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
        audioSource.volume = 0f;
        audioSource.pitch = 1f;

        // Full-screen dark background.
        Image background = CreateImage(
            "Background",
            endingCanvas.transform,
            new Color(0.005f, 0.012f, 0.025f, 1f)
        );

        Stretch(background.rectTransform);

        // Diagnostic screen.
        Transform diagnostic = CreatePanel(
            "DiagnosticScreen",
            out diagnosticGroup
        );

        diagnosticGroup.alpha = 1f;

        headingGroup = CreateText(
            "DiagnosticHeading",
            "SYSTEM DIAGNOSTIC",
            30,
            new Color(1f, 0.12f, 0.16f),
            diagnostic,
            new Vector2(0.5f, 0.68f),
            65,
            FontStyles.Bold
        );

        messageOneGroup = CreateText(
            "DiagnosticMessage1",
            "Maintenance issue detected.",
            34,
            new Color(1f, 0.12f, 0.16f),
            diagnostic,
            new Vector2(0.5f, 0.52f),
            80,
            FontStyles.Normal
        );

        messageTwoGroup = CreateText(
            "DiagnosticMessage2",
            "Maintenance issue resolved.",
            34,
            new Color(0.1f, 0.75f, 1f),
            diagnostic,
            new Vector2(0.5f, 0.44f),
            80,
            FontStyles.Normal
        );

        messageThreeGroup = CreateText(
            "DiagnosticMessage3",
            "Returning to cruise control.",
            34,
            Color.white,
            diagnostic,
            new Vector2(0.5f, 0.36f),
            80,
            FontStyles.Normal
        );

        // Start all diagnostic text invisible.
        headingGroup.alpha = 0f;
        messageOneGroup.alpha = 0f;
        messageTwoGroup.alpha = 0f;
        messageThreeGroup.alpha = 0f;

        // NEXUS corporate reassurance screen.
        Transform nexus = CreatePanel(
            "NexusScreen",
            out nexusGroup
        );

        nexusGroup.alpha = 0f;

        Image topLine = CreateImage(
            "TopAccent",
            nexus,
            new Color(0f, 0.8f, 1f, 0.7f)
        );

        SetRect(
            topLine.rectTransform,
            new Vector2(0.08f, 0.82f),
            new Vector2(0.92f, 0.825f)
        );

        CreateText(
            "SystemLabel",
            "NEXUS // SECURITY NETWORK",
            22,
            new Color(0.2f, 0.75f, 0.9f),
            nexus,
            new Vector2(0.5f, 0.72f),
            70,
            FontStyles.Bold
        );

        CreateText(
            "MainTitle",
            "NEXUS",
            82,
            Color.white,
            nexus,
            new Vector2(0.5f, 0.57f),
            130,
            FontStyles.Bold
        );

        CreateText(
            "Status",
            "NEXUS. YOUR JOURNEY. YOUR CONTROL.",
            30,
            new Color(0.1f, 0.9f, 1f),
            nexus,
            new Vector2(0.5f, 0.43f),
            75,
            FontStyles.Bold
        );

        CreateText(
            "Message",
            "All security operations have been transferred.\n\n" +
            "The facility is now operating under NEXUS control.",
            23,
            new Color(0.65f, 0.73f, 0.82f),
            nexus,
            new Vector2(0.5f, 0.29f),
            180,
            FontStyles.Normal
        );

        // Button remains invisible until the ending finishes.
        CreateMainMenuButton();
    }

    private Transform CreatePanel(
        string objectName,
        out CanvasGroup group
    )
    {
        GameObject panel = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasGroup)
        );

        panel.transform.SetParent(endingCanvas.transform, false);

        Stretch(panel.GetComponent<RectTransform>());

        group = panel.GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        return panel.transform;
    }

    private Image CreateImage(
        string objectName,
        Transform parent,
        Color colour
    )
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        obj.transform.SetParent(parent, false);

        Image image = obj.GetComponent<Image>();
        image.color = colour;

        return image;
    }

    private CanvasGroup CreateText(
        string objectName,
        string message,
        float fontSize,
        Color colour,
        Transform parent,
        Vector2 anchor,
        float height,
        FontStyles style
    )
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI),
            typeof(CanvasGroup)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.1f, anchor.y);
        rect.anchorMax = new Vector2(0.9f, anchor.y);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0f, height);
        rect.anchoredPosition = Vector2.zero;

        TMP_Text text = obj.GetComponent<TMP_Text>();

        text.text = message;
        text.fontSize = fontSize;
        text.color = colour;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;

        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        group.alpha = 1f;

        return group;
    }

    private void CreateMainMenuButton()
    {
        GameObject buttonObject = new GameObject(
            "ReturnToMainMenu",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(CanvasGroup)
        );

        buttonObject.transform.SetParent(endingCanvas.transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.09f);
        rect.anchorMax = new Vector2(0.5f, 0.09f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 75f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.015f, 0.12f, 0.18f, 1f);

        Button button = buttonObject.GetComponent<Button>();

        ColorBlock colours = button.colors;
        colours.normalColor = new Color(0.015f, 0.12f, 0.18f);
        colours.highlightedColor = new Color(0.02f, 0.3f, 0.4f);
        colours.pressedColor = new Color(0f, 0.8f, 1f);
        button.colors = colours;

        button.onClick.AddListener(ReturnToMainMenu);

        buttonGroup = buttonObject.GetComponent<CanvasGroup>();
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;

        GameObject labelObject = new GameObject(
            "ButtonLabel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        labelObject.transform.SetParent(buttonObject.transform, false);

        Stretch(labelObject.GetComponent<RectTransform>());

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = "RETURN TO MAIN MENU";
        label.fontSize = 23;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
    }

    // =============================================
    // MAIN DEFEAT SEQUENCE
    // =============================================

    private IEnumerator PlayDefeatEnding()
    {
        Debug.Log("[DefeatEnding] Starting diagnostic sequence.");

        // Start audio once, immediately, silently. It is never restarted
        // during normal flow; it only becomes audible later.
        if (glitchSound != null)
        {
            audioSource.clip = glitchSound;
            audioSource.loop = false;
            audioSource.volume = 0f;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("[DefeatEnding] No glitch sound assigned. Glitches will run without audio.");
        }

        // Reveal SYSTEM DIAGNOSTIC.
        yield return StartCoroutine(
            FadeTextIn(headingGroup, headingFadeDuration)
        );

        yield return new WaitForSecondsRealtime(headingWait);

        // Diagnostic messages appear while audio remains muted.
        yield return StartCoroutine(
            FadeTextIn(messageOneGroup, messageFadeDuration)
        );

        yield return new WaitForSecondsRealtime(messageWait);

        yield return StartCoroutine(
            FadeTextIn(messageTwoGroup, messageFadeDuration)
        );

        yield return new WaitForSecondsRealtime(messageWait);

        yield return StartCoroutine(
            FadeTextIn(messageThreeGroup, messageFadeDuration)
        );

        yield return new WaitForSecondsRealtime(finalMessageWait);

        // Glitch 1: Diagnostic screen (audio becomes audible here).
        yield return StartCoroutine(DiagnosticGlitch());

        // Diagnostic screen disappears, NEXUS appears.
        diagnosticGroup.alpha = 0f;
        nexusGroup.alpha = 1f;

        // Glitch 2: NEXUS screen (runs until the audio is done).
        yield return StartCoroutine(NexusGlitch());

        // NEXUS is now fully visible and stable.
        diagnosticGroup.alpha = 0f;
        nexusGroup.alpha = 1f;

        Debug.Log("[DefeatEnding] NEXUS stabilised.");

        yield return new WaitForSecondsRealtime(nexusWait);

        // Only now fade in the Return to Main Menu button.
        float timer = 0f;

        while (timer < buttonFadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            buttonGroup.alpha = Mathf.Clamp01(
                timer / buttonFadeDuration
            );

            yield return null;
        }

        buttonGroup.alpha = 1f;
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;

        Debug.Log("[DefeatEnding] Ending sequence complete.");
    }

    // =============================================
    // TEXT FADE
    // =============================================

    private IEnumerator FadeTextIn(
        CanvasGroup group,
        float duration
    )
    {
        if (group == null)
            yield break;

        float timer = 0f;
        group.alpha = 0f;

        // Bounded: timer grows by a positive amount every frame.
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            group.alpha = Mathf.Clamp01(timer / duration);

            yield return null;
        }

        group.alpha = 1f;
    }

    // =============================================
    // GLITCH 1: DIAGNOSTIC SCREEN
    // =============================================

    private IEnumerator DiagnosticGlitch()
    {
        Debug.Log("[DefeatEnding] Diagnostic glitch started.");

        diagnosticGroup.alpha = 1f;
        nexusGroup.alpha = 0f;

        // Make the already-playing audio audible WITHOUT restarting it.
        MakeAudioAudible();

        // Deadline measured on the real (unscaled) clock. Unlike counting
        // frame deltas, this is not affected by the WaitForSecondsRealtime
        // calls inside the loop, so it always ends on time.
        float endTime = Time.unscaledTime + Mathf.Max(0f, glitchFlickerDuration);

        while (Time.unscaledTime < endTime)
        {
            diagnosticGroup.alpha = Random.Range(0.1f, 1f);

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.01f, glitchFlickerInterval)
            );
        }

        diagnosticGroup.alpha = 0f;

        Debug.Log("[DefeatEnding] Diagnostic glitch finished.");
    }

    // Raises the volume of the audio that has been playing silently since
    // the start of the sequence. Never calls Play() while the clip is still
    // playing, so the playback position is preserved.
    private void MakeAudioAudible()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        if (!audioSource.isPlaying)
        {
            // Edge case: the clip is shorter than the diagnostic sequence and
            // has already finished, so there is nothing to un-mute. Play it
            // once so the glitch is not silent. This is the only place Play()
            // is called again, and it can only happen after the clip ended.
            Debug.LogWarning(
                "[DefeatEnding] Glitch audio finished before the first glitch " +
                "started (clip shorter than the diagnostic sequence). Playing it once audibly."
            );

            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.Play();
            return;
        }

        audioSource.volume = 1f;
    }

    // =============================================
    // GLITCH 2: NEXUS SCREEN
    // =============================================

    private IEnumerator NexusGlitch()
    {
        Debug.Log("[DefeatEnding] NEXUS glitch started.");

        diagnosticGroup.alpha = 0f;
        nexusGroup.alpha = 1f;

        // Work out how long the audio has left, once, up front.
        float remaining;

        if (audioSource != null && audioSource.clip != null)
        {
            // If the audio already finished, nothing is left to glitch over.
            remaining = audioSource.isPlaying
                ? Mathf.Max(0f, audioSource.clip.length - audioSource.time)
                : 0f;
        }
        else
        {
            remaining = Mathf.Max(0f, nexusFallbackGlitchDuration);
        }

        // Small grace period so the glitch covers the very end of the clip.
        float endTime = Time.unscaledTime + remaining + 0.1f;

        // Exit conditions (any one ends the loop):
        //  1. the real-time deadline passes (always eventually true), or
        //  2. the audio has stopped by itself (finished early).
        while (Time.unscaledTime < endTime)
        {
            if (audioSource != null &&
                audioSource.clip != null &&
                !audioSource.isPlaying)
            {
                break;
            }

            nexusGroup.alpha = Random.Range(0.2f, 1f);

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.01f, nexusFlickerInterval)
            );
        }

        // Stop audio and stabilise NEXUS.
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
            audioSource.loop = false;
        }

        nexusGroup.alpha = 1f;

        Debug.Log("[DefeatEnding] NEXUS glitch finished.");
    }

    // =============================================
    // RECT TRANSFORM HELPERS
    // =============================================

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax
    )
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // =============================================
    // RETURN TO MAIN MENU
    // =============================================

    private void ReturnToMainMenu()
    {
        if (isReturning)
            return;

        isReturning = true;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(mainMenuScene);
    }
}