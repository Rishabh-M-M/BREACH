
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TutorialSceneController : MonoBehaviour
{
    [Header("Tutorial Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    private bool returningToMenu = false;

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

        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Play();

        Debug.Log("[Tutorial] Tutorial video started.");
    }

    private void OnVideoFinished(VideoPlayer player)
    {
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

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
