
using UnityEngine;
using UnityEngine.UI;
using ProjectBreach.Core;
using TMPro;
using System.Collections;

namespace ProjectBreach.Core
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Power UI")]
        [SerializeField] private Slider powerBar;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI availablePowerText;

        [Header("Generator UI")]
        [SerializeField] private GeneratorPassive generatorPassive;
        [SerializeField] private GameObject generatorUI;
        [SerializeField] private Slider generatorProgressBar;
        [SerializeField] private TextMeshProUGUI regenerationTimerText;
        [SerializeField] private TextMeshProUGUI regenerationAmountText;

        [Header("Generator Display Settings")]
        [SerializeField] private float generatorInterval = 10f;
        [SerializeField] private float growthPercent = 0.05f;
        [SerializeField] private int minGrowth = 5;

        private float generatorTimer;

        [Header("Message UI")]
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float messageDuration = 2f;

        [Header("Insufficient Power Warning")]
        [SerializeField] private GameObject insufficientPowerText;
        [SerializeField] private CanvasGroup insufficientPowerCanvasGroup;
        [SerializeField] private float warningDuration = 2f;
        [SerializeField] private float fadeOutDuration = 0.8f;

        private Coroutine warningFadeCoroutine;

        [Header("Insufficient Power Sound")]
        [SerializeField] private AudioSource warningAudioSource;
        [SerializeField] private AudioClip insufficientPowerClip;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            EventRegistry.OnPowerChanged += RefreshPowerBar;
        }

        private void OnDisable()
        {
            EventRegistry.OnPowerChanged -= RefreshPowerBar;
        }

        private void Start()
        {
            InitializePowerUI();
            HideMessage();
            HideInsufficientPowerWarning();

            if (generatorProgressBar != null)
            {
                generatorProgressBar.minValue = 0f;
                generatorProgressBar.maxValue = 1f;
                generatorProgressBar.value = 0f;
            }

            UpdateGeneratorUI();
        }

        private void Update()
        {
            UpdateGeneratorUI();
        }

        private void InitializePowerUI()
        {
            if (PowerManager.Instance != null)
            {
                RefreshPowerBar(
                    PowerManager.Instance.CurrentLoad,
                    PowerManager.Instance.MaxPower
                );
            }
            else
            {
                Debug.LogWarning("[UIManager] PowerManager not found!");
            }
        }

        private void RefreshPowerBar(int load, int max)
        {
            int remainingPower = Mathf.Max(0, max - load);

            if (powerBar != null)
            {
                powerBar.maxValue = max;
                powerBar.value = remainingPower;
            }

            if (powerText != null)
                powerText.text = $"{remainingPower}";

            if (availablePowerText != null)
                availablePowerText.text = $"AVAILABLE: {remainingPower}";
        }

        private void UpdateGeneratorUI()
        {
            if (generatorPassive == null || !generatorPassive.IsEnabled)
            {
                generatorTimer = 0f;

                if (generatorUI != null)
                    generatorUI.SetActive(false);

                return;
            }

            if (generatorUI != null && !generatorUI.activeSelf)
                generatorUI.SetActive(true);

            if (generatorInterval <= 0f)
                return;

            generatorTimer += Time.deltaTime;

            if (generatorTimer >= generatorInterval)
                generatorTimer -= generatorInterval;

            float progress = Mathf.Clamp01(generatorTimer / generatorInterval);

            if (generatorProgressBar != null)
                generatorProgressBar.value = progress;

            if (regenerationTimerText != null)
            {
                int secondsRemaining =
                    Mathf.CeilToInt(generatorInterval - generatorTimer);

                regenerationTimerText.text =
                    $"CHARGING: {secondsRemaining}s";
            }

            if (regenerationAmountText != null && PowerManager.Instance != null)
            {
                int nextIncrease = Mathf.Max(
                    minGrowth,
                    Mathf.RoundToInt(
                        PowerManager.Instance.MaxPower * growthPercent
                    )
                );

                regenerationAmountText.text = $" +{nextIncrease} POWER";
            }
        }

        public void ShowMessage(string message)
        {
            if (messagePanel == null || messageText == null)
                return;

            messageText.text = message;
            messagePanel.SetActive(true);

            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), messageDuration);
        }

        public void ShowInsufficientPowerMessage()
        {
            if (insufficientPowerText != null)
            {
                // Stop any previous fade.
                if (warningFadeCoroutine != null)
                    StopCoroutine(warningFadeCoroutine);

                // Make the warning fully visible immediately.
                if (insufficientPowerCanvasGroup != null)
                    insufficientPowerCanvasGroup.alpha = 1f;

                insufficientPowerText.SetActive(true);

                // Start the visibility and fade sequence again.
                warningFadeCoroutine =
                    StartCoroutine(FadeOutInsufficientPowerWarning());
            }

            // Play warning sound.
            if (warningAudioSource != null && insufficientPowerClip != null)
            {
                warningAudioSource.PlayOneShot(insufficientPowerClip);
            }
        }

        private IEnumerator FadeOutInsufficientPowerWarning()
        {
            // Keep warning visible for the configured duration.
            yield return new WaitForSeconds(warningDuration);

            // Slowly fade away.
            if (insufficientPowerCanvasGroup != null)
            {
                float elapsed = 0f;

                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;

                    insufficientPowerCanvasGroup.alpha =
                        Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);

                    yield return null;
                }

                insufficientPowerCanvasGroup.alpha = 0f;
            }

            // Hide the warning after fading.
            if (insufficientPowerText != null)
                insufficientPowerText.SetActive(false);

            warningFadeCoroutine = null;
        }

        private void HideInsufficientPowerWarning()
        {
            if (warningFadeCoroutine != null)
            {
                StopCoroutine(warningFadeCoroutine);
                warningFadeCoroutine = null;
            }

            if (insufficientPowerCanvasGroup != null)
                insufficientPowerCanvasGroup.alpha = 0f;

            if (insufficientPowerText != null)
                insufficientPowerText.SetActive(false);
        }

        private void HideMessage()
        {
            if (messagePanel != null)
                messagePanel.SetActive(false);
        }

        [ContextMenu("Test - Show Message")]
        private void TestShowMessage()
        {
            ShowInsufficientPowerMessage();
        }
    }
}
