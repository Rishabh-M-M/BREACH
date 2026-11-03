
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSequenceController : MonoBehaviour
{
    [Header("Ending Scenes")]
    [SerializeField] private string defeatEndingScene = "DefeatEnding";
    [SerializeField] private string victoryEndingScene = "VictoryEnding";

    private bool endingStarted;

    public void PlayEnding(bool isWin)
    {
        if (endingStarted)
        {
            Debug.LogWarning("[EndingSequenceController] An ending is already loading.");
            return;
        }

        endingStarted = true;

        // Make sure animations and audio work normally.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (isWin)
        {
            Debug.Log("[EndingSequenceController] Player won. Loading VictoryEnding.");

            SceneManager.LoadScene(victoryEndingScene);
        }
        else
        {
            Debug.Log("[EndingSequenceController] Player lost. Loading DefeatEnding.");

            SceneManager.LoadScene(defeatEndingScene);
        }
    }
}
