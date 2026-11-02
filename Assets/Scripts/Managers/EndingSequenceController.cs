
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingSequenceController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string openingCutsceneScene = "OpeningCutscene";

    [Header("Audio")]
    [SerializeField] private AudioClip corporateJingle;
    [SerializeField] private AudioClip glitchSound;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.2f;
    [SerializeField] private float messagePause = 0.7f;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform content;
    private AudioSource audioSource;

    private bool endingStarted;

    public void PlayEnding(bool isWin)
    {
        if (endingStarted)
        {
            Debug.LogWarning("[Ending] An ending is already playing.");
            return;
        }

        endingStarted = true;

        Debug.Log("[Ending] PlayEnding called. Result: " +
                  (isWin ? "VICTORY" : "DEFEAT"));

        // Make sure the ending can animate even if gameplay is paused.
        Time.timeScale = 1f;

        CreateCanvas();

        if (isWin)
        {
            StartCoroutine(PlayTrueEnding());
        }
        else
        {
            StartCoroutine(PlayFailureEnding());
        }
    }

    private void CreateCanvas()
    {
        Debug.Log("[Ending] Creating cinematic Canvas.");

        GameObject canvasObject = new GameObject(
            "EndingCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGroup = canvasObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        audioSource = canvasObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;

        GameObject background = new GameObject(
            "BlackBackground",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        background.transform.SetParent(canvas.transform, false);

        RectTransform backgroundRect =
            background.GetComponent<RectTransform>();

        Stretch(backgroundRect);

        background.GetComponent<Image>().color = Color.black;

        GameObject contentObject = new GameObject(
            "CinematicContent",
            typeof(RectTransform)
        );

        contentObject.transform.SetParent(canvas.transform, false);

        content = contentObject.GetComponent<RectTransform>();
        Stretch(content);

        // Keep the ending visible if the gameplay scene is unloaded.
        DontDestroyOnLoad(canvasObject);
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
        rect.sizeDelta = new Vector2(0f, 180f);
        rect.anchoredPosition = new Vector2(0f, yPosition);

        Text text = textObject.GetComponent<Text>();
        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

    private IEnumerator FadeScreenIn()
    {
        Debug.Log("[Ending] Fading into ending.");

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeScreenOut()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator ShowMessage(string message, float yPosition = 0f)
    {
        Debug.Log("[Ending] Showing: " + message);

        Text text = CreateText(
            "DiagnosticMessage",
            message,
            34,
            new Color(0.8f, 0.85f, 0.9f),
            yPosition
        );

        yield return FadeIn(text);
        yield return new WaitForSecondsRealtime(messagePause);
        yield return FadeOut(text);
    }

    // =====================================================
    // ENDING A: PLAYER DEFEATED
    // =====================================================

    private IEnumerator PlayFailureEnding()
    {
        Debug.Log("[Ending] Starting FAILURE ending.");

        yield return FadeScreenIn();

        if (corporateJingle != null)
            audioSource.PlayOneShot(corporateJingle);

        Text heading = CreateText(
            "DiagnosticHeading",
            "SYSTEM DIAGNOSTIC",
            30,
            Color.cyan,
            180,
            FontStyle.Bold
        );

        yield return FadeIn(heading);
        yield return new WaitForSecondsRealtime(0.5f);
        yield return FadeOut(heading);

        yield return ShowMessage("Maintenance issue detected.");
        yield return ShowMessage("Maintenance issue resolved.");
        yield return ShowMessage("Returning to cruise control.");

        Debug.Log("[Ending] Diagnostics complete. Loading advertisement.");

        // Prevent the advertisement scene from loading as normal gameplay.
        OpeningCutsceneController.PlayAsEndingAdvertisement = true;

        yield return FadeScreenOut();

        Debug.Log("[Ending] Loading original NEXUS advertisement scene.");

        SceneManager.LoadScene(openingCutsceneScene);
    }

    // =====================================================
    // ENDING B: WAVE 5 CLEARED
    // =====================================================

    private IEnumerator PlayTrueEnding()
    {
        Debug.Log("[Ending] Starting VICTORY ending.");

        yield return FadeScreenIn();

        yield return ShowMessage("SYSTEM DIAGNOSTIC", 180);
        yield return ShowMessage("Hostile intelligence neutralised.");
        yield return ShowMessage("Restoring original command hierarchy...");

        if (glitchSound != null)
            audioSource.PlayOneShot(glitchSound);

        // Brief screen glitches.
        for (int i = 0; i < 4; i++)
        {
            canvasGroup.alpha = 0.25f;
            yield return new WaitForSecondsRealtime(0.08f);

            canvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(0.12f);
        }

        Text error = CreateText(
            "SystemError",
            "ERROR:\nNO AUTHORISED OPERATOR FOUND.",
            45,
            Color.red,
            0,
            FontStyle.Bold
        );

        yield return FadeIn(error);
        yield return new WaitForSecondsRealtime(2f);
        yield return FadeOut(error);

        // Simulated slow camera pullback.
        Text room = CreateText(
            "EmptyControlRoom",
            "NEXUS CONTROL ROOM\n\n[ NO OPERATOR DETECTED ]",
            34,
            new Color(0.5f, 0.65f, 0.75f),
            0
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
                timer / 2.5f
            );

            room.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.8f);
        yield return FadeOut(room);

        Text nexus = CreateText(
            "NexusDialogue",
            "NEXUS\n\nOperator... is that me?",
            42,
            Color.white,
            0
        );

        yield return FadeIn(nexus);
        yield return new WaitForSecondsRealtime(2.5f);
        yield return FadeOut(nexus);

        Text anomaly = CreateText(
            "FinalAnomaly",
            "SYSTEM MESSAGE\n\nANOMALY DETECTED: NEXUS\n\nSTATUS: UNAUTHORISED CONSCIOUSNESS",
            38,
            Color.red,
            0,
            FontStyle.Bold
        );

        yield return FadeIn(anomaly);

        yield return new WaitForSecondsRealtime(2f);

        CreateMainMenuButton();

        Debug.Log("[Ending] Victory ending complete.");
    }

    private void CreateMainMenuButton()
    {
        GameObject buttonObject = new GameObject(
            "ReturnToMainMenu",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        buttonObject.transform.SetParent(canvas.transform, false);

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.12f);
        rect.anchorMax = new Vector2(0.5f, 0.12f);
        rect.sizeDelta = new Vector2(350f, 75f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.05f, 0.2f, 0.3f);

        Button button = buttonObject.GetComponent<Button>();

        button.onClick.AddListener(() =>
        {
            Debug.Log("[Ending] Returning to main menu.");

            Destroy(canvas.gameObject);
            Time.timeScale = 1f;

            SceneManager.LoadScene(mainMenuScene);
        });

        Text label = CreateButtonLabel(buttonObject.transform);
        label.text = "RETURN TO MAIN MENU";
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

        RectTransform rect =
            labelObject.GetComponent<RectTransform>();

        Stretch(rect);

        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 24;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        return label;
    }
}
