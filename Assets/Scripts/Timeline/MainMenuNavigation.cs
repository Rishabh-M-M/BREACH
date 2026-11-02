
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuNavigation : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Wait for MainMenuButtonSetup to generate the buttons.
        yield return null;

        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>(true);

            if (buttonText == null)
                continue;

            string label = buttonText.text.Trim().ToUpper();

            switch (label)
            {
                case "START GAME":
                    button.onClick.AddListener(StartGame);
                    break;

                case "TUTORIAL":
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log("Tutorial will be connected later.");
                    });
                    break;

                case "SKIP TO BOSS":
                    button.onClick.AddListener(SkipToBoss);
                    break;
            }
        }
    }

    private void StartGame()
    {
        Debug.Log("Starting NEXUS advertisement...");
        SceneManager.LoadScene("OpeningCutscene");
    }

    private void SkipToBoss()
    {
        Debug.Log("Opening Skip to Boss introduction...");
        SceneManager.LoadScene("BossSkipIntro");
    }
}
