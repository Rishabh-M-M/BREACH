using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSequenceController : MonoBehaviour
{
    [Header("Ending Scenes")]
    [SerializeField] private string defeatEndingScene = "DefeatEnding";
    [SerializeField] private string victoryEndingScene = "VictoryEnding";

    // Guards against double-loading within one scene. It clears itself on every
    // scene load, so a controller that survives a scene change (for example
    // because it shares a persistent GameObject with GameManager) can never
    // carry a stale "already started" value into the next run.
    private bool endingStarted;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        endingStarted = false;
    }

    /// <summary>
    /// Starts loading the ending scene. Returns true if the load was started,
    /// false if it was refused (already loading, or the scene can't be loaded).
    /// </summary>
    public bool PlayEnding(bool isWin)
    {
        if (endingStarted)
        {
            Debug.LogWarning("[EndingSequenceController] An ending is already loading.");
            return false;
        }

        string sceneName = isWin ? victoryEndingScene : defeatEndingScene;

        if (string.IsNullOrEmpty(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"[EndingSequenceController] Scene '{sceneName}' can't be loaded. " +
                "Check the name and File > Build Settings."
            );
            return false;
        }

        endingStarted = true;

        // Make sure animations and audio work normally.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Debug.Log(
            isWin
                ? "[EndingSequenceController] Player won. Loading VictoryEnding."
                : "[EndingSequenceController] Player lost. Loading DefeatEnding."
        );

        SceneManager.LoadScene(sceneName);
        return true;
    }
}