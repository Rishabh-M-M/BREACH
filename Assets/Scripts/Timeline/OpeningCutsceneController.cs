
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class OpeningCutsceneController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "StartScene";

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Set to true when the advertisement is played after Ending A.
    public static bool PlayAsEndingAdvertisement = false;

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

        if (PlayAsEndingAdvertisement)
        {
            PlayAsEndingAdvertisement = false;

            Debug.Log(
                "Ending A advertisement finished. Returning to main menu..."
            );

            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.Log(
                "NEXUS advertisement finished. Loading gameplay..."
            );

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
