
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Plays the DOMINUS boss-eye cinematic as a full-screen overlay during
/// gameplay, and blocks the calling coroutine until it is over.
///
/// WebGL hardening: the video is loaded by URL from StreamingAssets on every
/// platform, because a VideoClip reference to a file inside StreamingAssets
/// can arrive null in builds (raw copy, not packed). The coroutine also has
/// prepare/finish watchdogs so a 404, a rejected playback attempt or a
/// stalled stream can never stall the wave logic that awaits PlayAndWait().
/// </summary>
public class BossWaveIntroOverlay : MonoBehaviour
{
    [Header("Video")]
    [Tooltip("File name, with extension, of the video inside StreamingAssets. " +
             "A plain string is used instead of a VideoClip reference: clips " +
             "that live in StreamingAssets are copied into builds as raw " +
             "files, NOT packed as assets, so the clip reference can arrive " +
             "null in a player even though it resolves fine in the editor.")]
    [SerializeField] private string videoFileName = "compressed-jumpscare.mp4";

    [SerializeField] private RenderTexture bossEyeRenderTexture;

    [Header("Playback Safety Net")]
    [Tooltip("Seconds to wait for the video to become prepared before giving " +
             "up; without this a 404 or a stalled network hangs the wave " +
             "logic forever, because the coroutine would never return.")]
    [SerializeField] private float prepareTimeout = 15f;

    [Tooltip("Extra seconds on top of the reported clip length before the " +
             "cinematic is force-finished when the finish event never arrives.")]
    [SerializeField] private float finishTimeoutMargin = 10f;

    private VideoPlayer videoPlayer;
    private GameObject videoScreen;
    private bool isPlaying;

    // Set by EITHER loopPointReached or errorReceived, so a failed video
    // releases the awaiting coroutine exactly like a finished one would.
    private bool playbackEnded;

    public IEnumerator PlayAndWait()
    {
        if (bossEyeRenderTexture == null)
        {
            Debug.LogError(
                "[BossWaveIntroOverlay] Assign the RenderTexture."
            );
            yield break;
        }

        if (isPlaying)
            yield break;

        isPlaying = true;
        playbackEnded = false;

        CreateVideoInterface();

        videoPlayer.Prepare();

        // A missing StreamingAssets file or a stalled network would otherwise
        // wait here forever and stall the wave logic along with it.
        float prepareDeadline = Time.unscaledTime + prepareTimeout;

        while (!videoPlayer.isPrepared && !playbackEnded)
        {
            if (Time.unscaledTime >= prepareDeadline)
            {
                Debug.LogWarning(
                    "[BossWaveIntroOverlay] Video preparation timed out. " +
                    "Skipping the cinematic.");
                Cleanup();
                yield break;
            }

            yield return null;
        }

        // errorReceived fired during preparation; the handler already logged
        // the reason, so just bail out cleanly.
        if (playbackEnded)
        {
            Cleanup();
            yield break;
        }

        videoPlayer.Play();

        Debug.Log("[BossWaveIntroOverlay] DOMINUS cinematic playing.");

        // Wait one frame so the finish deadline below is measured against the
        // real playback state instead of the stale pre-play state.
        yield return null;

        // Fail-safe: loopPointReached is the normal exit, but it is not
        // guaranteed to be delivered on WebGL if the stream stalls, so also
        // poll against a deadline derived from the prepared clip length.
        float finishDeadline =
            Time.unscaledTime + (float)videoPlayer.length + finishTimeoutMargin;

        while (!playbackEnded && Time.unscaledTime < finishDeadline)
            yield return null;

        if (!playbackEnded)
        {
            Debug.LogWarning(
                "[BossWaveIntroOverlay] Finish event overdue. " +
                "Force-finishing the cinematic.");
        }

        Cleanup();

        Debug.Log("[BossWaveIntroOverlay] Cinematic finished.");
    }

    private void Cleanup()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            Destroy(videoPlayer.gameObject);
        }

        if (videoScreen != null)
        {
            Destroy(videoScreen.transform.root.gameObject);
        }

        // Reset so a later boss wave can play the cinematic again.
        isPlaying = false;
    }

    private void CreateVideoInterface()
    {
        GameObject canvasObject = new GameObject(
            "BossWaveCinematicCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        videoScreen = new GameObject(
            "BossEyeRawImage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage)
        );

        videoScreen.transform.SetParent(canvas.transform, false);

        RectTransform rect = videoScreen.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RawImage image = videoScreen.GetComponent<RawImage>();
        image.texture = bossEyeRenderTexture;
        image.color = Color.white;
        image.raycastTarget = false;

        GameObject playerObject = new GameObject("BossWaveVideoPlayer");
        videoPlayer = playerObject.AddComponent<VideoPlayer>();

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
        Debug.Log("[BossWaveIntroOverlay] Video URL: " + videoPlayer.url);

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = bossEyeRenderTexture;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        videoPlayer.loopPointReached += HandleVideoFinished;
        videoPlayer.errorReceived += HandleVideoError;
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        playbackEnded = true;
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning(
            "[BossWaveIntroOverlay] Video error: " + message +
            " -- skipping the cinematic.");
        playbackEnded = true;
    }
}
