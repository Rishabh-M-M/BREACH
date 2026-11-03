
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class BossSkipIntroController : MonoBehaviour
{
    [Header("Video")]
    [Tooltip("File name, with extension, of the video inside StreamingAssets. " +
             "A plain string is used instead of a VideoClip reference: clips " +
             "that live in StreamingAssets are copied into builds as raw " +
             "files, NOT packed as assets, so the clip reference can arrive " +
             "null in a player even though it resolves fine in the editor -- " +
             "which previously left this scene on a permanent black screen.")]
    [SerializeField] private string videoFileName = "compressed-jumpscare.mp4";

    [SerializeField] private RenderTexture bossEyeRenderTexture;

    [Header("Scene")]
    [SerializeField] private string gameplayScene = "StartScene";

    [Header("Appearance")]
    [SerializeField]
    private Color backgroundColour =
        new Color(0.008f, 0.012f, 0.025f);

    [SerializeField]
    private Color accentRed =
        new Color(1f, 0.035f, 0.06f);

    [SerializeField]
    private Color cyan =
        new Color(0f, 0.85f, 1f);

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.8f;

    private Canvas canvas;

    private CanvasGroup titleGroup;
    private Transform titleRoot;

    private VideoPlayer videoPlayer;
    private GameObject videoScreen;

    private bool transitionStarted;
    private bool videoFinished;

    public static bool IsSkipToBossRun { get; private set; }

    public static void ResetSkipToBossRun()
    {
        IsSkipToBossRun = false;
        Debug.Log("[BossSkipIntro] Skip-to-boss flag reset.");
    }

    public static int BonusPower => IsSkipToBossRun ? 200 : 0;

    private void Start()
    {
        IsSkipToBossRun = false;

        if (bossEyeRenderTexture == null)
        {
            Debug.LogError(
                "[BossSkipIntro] Assign BossEyeRenderTexture!"
            );
            return;
        }

        CreateInterface();
        CreateVideoPlayer();
    }

    // --------------------------------------------------
    // CANVAS AND TITLE SCREEN
    // --------------------------------------------------

    private void CreateInterface()
    {
        GameObject canvasObject = new GameObject(
            "BossSkipIntroCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Persistent background — stays visible during the video.
        Image background = CreateImage(
            "Background",
            canvasObject.transform,
            backgroundColour
        );

        Stretch(background.rectTransform);

        // Separate title screen container.
        GameObject titleObject = new GameObject(
            "TitleScreen",
            typeof(RectTransform),
            typeof(CanvasGroup)
        );

        titleObject.transform.SetParent(canvasObject.transform, false);

        titleRoot = titleObject.transform;
        titleGroup = titleObject.GetComponent<CanvasGroup>();

        // Red atmosphere
        Image overlay = CreateImage(
            "RedAtmosphere",
            titleRoot,
            new Color(0.18f, 0.005f, 0.018f, 0.16f)
        );

        Stretch(overlay.rectTransform);

        // Decorative lines
        CreateAccentLine(
            titleRoot,
            "TopRedLine",
            new Vector2(0, 470),
            new Vector2(1100, 4)
        );

        CreateAccentLine(
            titleRoot,
            "BottomRedLine",
            new Vector2(0, -470),
            new Vector2(1100, 4)
        );

        // Header
        CreateText(
            "SYSTEM WARNING",
            titleRoot,
            22,
            accentRed,
            new Vector2(0, 390),
            new Vector2(800, 50),
            FontStyles.Bold
        );

        // Main title
        CreateText(
            "DOMINUS",
            titleRoot,
            100,
            Color.white,
            new Vector2(0, 275),
            new Vector2(1200, 130),
            FontStyles.Bold
        );

        // Subtitle
        CreateText(
            "FINAL ENCOUNTER",
            titleRoot,
            28,
            accentRed,
            new Vector2(0, 190),
            new Vector2(900, 55),
            FontStyles.Bold
        );

        // Warning panel
        Image warningPanel = CreateImage(
            "WarningPanel",
            titleRoot,
            new Color(0.12f, 0.015f, 0.025f, 0.85f)
        );

        warningPanel.rectTransform.sizeDelta =
            new Vector2(950, 220);

        warningPanel.rectTransform.anchoredPosition =
            new Vector2(0, 5);

        CreateText(
            "WARNING",
            warningPanel.transform,
            34,
            accentRed,
            new Vector2(0, 65),
            new Vector2(850, 55),
            FontStyles.Bold
        );

        CreateText(
            "You are skipping the entire game and proceeding " +
            "directly to the final boss encounter.\n\n" +
            "DOMINUS is extremely difficult to defeat. " +
            "Make sure you are prepared.",
            warningPanel.transform,
            24,
            Color.white,
            new Vector2(0, -30),
            new Vector2(850, 130),
            FontStyles.Normal
        );

        // Bonus power
        CreateText(
            "+200 POWER",
            titleRoot,
            55,
            cyan,
            new Vector2(0, -185),
            new Vector2(800, 80),
            FontStyles.Bold
        );

        CreateText(
            "ADDITIONAL POWER WILL BE GRANTED " +
            "TO HELP YOU SURVIVE",
            titleRoot,
            19,
            Color.white,
            new Vector2(0, -240),
            new Vector2(900, 50),
            FontStyles.Bold
        );

        CreateContinueButton(titleRoot);

        CreateText(
            "THERE IS NO TURNING BACK",
            titleRoot,
            16,
            new Color(1f, 0.3f, 0.3f),
            new Vector2(0, -440),
            new Vector2(800, 40),
            FontStyles.Bold
        );

        // EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule)
            );
        }
    }

    // --------------------------------------------------
    // CONTINUE BUTTON
    // --------------------------------------------------

    private void CreateContinueButton(Transform parent)
    {
        Image buttonImage = CreateImage(
            "ContinueButton",
            parent,
            new Color(0.65f, 0.015f, 0.035f)
        );

        buttonImage.rectTransform.sizeDelta =
            new Vector2(430, 75);

        buttonImage.rectTransform.anchoredPosition =
            new Vector2(0, -340);

        Button button =
            buttonImage.gameObject.AddComponent<Button>();

        ColorBlock colours = button.colors;

        colours.normalColor =
            new Color(0.65f, 0.015f, 0.035f);

        colours.highlightedColor =
            new Color(1f, 0.04f, 0.08f);

        colours.pressedColor =
            new Color(0.35f, 0.005f, 0.02f);

        colours.fadeDuration = 0.15f;

        button.colors = colours;

        CreateText(
            "CONTINUE TO DOMINUS",
            buttonImage.transform,
            25,
            Color.white,
            Vector2.zero,
            new Vector2(420, 70),
            FontStyles.Bold
        );

        button.onClick.AddListener(OnContinuePressed);
    }

    // --------------------------------------------------
    // VIDEO PLAYER AND CANVAS RAIMAGE
    // --------------------------------------------------

    private void CreateVideoPlayer()
    {
        GameObject videoObject = new GameObject(
            "BossEyeVideoPlayer"
        );

        videoPlayer = videoObject.AddComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        // Load by URL on EVERY platform, never from a VideoClip reference:
        // clips in StreamingAssets are copied to builds as raw files and their
        // asset references can be null in players. URL loading is also
        // mandatory on WebGL, where VideoPlayer cannot stream a clip out of
        // the packed .data blob anyway.
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url =
            Application.streamingAssetsPath + "/" + videoFileName;

        // Logged so the exact request URL can be pasted into a browser tab
        // when diagnosing deployment problems.
        Debug.Log("[BossSkipIntro] Video URL: " + videoPlayer.url);

        videoPlayer.renderMode =
            VideoRenderMode.RenderTexture;

        videoPlayer.targetTexture =
            bossEyeRenderTexture;

        videoPlayer.audioOutputMode =
            VideoAudioOutputMode.Direct;

        videoPlayer.waitForFirstFrame = true;

        videoPlayer.loopPointReached += OnVideoFinished;

        // A 404 or a codec the browser rejects must not leave the player on a
        // black screen: route errors through the normal finish path so the
        // scene still advances (exactly once, thanks to the videoFinished
        // flag guarding OnVideoFinished).
        videoPlayer.errorReceived += OnVideoError;

        // Create RawImage as a child of the Canvas.
        videoScreen = new GameObject(
            "BossEyeRawImage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage)
        );

        videoScreen.transform.SetParent(canvas.transform, false);

        RectTransform videoRect =
            videoScreen.GetComponent<RectTransform>();

        Stretch(videoRect);

        RawImage videoImage =
            videoScreen.GetComponent<RawImage>();

        videoImage.texture = bossEyeRenderTexture;
        videoImage.color = Color.white;
        videoImage.raycastTarget = false;

        // Keep video above all other Canvas elements.
        videoScreen.transform.SetAsLastSibling();

        // Initially hidden.
        videoScreen.SetActive(false);
    }

    // --------------------------------------------------
    // VIDEO TRANSITION
    // --------------------------------------------------

    private void OnContinuePressed()
    {
        if (transitionStarted)
            return;

        if (bossEyeRenderTexture == null)
            return;

        transitionStarted = true;

        StartCoroutine(PlayBossIntroduction());
    }

    private IEnumerator PlayBossIntroduction()
    {
        // Fade out title only.
        yield return StartCoroutine(FadeCanvas(1f, 0f));

        // Hide title, NOT the Canvas.
        titleRoot.gameObject.SetActive(false);

        // Show full-screen video.
        videoScreen.SetActive(true);

        // Clear previous frame.
        RenderTexture.active = bossEyeRenderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        // Prepare video.
        videoPlayer.Prepare();

        // A missing StreamingAssets file or a stalled network would otherwise
        // wait here forever on a black screen with no feedback.
        float prepareDeadline = Time.unscaledTime + 15f;

        while (!videoPlayer.isPrepared)
        {
            if (Time.unscaledTime >= prepareDeadline)
            {
                Debug.LogWarning(
                    "[BossSkipIntro] Video preparation timed out. " +
                    "Advancing to gameplay.");

                // Set the guard flag before loading so a late finish event
                // cannot queue a second scene load.
                videoFinished = true;
                StartCoroutine(LoadBossGameplay());
                yield break;
            }

            yield return null;
        }

        // Play video inside Canvas RawImage.
        videoPlayer.Play();

        Debug.Log(
            "[BossSkipIntro] Boss-eye video playing inside Canvas."
        );

        // Fail-safe: loopPointReached is the normal exit, but it is not
        // guaranteed to be delivered on WebGL if the stream stalls, so poll
        // the player state against a deadline derived from the clip length.
        float finishDeadline =
            Time.unscaledTime + (float)videoPlayer.length + 10f;

        while (videoPlayer.isPlaying && Time.unscaledTime < finishDeadline)
            yield return null;

        if (!videoFinished)
        {
            if (Time.unscaledTime >= finishDeadline)
            {
                Debug.LogWarning(
                    "[BossSkipIntro] Finish event overdue. " +
                    "Advancing to gameplay.");
            }

            OnVideoFinished(videoPlayer);
        }
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning(
            "[BossSkipIntro] Video error: " + message +
            " -- advancing to gameplay.");

        OnVideoFinished(source);
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        if (videoFinished)
            return;

        videoFinished = true;

        StartCoroutine(LoadBossGameplay());
    }

    // --------------------------------------------------
    // LOAD GAMEPLAY
    // --------------------------------------------------

    private IEnumerator LoadBossGameplay()
    {
        videoPlayer.Stop();

        IsSkipToBossRun = true;

        Debug.Log(
            "[BossSkipIntro] Video complete. Loading StartScene."
        );

        yield return SceneManager.LoadSceneAsync(gameplayScene);
    }

    // --------------------------------------------------
    // TITLE FADE
    // --------------------------------------------------

    private IEnumerator FadeCanvas(float from, float to)
    {
        float elapsed = 0f;

        titleGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            titleGroup.alpha = Mathf.Lerp(
                from,
                to,
                elapsed / fadeDuration
            );

            yield return null;
        }

        titleGroup.alpha = to;
    }

    // --------------------------------------------------
    // UI CREATION HELPERS
    // --------------------------------------------------

    private Image CreateImage(
        string objectName,
        Transform parent,
        Color colour)
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

    private void CreateAccentLine(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size)
    {
        Image line = CreateImage(
            objectName,
            parent,
            accentRed
        );

        line.rectTransform.sizeDelta = size;
        line.rectTransform.anchoredPosition = position;
    }

    private void CreateText(
        string content,
        Transform parent,
        float fontSize,
        Color colour,
        Vector2 position,
        Vector2 size,
        FontStyles style)
    {
        GameObject obj = new GameObject(
            "IntroText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text =
            obj.GetComponent<TextMeshProUGUI>();

        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = colour;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
