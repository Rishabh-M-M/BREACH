using UnityEngine;
using TMPro;
using ProjectBreach.Core;

namespace ProjectBreach.UI
{
    /// <summary>
    /// Banner that announces each incoming wave.
    ///
    /// GRASP: Indirection -- it listens on EventRegistry instead of holding a
    /// reference to WaveManager, so it keeps working no matter who owns the waves.
    ///
    /// Scene setup: create a Canvas group with a TextMeshProUGUI, put this
    /// component on a HUD root, drag the text in. Starts hidden.
    /// </summary>
    public class WaveBannerHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject bannerRoot;
        [SerializeField] private TextMeshProUGUI bannerText;

        [Header("Timing")]
        [SerializeField] private float showDuration = 3f;

        [Header("Text")]
        [Tooltip("{0} is replaced with the 1-based wave number.")]
        [SerializeField] private string format = "Wave {0}";

        private Coroutine _hideRoutine;

        private void OnEnable()
        {
            EventRegistry.OnWaveStart += HandleWaveStart;
        }

        private void OnDisable()
        {
            EventRegistry.OnWaveStart -= HandleWaveStart;
            // Unsubscribe on the way out as well, or the banner keeps showing while
            // this object is disabled and the routine dies with it.
            if (bannerRoot != null)
            {
                bannerRoot.SetActive(false);
            }
        }

        private void Start()
        {
            if (bannerRoot != null)
            {
                bannerRoot.SetActive(false);
            }
        }

        private void HandleWaveStart(int waveIndex)
        {
            if (bannerText != null)
            {
                bannerText.text = string.Format(format, waveIndex + 1);
            }

            if (bannerRoot != null)
            {
                bannerRoot.SetActive(true);
            }

            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
            }

            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private System.Collections.IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(showDuration);

            if (bannerRoot != null)
            {
                bannerRoot.SetActive(false);
            }
        }
    }
}
