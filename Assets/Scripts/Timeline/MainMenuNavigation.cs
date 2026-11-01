
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
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);

            if (buttonText == null)
                continue;

            string label = buttonText.text.Trim().ToUpper();

            switch (label)
            {
                case "START GAME":
                    button.onClick.AddListener(StartGame);
                    break;

                case "QUIT":
                    button.onClick.AddListener(QuitGame);
                    break;

                case "TUTORIAL":
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log("Tutorial will be connected later.");
                    });
                    break;

                case "SKIP TO BOSS":
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log("Skip to Boss will be connected later.");
                    });
                    break;
            }
        }
    }

    private void StartGame()
    {
        Debug.Log("Starting NEXUS advertisement...");
        SceneManager.LoadScene("OpeningCutscene");
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Breach...");
        Application.Quit();
    }
}
