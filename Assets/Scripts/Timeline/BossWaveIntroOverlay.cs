
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BossWaveIntroOverlay : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoClip bossEyeClip;
    [SerializeField] private RenderTexture bossEyeRenderTexture;

    private VideoPlayer videoPlayer;
    private GameObject videoScreen;
    private bool isPlaying;

    public IEnumerator PlayAndWait()
    {
        if (bossEyeClip == null || bossEyeRenderTexture == null)
        {
            Debug.LogError(
                "[BossWaveIntroOverlay] Assign the video and RenderTexture."
            );
            yield break;
        }

        if (isPlaying)
            yield break;

        isPlaying = true;

        CreateVideoInterface();

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        Debug.Log("[BossWaveIntroOverlay] DOMINUS cinematic playing.");

        while (videoPlayer.isPlaying)
            yield return null;

        // Wait for the video completion callback.
        while (!videoFinished)
            yield return null;

        videoPlayer.Stop();

        Destroy(videoPlayer.gameObject);
        Destroy(videoScreen.transform.root.gameObject);

        Debug.Log("[BossWaveIntroOverlay] Cinematic finished.");
    }

    private bool videoFinished;

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
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = bossEyeClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = bossEyeRenderTexture;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        videoPlayer.loopPointReached += HandleVideoFinished;
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        videoFinished = true;
    }
}
