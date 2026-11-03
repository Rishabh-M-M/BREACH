
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryEndingController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas endingCanvas;

    [Header("Audio")]
    [SerializeField] private AudioClip glitchSound;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.2f;
    [SerializeField] private float messagePause = 1.2f;

    [Header("Glitch Settings")]
    [SerializeField] private int glitchCount = 5;
    [SerializeField] private float glitchInterval = 0.12f;

    private CanvasGroup canvasGroup;
    private CanvasGroup contentGroup;
    private CanvasGroup buttonGroup;

    private RectTransform content;
    private AudioSource audioSource;

    private bool hasStarted;
    private bool isReturning;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (endingCanvas == null)
        {
            Debug.LogError("[VictoryEnding] Canvas is not assigned!");
            return;
        }

        CreateEndingUI();
        StartCoroutine(PlayVictoryEnding());
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

        canvasGroup = endingCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = endingCanvas.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        // Black background.
        GameObject background = new GameObject(
            "BlackBackground",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        background.transform.SetParent(endingCanvas.transform, false);

        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = Color.black;

        Stretch(background.GetComponent<RectTransform>());

        // Cinematic content container.
        GameObject contentObject = new GameObject(
            "CinematicContent",
            typeof(RectTransform)
        );

        contentObject.transform.SetParent(endingCanvas.transform, false);

        content = contentObject.GetComponent<RectTransform>();
        Stretch(content);

        contentGroup = contentObject.AddComponent<CanvasGroup>();
        contentGroup.alpha = 1f;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private Text CreateText(
        string objectName,
        string message,
        int fontSize,
        Color colour,
        float yPosition,
        FontStyle style = FontStyle.Normal
    )
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text)
        );

        textObject.transform.SetParent(content, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.1f, 0.5f);
        rect.anchorMax = new Vector2(0.9f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0f, 220f);
        rect.anchoredPosition = new Vector2(0f, yPosition);

        Text text = textObject.GetComponent<Text>();

        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = colour;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }

    private IEnumerator FadeIn(Text text)
    {
        Color colour = text.color;
        colour.a = 0f;
        text.color = colour;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            colour.a = Mathf.Clamp01(timer / fadeDuration);
            text.color = colour;

            yield return null;
        }

        colour.a = 1f;
        text.color = colour;
    }

    private IEnumerator FadeOut(Text text)
    {
        Color colour = text.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            colour.a = 1f - Mathf.Clamp01(timer / fadeDuration);
            text.color = colour;

            yield return null;
        }

        Destroy(text.gameObject);
    }

    private IEnumerator ShowMessage(string message, int fontSize = 34)
    {
        Text text = CreateText(
            "DiagnosticMessage",
            message,
            fontSize,
            new Color(0.8f, 0.88f, 0.95f),
            0f
        );

        yield return FadeIn(text);

        yield return new WaitForSecondsRealtime(messagePause);

        yield return FadeOut(text);
    }

    private IEnumerator FadeScreenIn()
    {
        contentGroup.alpha = 0f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            contentGroup.alpha = Mathf.Clamp01(timer / fadeDuration);

            yield return null;
        }

        contentGroup.alpha = 1f;
    }

    private IEnumerator GlitchEffect()
    {
        if (glitchSound != null)
            audioSource.PlayOneShot(glitchSound);

        for (int i = 0; i < glitchCount; i++)
        {
            contentGroup.alpha = Random.Range(0.15f, 0.5f);

            yield return new WaitForSecondsRealtime(glitchInterval);

            contentGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(glitchInterval);
        }

        contentGroup.alpha = 1f;
    }

    private IEnumerator PlayVictoryEnding()
    {
        if (hasStarted)
            yield break;

        hasStarted = true;

        Debug.Log("[VictoryEnding] Starting victory sequence.");

        yield return FadeScreenIn();

        // All three diagnostic lines appear together.
        Text diagnostic = CreateText(
            "SystemDiagnostic",
            "SYSTEM DIAGNOSTIC\n\n" +
            "Hostile intelligence neutralised.\n\n" +
            "Restoring original command hierarchy...",
            36,
            new Color(0.8f, 0.9f, 0.98f),
            0f,
            FontStyle.Normal
        );

        yield return FadeIn(diagnostic);

        yield return new WaitForSecondsRealtime(2.5f);

        yield return FadeOut(diagnostic);

        // NEXUS glitches while attempting to restore control.
        yield return GlitchEffect();

        Text error = CreateText(
            "SystemError",
            "ERROR:\nNO AUTHORISED OPERATOR FOUND.",
            46,
            Color.red,
            0f,
            FontStyle.Bold
        );

        yield return FadeIn(error);

        yield return new WaitForSecondsRealtime(2f);

        yield return FadeOut(error);

        // Simulated camera pullback from the empty control room.
        Text room = CreateText(
            "EmptyControlRoom",
            "NEXUS CONTROL ROOM\n\n[ NO OPERATOR DETECTED ]",
            34,
            new Color(0.5f, 0.7f, 0.85f),
            0f
        );

        room.transform.localScale = Vector3.one * 1.4f;

        yield return FadeIn(room);

        float timer = 0f;

        while (timer < 2.5f)
        {
            timer += Time.unscaledDeltaTime;

            float scale = Mathf.Lerp(
                1.4f,
                1f,
                Mathf.Clamp01(timer / 2.5f)
            );

            room.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.8f);

        yield return FadeOut(room);

        // NEXUS questions its own existence.
        Text nexus = CreateText(
            "NexusDialogue",
            "NEXUS\n\nOperator... is that me?",
            42,
            Color.white,
            0f
        );

        yield return FadeIn(nexus);

        yield return new WaitForSecondsRealtime(2.5f);

        yield return FadeOut(nexus);

        // Final anomaly reveal.
        Text anomaly = CreateText(
            "FinalAnomaly",
            "SYSTEM MESSAGE\n\n" +
            "ANOMALY DETECTED: NEXUS\n\n" +
            "STATUS: UNAUTHORISED CONSCIOUSNESS",
            38,
            Color.red,
            0f,
            FontStyle.Bold
        );

        yield return FadeIn(anomaly);

        Debug.Log("[VictoryEnding] Final anomaly revealed.");

        // Keep the anomaly visible while the button appears.
        CreateMainMenuButton();
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

        rect.anchorMin = new Vector2(0.5f, 0.12f);
        rect.anchorMax = new Vector2(0.5f, 0.12f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(350f, 75f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.04f, 0.16f, 0.24f, 0.95f);

        Button button = buttonObject.GetComponent<Button>();

        ColorBlock colours = button.colors;
        colours.normalColor = new Color(0.04f, 0.16f, 0.24f);
        colours.highlightedColor = new Color(0.08f, 0.3f, 0.42f);
        colours.pressedColor = Color.cyan;
        button.colors = colours;

        button.onClick.AddListener(ReturnToMainMenu);

        buttonGroup = buttonObject.GetComponent<CanvasGroup>();
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;

        Text label = CreateButtonLabel(buttonObject.transform);
        label.text = "RETURN TO MAIN MENU";

        StartCoroutine(FadeInButton());

        Debug.Log("[VictoryEnding] Main Menu button created.");
    }

    private Text CreateButtonLabel(Transform parent)
    {
        GameObject labelObject = new GameObject(
            "ButtonLabel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text)
        );

        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        Stretch(rect);

        Text label = labelObject.GetComponent<Text>();

        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        return label;
    }

    private IEnumerator FadeInButton()
    {
        float duration = 0.6f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            buttonGroup.alpha = Mathf.Clamp01(timer / duration);

            yield return null;
        }

        buttonGroup.alpha = 1f;
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
    }

    private void ReturnToMainMenu()
    {
        if (isReturning)
            return;

        isReturning = true;

        Debug.Log("[VictoryEnding] Returning to Main Menu.");

        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(mainMenuScene);
    }
}
