using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using ProjectBreach.Core;

/// <summary>
/// Plays the NEXUS advertisement video, then advances to the next scene.
///
/// WebGL hardening: on WebGL this scene used to hang on a permanent black
/// screen whenever the video never finished, because the only exit was the
/// loopPointReached event. Three independent failure modes can stop that
/// event from ever firing in a browser:
///   1. The StreamingAssets URL never gets requested or 404s.
///   2. The browser's autoplay policy rejects unmuted playback, because the
///      user gesture from the START button does not survive the scene load.
///   3. The stream stalls, so loopPointReached is never delivered.
/// The watchdogs below guarantee the scene always advances eventually.
/// </summary>
public class OpeningCutsceneController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Video File (inside StreamingAssets)")]
    [Tooltip("File name, with extension, of the video inside StreamingAssets. " +
             "A plain string is used instead of relying on the VideoClip " +
             "reference: clips that live in StreamingAssets are copied into " +
             "builds as raw files, NOT packed as assets, so the clip " +
             "reference can arrive null in a player even though it resolves " +
             "fine in the editor -- which previously left the URL empty and " +
             "the browser never even requested the file.")]
    [SerializeField] private string videoFileName = "compressed-StartScene-3.mp4";

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "StartScene";

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("WebGL Playback Safety Net")]
    [Tooltip("Seconds to wait for playback to actually begin before retrying muted.")]
    [SerializeField] private float playbackStartTimeout = 3f;

    [Tooltip("Grace period during which skip input is ignored, so the click that " +
             "pressed START cannot accidentally skip the cutscene.")]
    [SerializeField] private float skipInputGrace = 0.5f;

    [Tooltip("Extra seconds on top of the reported clip length before the cutscene " +
             "is force-skipped when the finish event never arrives.")]
    [SerializeField] private float finishTimeoutMargin = 10f;

    [Tooltip("Absolute maximum seconds this scene may run before force-advancing, " +
             "no matter what state the video is in.")]
    [SerializeField] private float hardCapSeconds = 120f;

    // Set to true when the advertisement is played after Ending A.
    public static bool PlayAsEndingAdvertisement = false;

    private bool hasTransitioned = false;

    // -1 until the VideoPlayer reports isPlaying; stored as scene time so the
    // finish fail-safe can be measured from the real playback start rather
    // than from scene load (slow networks spend a while preparing first).
    private float playbackStartedAt = -1f;

    private bool mutedRetryDone = false;
    private float sceneElapsed = 0f;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            // Load by URL on EVERY platform, never from a VideoClip reference.
            // Besides the null-reference trap described on videoFileName, this
            // also keeps editor and build behaviour identical, and it is
            // mandatory on WebGL where VideoPlayer cannot stream a clip out of
            // the packed .data blob anyway.
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url =
                Application.streamingAssetsPath + "/" + videoFileName;

            // Logged so the exact request URL can be pasted into a browser
            // tab when diagnosing deployment problems.
            Debug.Log("[OpeningCutscene] Video URL: " + videoPlayer.url);
        }
    }

    private void Start()
    {
        // Do not rely on PlayOnAwake alone: explicitly request playback so the
        // browser actually starts fetching the file. If the autoplay policy
        // rejects this (unmuted, outside a user gesture), the Update watchdog
        // retries muted a few seconds later.
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }

    private void OnEnable()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.loopPointReached += OnVideoFinished;

        // Without an error handler, a 404 or a codec the browser rejects would
        // leave the scene on a permanent black screen, because loopPointReached
        // never fires for a video that never plays.
        videoPlayer.errorReceived += OnVideoError;
    }

    private void OnDisable()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }

    private void Update()
    {
        if (hasTransitioned)
        {
            return;
        }

        // Unscaled time so a frozen timeScale can never freeze the watchdogs.
        sceneElapsed += Time.unscaledDeltaTime;

        // Let the player skip with any key or mouse click. The grace window
        // prevents the START button click itself (or a double-click during the
        // scene swap) from instantly skipping the ad.
        if (sceneElapsed >= skipInputGrace && Input.anyKeyDown)
        {
            Debug.Log("[OpeningCutscene] Skipped by player input.");
            Transition();
            return;
        }

        if (playbackStartedAt < 0f)
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                playbackStartedAt = sceneElapsed;
                Debug.Log("[OpeningCutscene] Playback started.");
            }
            else if (!mutedRetryDone && sceneElapsed >= playbackStartTimeout)
            {
                // Browsers only block UNMUTED autoplay outside a user gesture,
                // and the START click's gesture does not survive the scene load
                // on WebGL, so the first play attempt is silently rejected
                // there. Muted autoplay is always allowed, so retry with the
                // direct audio track muted -- a silent ad beats a black screen.
                mutedRetryDone = true;
                Debug.LogWarning(
                    "[OpeningCutscene] Video did not start within " +
                    playbackStartTimeout +
                    "s. Retrying muted (browser autoplay policy).");

                if (videoPlayer != null)
                {
                    // Valid because this scene's VideoPlayer uses Direct audio
                    // mode; would only log a warning if that mode is changed.
                    videoPlayer.SetDirectAudioMute(0, true);
                    videoPlayer.Play();
                }
            }
            else if (mutedRetryDone && sceneElapsed >= playbackStartTimeout * 2f)
            {
                Debug.LogWarning(
                    "[OpeningCutscene] Muted retry also failed. " +
                    "Advancing to the next scene.");
                Transition();
            }
        }
        else if (videoPlayer != null && videoPlayer.isPrepared &&
                 videoPlayer.length > 0.0 &&
                 sceneElapsed >= playbackStartedAt + (float)videoPlayer.length +
                                 finishTimeoutMargin)
        {
            // loopPointReached is not guaranteed to be delivered on WebGL when
            // the stream stalls near the end, so it must not be the only exit.
            Debug.LogWarning(
                "[OpeningCutscene] Finish event overdue. Force-advancing.");
            Transition();
        }

        // Absolute fail-safe for every remaining case (never prepared, network
        // stall before the first frame, etc.).
        if (sceneElapsed >= hardCapSeconds)
        {
            Debug.LogWarning("[OpeningCutscene] Hard cap reached. Force-advancing.");
            Transition();
        }
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        Debug.Log("[OpeningCutscene] Video finished.");
        Transition();
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        Debug.LogWarning(
            "[OpeningCutscene] Video error: " + message +
            " -- advancing to the next scene.");
        Transition();
    }

    private void Transition()
    {
        if (hasTransitioned)
        {
            return;
        }

        hasTransitioned = true;

        string targetScene;

        if (PlayAsEndingAdvertisement)
        {
            PlayAsEndingAdvertisement = false;
            Debug.Log("[OpeningCutscene] Ending A advertisement finished. Returning to main menu...");
            targetScene = mainMenuSceneName;
        }
        else
        {
            Debug.Log("[OpeningCutscene] NEXUS advertisement finished. Loading gameplay...");
            targetScene = nextSceneName;
        }

        // A missing Build Settings entry also fails as a silent black screen,
        // so refuse loudly instead of calling LoadScene blind.
        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogError(
                "[OpeningCutscene] Scene '" + targetScene + "' can't be loaded. " +
                "Check the name and File > Build Settings.");
            return;
        }

        SceneManager.LoadScene(targetScene);
    }
}
