
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuButtonSetup : MonoBehaviour
{
    [Header("Button Settings")]
    public float buttonWidth = 360f;
    public float buttonHeight = 65f;
    public float spacing = 18f;
    public float startY = -100f;

    [Header("Glass Appearance")]
    [Range(0f, 1f)]
    public float glassTransparency = 0.22f;

    public Color normalColor =
        new Color32(35, 55, 85, 255);

    public Color highlightedColor =
        new Color32(0, 160, 220, 255);

    public Color pressedColor =
        new Color32(0, 191, 255, 255);

    [Header("Black Outline")]
    public Color outlineColor = Color.black;
    public Color hoverOutlineColor = new Color32(0, 200, 255, 255);
    public Vector2 outlineThickness = new Vector2(2f, -2f);

    [Header("Button Animation")]
    public float hoverScale = 1.06f;
    public float pressedScale = 0.96f;
    public float animationSpeed = 12f;

    [Header("Text Settings")]
    public int fontSize = 28;
    public float characterSpacing = 3f;

    [Header("Button Sounds")]
    public AudioClip clickSound;
    public AudioClip hoverSound;
    [Range(0f, 1f)] public float clickVolume = 0.8f;
    [Range(0f, 1f)] public float hoverVolume = 0.4f;

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.35f;

    private AudioSource musicSource;
    private AudioSource soundSource;

    private readonly Dictionary<RectTransform, Coroutine>
        activeAnimations = new Dictionary<RectTransform, Coroutine>();

    private readonly string[] buttonNames =
    {
        "START GAME",
        "TUTORIAL",
        "SKIP TO BOSS",
    };

    private void Start()
    {
        SetupAudio();
        CreateButtons();
    }

    private void SetupAudio()
    {
        // Background music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        // Button sound source
        soundSource = gameObject.AddComponent<AudioSource>();
        soundSource.playOnAwake = false;
        soundSource.loop = false;

        // Start background music if assigned
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    private void CreateButtons()
    {
        Transform oldButtons = transform.Find("GeneratedButtons");

        if (oldButtons != null)
            Destroy(oldButtons.gameObject);

        GameObject container = new GameObject(
            "GeneratedButtons",
            typeof(RectTransform)
        );

        container.transform.SetParent(transform, false);

        RectTransform containerRect =
            container.GetComponent<RectTransform>();

        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;

        for (int i = 0; i < buttonNames.Length; i++)
        {
            CreateButton(container.transform, buttonNames[i], i);
        }
    }

    private void CreateButton(
        Transform parent,
        string buttonName,
        int index)
    {
        GameObject buttonObject = new GameObject(
            buttonName.Replace(" ", "") + "Button",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline),
            typeof(Button),
            typeof(EventTrigger)
        );

        buttonObject.transform.SetParent(parent, false);

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

        rect.anchoredPosition = new Vector2(
            0,
            startY - index * (buttonHeight + spacing)
        );

        rect.localScale = Vector3.one;

        // Glass background
        Image image = buttonObject.GetComponent<Image>();

        image.color = new Color(
            1f, 1f, 1f, glassTransparency
        );

        image.raycastTarget = true;

        // Outline
        Outline outline = buttonObject.GetComponent<Outline>();

        outline.effectColor = outlineColor;
        outline.effectDistance = outlineThickness;
        outline.useGraphicAlpha = false;

        // Button colour transitions
        Button button = buttonObject.GetComponent<Button>();

        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;

        colors.normalColor = new Color(
            normalColor.r,
            normalColor.g,
            normalColor.b,
            glassTransparency
        );

        colors.highlightedColor = new Color(
            highlightedColor.r,
            highlightedColor.g,
            highlightedColor.b,
            0.45f
        );

        colors.pressedColor = new Color(
            pressedColor.r,
            pressedColor.g,
            pressedColor.b,
            0.65f
        );

        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);

        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.15f;

        button.colors = colors;

        // Button text
        GameObject textObject = new GameObject(
            "Text (TMP)",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();

        text.text = buttonName;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.characterSpacing = characterSpacing;
        text.raycastTarget = false;

        // Add mouse interaction effects
        SetupButtonEvents(buttonObject, rect, outline);
    }

    private void SetupButtonEvents(
        GameObject buttonObject,
        RectTransform rect,
        Outline outline)
    {
        EventTrigger trigger =
            buttonObject.GetComponent<EventTrigger>();

        trigger.triggers = new List<EventTrigger.Entry>();

        // Mouse enters button
        AddEvent(trigger, EventTriggerType.PointerEnter, (data) =>
        {
            AnimateButton(rect, Vector3.one * hoverScale);

            outline.effectColor = hoverOutlineColor;

            PlayHoverSound();
        });

        // Mouse leaves button
        AddEvent(trigger, EventTriggerType.PointerExit, (data) =>
        {
            AnimateButton(rect, Vector3.one);

            outline.effectColor = outlineColor;
        });

        // Mouse presses button
        AddEvent(trigger, EventTriggerType.PointerDown, (data) =>
        {
            AnimateButton(rect, Vector3.one * pressedScale);

            PlayClickSound();
        });

        // Mouse releases button
        AddEvent(trigger, EventTriggerType.PointerUp, (data) =>
        {
            AnimateButton(rect, Vector3.one * hoverScale);
        });
    }

    private void AddEvent(
        EventTrigger trigger,
        EventTriggerType eventType,
        System.Action<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = eventType;
        entry.callback.AddListener((data) => action(data));

        trigger.triggers.Add(entry);
    }

    private void AnimateButton(
        RectTransform rect,
        Vector3 targetScale)
    {
        if (activeAnimations.TryGetValue(rect, out Coroutine running))
        {
            if (running != null)
                StopCoroutine(running);
        }

        activeAnimations[rect] =
            StartCoroutine(ScaleAnimation(rect, targetScale));
    }

    private IEnumerator ScaleAnimation(
        RectTransform rect,
        Vector3 targetScale)
    {
        while (Vector3.Distance(rect.localScale, targetScale) > 0.001f)
        {
            rect.localScale = Vector3.Lerp(
                rect.localScale,
                targetScale,
                Time.unscaledDeltaTime * animationSpeed
            );

            yield return null;
        }

        rect.localScale = targetScale;
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            soundSource.PlayOneShot(clickSound, clickVolume);
        }
    }

    private void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            soundSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }
}
