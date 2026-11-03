
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// Plays the tutorial video, then returns to the main menu.
///
/// WebGL hardening: this scene previously had no skip button wired up and no
/// error handling, so any video failure (StreamingAssets 404, browser
/// autoplay policy rejecting unmuted playback, stalled stream) left the
/// player on a permanent black screen. The watchdogs below guarantee the
/// scene always returns to the menu eventually, and any key/click skips.
/// </summary>
public class TutorialSceneController : MonoBehaviour
{
    [Header("Tutorial Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Video File (inside StreamingAssets)")]
    [Tooltip("File name, with extension, of the video inside StreamingAssets. " +
             "A plain string is used instead of relying on the VideoClip " +
             "reference: clips that live in StreamingAssets are copied into " +
             "builds as raw files, NOT packed as assets, so the clip " +
             "reference can arrive null in a player even though it resolves " +
             "fine in the editor.")]
    [SerializeField] private string videoFileName = "compressed-Tutorial-3.mp4";

    [Header("WebGL Playback Safety Net")]
    [Tooltip("Seconds to wait for playback to actually begin before retrying muted.")]
    [SerializeField] private float playbackStartTimeout = 3f;

    [Tooltip("Grace period during which skip input is ignored, so the click that " +
             "opened the tutorial cannot accidentally skip it.")]
    [SerializeField] private float skipInputGrace = 0.5f;

    [Tooltip("Extra seconds on top of the reported clip length before the scene " +
             "is exited when the finish event never arrives.")]
    [SerializeField] private float finishTimeoutMargin = 10f;

    [Tooltip("Absolute maximum seconds this scene may run before returning to " +
             "the menu, no matter what state the video is in. Must exceed the " +
             "tutorial clip length (~116s).")]
    [SerializeField] private float hardCapSeconds = 150f;

    private bool returningToMenu = false;

    // -1 until the VideoPlayer reports isPlaying; measured against scene time
    // so the finish fail-safe starts from real playback, not from scene load.
    private float playbackStartedAt = -1f;

    private bool mutedRetryDone = false;
    private float sceneElapsed = 0f;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (videoPlayer == null)
        {
            Debug.LogError("[Tutorial] Video Player is not assigned!");
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        // Load by URL on EVERY platform, never from a VideoClip reference:
        // clips in StreamingAssets are copied to builds as raw files and their
        // asset references can be null in players, which silently disabled the
        // old WebGL-only URL setup (the browser never even requested the
        // file). URL loading is also mandatory on WebGL, where VideoPlayer
        // cannot stream a clip out of the packed .data blob anyway.
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Application.streamingAssetsPath + "/" + videoFileName;

        // Logged so the exact request URL can be pasted into a browser tab
        // when diagnosing deployment problems.
        Debug.Log("[Tutorial] Video URL: " + videoPlayer.url);

        videoPlayer.loopPointReached += OnVideoFinished;

        // Without an error handler, a 404 or a codec the browser rejects would
        // hang this scene forever (loopPointReached never fires for a video
        // that never plays).
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.Play();

        Debug.Log("[Tutorial] Tutorial video started.");
    }

    private void Update()
    {
        if (returningToMenu || videoPlayer == null)
        {
            return;
        }

        // Unscaled time so a frozen timeScale can never freeze the watchdogs.
        sceneElapsed += Time.unscaledDeltaTime;

        // Any key or click skips the tutorial; the grace window stops the
        // opening click from skipping it accidentally.
        if (sceneElapsed >= skipInputGrace && Input.anyKeyDown)
        {
            Debug.Log("[Tutorial] Skipped by player input.");
            ReturnToMainMenu();
            return;
        }

        if (playbackStartedAt < 0f)
        {
            if (videoPlayer.isPlaying)
            {
                playbackStartedAt = sceneElapsed;
            }
            else if (!mutedRetryDone && sceneElapsed >= playbackStartTimeout)
            {
                // Browsers block UNMUTED autoplay outside a user gesture, and
                // the opening click's gesture does not survive the scene load
                // on WebGL. Muted autoplay is always allowed, so retry muted --
                // a silent tutorial beats a black screen.
                mutedRetryDone = true;
                Debug.LogWarning(
                    "[Tutorial] Video did not start within " +
                    playbackStartTimeout +
                    "s. Retrying muted (browser autoplay policy).");

                // Valid while the VideoPlayer uses Direct audio mode; would
                // only log a warning if that mode is ever changed.
                videoPlayer.SetDirectAudioMute(0, true);
                videoPlayer.Play();
            }
            else if (mutedRetryDone && sceneElapsed >= playbackStartTimeout * 2f)
            {
                Debug.LogWarning(
                    "[Tutorial] Muted retry also failed. Returning to menu.");
                ReturnToMainMenu();
            }
        }
        else if (videoPlayer.isPrepared && videoPlayer.length > 0.0 &&
                 sceneElapsed >= playbackStartedAt + (float)videoPlayer.length +
                                 finishTimeoutMargin)
        {
            // loopPointReached is not guaranteed on WebGL when the stream
            // stalls near the end, so it must not be the only exit.
            Debug.LogWarning("[Tutorial] Finish event overdue. Returning to menu.");
            ReturnToMainMenu();
        }

        // Absolute fail-safe for every remaining case (never prepared, network
        // stall before the first frame, etc.).
        if (sceneElapsed >= hardCapSeconds)
        {
            Debug.LogWarning("[Tutorial] Hard cap reached. Returning to menu.");
            ReturnToMainMenu();
        }
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        ReturnToMainMenu();
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        Debug.LogWarning(
            "[Tutorial] Video error: " + message + " -- returning to menu.");
        ReturnToMainMenu();
    }

    public void SkipTutorial()
    {
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        if (returningToMenu)
            return;

        returningToMenu = true;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Debug.Log("[Tutorial] Returning to MainMenu.");

        // A missing Build Settings entry fails as a silent black screen too,
        // so refuse loudly instead of calling LoadScene blind.
        if (!Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            Debug.LogError(
                "[Tutorial] Scene 'MainMenu' can't be loaded. " +
                "Check the name and File > Build Settings.");
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }
}
