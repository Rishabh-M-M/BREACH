
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class OpeningCutsceneController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "StartScene";

    private bool hasTransitioned = false;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        if (hasTransitioned)
            return;

        hasTransitioned = true;

        Debug.Log("NEXUS advertisement finished. Loading gameplay...");

        SceneManager.LoadScene(nextSceneName);
    }
}
