using UnityEngine;

public class BreachAudioManager : MonoBehaviour
{
    public AudioSource buttonClickAudio;
    public AudioSource cameraGlitchAudio;
    public AudioSource breachAudio;

    public void PlayButtonClick()
    {
        if (buttonClickAudio != null)
        {
            buttonClickAudio.Play();
        }
    }

    public void PlayCameraGlitch()
    {
        if (cameraGlitchAudio != null)
        {
            cameraGlitchAudio.Play();
        }
    }

    public void PlayBreach()
    {
        if (breachAudio != null)
        {
            breachAudio.Play();
        }
    }
}