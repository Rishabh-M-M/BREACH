using UnityEngine;
using TMPro;
using ProjectBreach.Core;

namespace ProjectBreach.UI
{
    /// <summary>
    /// Shows the Core HP, optionally as a bar plus text.
    ///
    /// MainControlRoom only broadcasts when HP changes, so the initial value is
    /// pulled straight from the reference in Start; without that the HUD would sit
    /// blank until the first breach.
    /// </summary>
    public class CoreHPHUD : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Optional. Only needed to seed the display before the first hit.")]
        [SerializeField] private MainControlRoom mainControlRoom;

        [SerializeField] private UnityEngine.UI.Slider hpBar;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Text")]
        [Tooltip("{0} = current HP, {1} = max HP.")]
        [SerializeField] private string format = "Core {0}/{1}";

        [Header("Flash")]
        [Tooltip("Briefly reveal the text on damage even if it is normally hidden.")]
        [SerializeField] private bool flashOnDamage = true;

        [SerializeField] private float flashDuration = 1.5f;

        private CanvasGroup _canvasGroup;
        private Coroutine _flashRoutine;

        private void OnEnable()
        {
            EventRegistry.OnCoreHPChanged += HandleCoreHPChanged;
        }

        private void OnDisable()
        {
            EventRegistry.OnCoreHPChanged -= HandleCoreHPChanged;
        }

        private void Start()
        {
            if (hpText != null)
            {
                _canvasGroup = hpText.GetComponent<CanvasGroup>();
            }

            if (mainControlRoom != null)
            {
                Refresh(mainControlRoom.CurrentHP, mainControlRoom.MaxHP);
            }
        }

        private void HandleCoreHPChanged(int current, int max)
        {
            Refresh(current, max);

            if (flashOnDamage && _canvasGroup != null)
            {
                if (_flashRoutine != null)
                {
                    StopCoroutine(_flashRoutine);
                }

                _flashRoutine = StartCoroutine(FlashRoutine());
            }
        }

        private void Refresh(int current, int max)
        {
            if (hpBar != null && max > 0)
            {
                hpBar.maxValue = max;
                hpBar.value = current;
            }

            if (hpText != null)
            {
                hpText.text = string.Format(format, current, max);
            }
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            _canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(flashDuration);
            _canvasGroup.alpha = 0f;
        }
    }
}
