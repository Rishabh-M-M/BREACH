using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecurityMinimap : MonoBehaviour
{
    [Header("Shield References")]
    [SerializeField] private Shield mainRoomShield;
    [SerializeField] private Shield[] roomShields = new Shield[4];

    [Header("Automatic Room Mapping")]
    [Tooltip("Drag complete Room GameObjects here in order: Room 1, Room 2, Room 3, Room 4.")]
    [SerializeField] private GameObject[] roomObjects = new GameObject[4];

    [Tooltip("Extra padding added around automatically detected room bounds.")]
    [SerializeField] private float roomBoundsPadding = 0.5f;

    [Tooltip("Reverse the forward direction for rooms where enemies move toward decreasing world Z.")]
    [SerializeField] private bool[] invertRoomDepth = new bool[4];

    [Header("Minimap Appearance")]
    [SerializeField] private Vector2 panelSize = new Vector2(260f, 230f);
    [SerializeField] private Vector2 panelPosition = new Vector2(18f, 18f);

    [SerializeField]
    private Color panelColour =
        new Color(0.015f, 0.035f, 0.065f, 0.96f);

    [SerializeField]
    private Color panelBorderColour =
        new Color(0.05f, 0.75f, 0.85f, 0.45f);

    [SerializeField]
    private Color laneBackgroundColour =
        new Color(0.025f, 0.07f, 0.10f, 1f);

    [SerializeField]
    private Color healthyColour =
        new Color(0.05f, 0.95f, 1f, 1f);

    [SerializeField]
    private Color warningColour =
        new Color(1f, 0.58f, 0.08f, 1f);

    [SerializeField]
    private Color criticalColour =
        new Color(1f, 0.04f, 0.08f, 1f);

    [SerializeField]
    private Color fallenColour =
        new Color(0.035f, 0.045f, 0.06f, 1f);

    [Header("Text / Labels")]
    [Tooltip("Optional. Leave empty to use Unity's built-in font.")]
    [SerializeField] private Font uiFont;

    [SerializeField]
    private Color textColour =
        new Color(0.70f, 0.95f, 1f, 1f);

    [SerializeField] private string panelTitle = "SECURITY GRID";
    [SerializeField] private float titleHeight = 16f;
    [SerializeField] private float laneHeaderHeight = 28f;

    [Tooltip("Shadows can make small text look blurry. Leave off for crisp text.")]
    [SerializeField] private bool textShadow = false;

    [Tooltip("Renders text at this multiple of its size and scales it down. 2 is a big sharpness boost, 3 or 4 is sharper still.")]
    [Range(1, 4)]
    [SerializeField] private int textSupersample = 2;

    [Tooltip("Thickness (UI pixels) of the lane / panel borders.")]
    [SerializeField] private float borderThickness = 2f;

    [Header("Enemy Dots")]
    [SerializeField] private float enemyDotSize = 16f;
    [SerializeField] private float enemyGlowSize = 30f;
    [SerializeField]
    private Color enemyRed =
        new Color(1f, 0.005f, 0.005f, 1f);

    [SerializeField] private float enemyScanInterval = 0.1f;
    [SerializeField] private float enemyPulseSpeed = 6f;

    [Header("Enemy Dot Placement")]
    [Tooltip("Each dot gets a random sideways offset (in UI pixels) around the lane centre, so dots spawn in the middle but don't stack perfectly.")]
    [SerializeField] private float dotCentreOffset = 7f;

    [Tooltip("How much the enemy's real left/right position moves the dot (0 = always centre, 1 = full lane width).")]
    [Range(0f, 1f)]
    [SerializeField] private float dotSideTracking = 0.12f;

    private RectTransform minimapRoot;
    private RectTransform[] laneAreas = new RectTransform[4];

    private Image[] laneBackgrounds = new Image[4];
    private Image[] laneShieldFills = new Image[4];
    private Image[] laneShieldGlows = new Image[4];
    private Image[][] laneBorders = new Image[4][];
    private Image[] panelBorder;
    private Image[] mainBorder;
    private Text[] laneLabels = new Text[4];
    private Text[] laneBreachTexts = new Text[4];
    private int[] laneEnemyCount = new int[4];

    private Image mainRoomBackground;
    private Image mainRoomShieldFill;
    private Image mainRoomShieldGlow;
    private Text mainRoomText;
    private int lastMainPercent = -1;
    private bool lastMainBreached;

    private Text threatText;
    private int lastThreatCount = -1;

    private readonly Dictionary<EnemyHealth, EnemyDot> enemyDots =
        new Dictionary<EnemyHealth, EnemyDot>();

    private readonly List<EnemyHealth> enemiesToRemove =
        new List<EnemyHealth>();

    private float nextEnemyScanTime;
    private Sprite circleSprite;
    private Sprite solidSprite;
    private Font cachedFont;

    private readonly List<Text> allTexts = new List<Text>();
    private Canvas rootCanvas;
    private float lastCanvasScale = -1f;

    private class EnemyDot
    {
        public RectTransform root;
        public Image glow;
        public Image core;
        public Image centre;
        public int roomIndex;
        public float pulseOffset;
        public float sideOffset;
    }

    private void Awake()
    {
        CreateCircleSprite();
        CreateSolidSprite();
        BuildMinimap();

        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null)
            rootCanvas = parentCanvas.rootCanvas;
    }

    private void OnEnable()
    {
        StartCoroutine(RefreshTextDelayed());
    }

    private void Update()
    {
        CheckCanvasScale();
        UpdateShieldVisuals();
        UpdateEnemyDots();

        if (Time.unscaledTime >= nextEnemyScanTime)
        {
            nextEnemyScanTime =
                Time.unscaledTime + Mathf.Max(0.02f, enemyScanInterval);

            ScanForEnemies();
        }
    }

    // =========================================================
    // TEXT SHARPNESS
    // =========================================================
    // Dynamic fonts get rasterised at whatever Canvas scale exists when the
    // text is first built. With "Scale With Screen Size" that scale often
    // isn't final on the first frames, so the glyphs start soft and only
    // sharpen later. Rebuilding the text once the scale settles fixes that.

    private void CheckCanvasScale()
    {
        if (rootCanvas == null)
            return;

        float scale = rootCanvas.scaleFactor;

        if (!Mathf.Approximately(scale, lastCanvasScale))
        {
            lastCanvasScale = scale;
            RefreshText();
        }
    }

    private IEnumerator RefreshTextDelayed()
    {
        yield return null;
        RefreshText();

        yield return new WaitForSecondsRealtime(0.5f);
        RefreshText();
    }

    private void RefreshText()
    {
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < allTexts.Count; i++)
        {
            Text text = allTexts[i];

            if (text == null)
                continue;

            text.FontTextureChanged();
            text.SetAllDirty();
        }
    }

    private void OnDestroy()
    {
        if (circleSprite != null)
        {
            Texture2D texture = circleSprite.texture;
            Destroy(circleSprite);

            if (texture != null)
                Destroy(texture);
        }

        // The solid sprite uses Unity's shared white texture, so only the sprite is destroyed.
        if (solidSprite != null)
            Destroy(solidSprite);
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform)
        );

        obj.transform.SetParent(parent, false);
        return obj;
    }

    private Font GetFont()
    {
        if (uiFont != null)
            return uiFont;

        if (cachedFont == null)
        {
#if UNITY_2022_2_OR_NEWER
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }

        return cachedFont;
    }

    private Text CreateText(
        string objectName,
        Transform parent,
        int fontSize,
        TextAnchor alignment,
        Color colour,
        FontStyle style = FontStyle.Bold)
    {
        GameObject obj = CreateUIObject(objectName, parent);

        Text text = obj.AddComponent<Text>();
        text.font = GetFont();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = colour;
        text.fontStyle = style;
        text.raycastTarget = false;
        text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        allTexts.Add(text);

        if (textShadow)
        {
            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1f, -1f);
        }

        return text;
    }

    // Supersampling: the text is built at (size x textSupersample) and then
    // scaled back down, so Unity rasterises the glyphs at a higher resolution
    // and the result is much sharper. The rect is anchored and pivoted at the
    // same point, so scaling it never moves it.
    private void SetTextRect(Text text, Vector2 anchorAndPivot, Vector2 position, Vector2 size)
    {
        float k = Mathf.Max(1, textSupersample);

        RectTransform r = text.rectTransform;
        r.anchorMin = anchorAndPivot;
        r.anchorMax = anchorAndPivot;
        r.pivot = anchorAndPivot;
        r.anchoredPosition = position;
        r.sizeDelta = size * k;
        r.localScale = new Vector3(1f / k, 1f / k, 1f);

        text.fontSize = Mathf.RoundToInt(text.fontSize * k);
    }

    // Complete rectangular border made of four solid strips, drawn inside
    // the parent so it is never clipped and always closes at the corners.
    private Image[] CreateBorder(RectTransform parent, float thickness, Color colour)
    {
        Image[] edges = new Image[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject obj = CreateUIObject("Border " + i, parent);

            Image img = obj.AddComponent<Image>();
            img.color = colour;
            img.raycastTarget = false;

            RectTransform r = img.rectTransform;

            switch (i)
            {
                case 0: // top
                    SetRect(r, new Vector2(0f, 1f), new Vector2(1f, 1f),
                        new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, thickness));
                    break;
                case 1: // bottom
                    SetRect(r, new Vector2(0f, 0f), new Vector2(1f, 0f),
                        new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, thickness));
                    break;
                case 2: // left
                    SetRect(r, new Vector2(0f, 0f), new Vector2(0f, 1f),
                        new Vector2(0f, 0.5f), Vector2.zero, new Vector2(thickness, 0f));
                    break;
                default: // right
                    SetRect(r, new Vector2(1f, 0f), new Vector2(1f, 1f),
                        new Vector2(1f, 0.5f), Vector2.zero, new Vector2(thickness, 0f));
                    break;
            }

            edges[i] = img;
        }

        return edges;
    }

    private void SetBorderColour(Image[] edges, Color colour)
    {
        if (edges == null)
            return;

        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i] != null)
                edges[i].color = colour;
        }
    }

    private void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    // =========================================================
    // BUILD MINIMAP
    // =========================================================

    private void BuildMinimap()
    {
        minimapRoot = GetComponent<RectTransform>();

        if (minimapRoot == null)
            minimapRoot = gameObject.AddComponent<RectTransform>();

        minimapRoot.anchorMin = Vector2.zero;
        minimapRoot.anchorMax = Vector2.zero;
        minimapRoot.pivot = Vector2.zero;
        minimapRoot.anchoredPosition = panelPosition;
        minimapRoot.sizeDelta = panelSize;

        Image panel = GetComponent<Image>();

        if (panel == null)
            panel = gameObject.AddComponent<Image>();

        panel.color = panelColour;
        panel.raycastTarget = false;

        panelBorder = CreateBorder(minimapRoot, borderThickness, panelBorderColour);

        float sidePadding = 8f;
        float laneGap = 5f;
        float laneWidth =
            (panelSize.x - sidePadding * 2f - laneGap * 3f) / 4f;

        float topPadding = 6f;
        float titleGap = 6f;
        float mainBarHeight = 23f;
        float mainBarBottom = 10f;
        float gapAboveMainBar = 8f;

        float laneBottom = mainBarBottom + mainBarHeight + gapAboveMainBar;

        float laneHeight =
            panelSize.y - topPadding - titleHeight - titleGap - laneBottom;

        // ---------- Title bar ----------
        Text titleText = CreateText(
            "Panel Title",
            minimapRoot,
            13,
            TextAnchor.MiddleLeft,
            textColour
        );

        titleText.text = panelTitle;

        SetTextRect(
            titleText,
            new Vector2(0f, 1f),
            new Vector2(sidePadding, -topPadding),
            new Vector2(panelSize.x - sidePadding * 2f, titleHeight)
        );

        threatText = CreateText(
            "Threat Counter",
            minimapRoot,
            12,
            TextAnchor.MiddleRight,
            healthyColour
        );

        threatText.text = "ALL CLEAR";

        SetTextRect(
            threatText,
            new Vector2(1f, 1f),
            new Vector2(-sidePadding, -topPadding),
            new Vector2(panelSize.x - sidePadding * 2f, titleHeight)
        );

        GameObject dividerObject =
            CreateUIObject("Title Divider", minimapRoot);

        Image divider = dividerObject.AddComponent<Image>();
        divider.color = panelBorderColour;
        divider.raycastTarget = false;

        SetRect(
            divider.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -(topPadding + titleHeight + 2f)),
            new Vector2(-sidePadding * 2f, 1f)
        );

        // ---------- Lanes ----------
        for (int i = 0; i < 4; i++)
        {
            GameObject laneObject =
                CreateUIObject("Room " + (i + 1) + " Lane", minimapRoot);

            RectTransform laneRect =
                laneObject.GetComponent<RectTransform>();

            laneRect.anchorMin = Vector2.zero;
            laneRect.anchorMax = Vector2.zero;
            laneRect.pivot = Vector2.zero;

            laneRect.sizeDelta = new Vector2(laneWidth, laneHeight);

            laneRect.anchoredPosition = new Vector2(
                sidePadding + i * (laneWidth + laneGap),
                laneBottom
            );

            Image laneBackground = laneObject.AddComponent<Image>();
            laneBackground.color = laneBackgroundColour;
            laneBackground.raycastTarget = false;

            laneAreas[i] = laneRect;
            laneBackgrounds[i] = laneBackground;
            laneBorders[i] = CreateBorder(
                laneRect,
                borderThickness,
                new Color(0.05f, 0.5f, 0.6f, 0.35f)
            );

            // Vertical shield track (sits below the header).
            GameObject trackObject =
                CreateUIObject("Shield Track", laneRect);

            RectTransform trackRect =
                trackObject.GetComponent<RectTransform>();

            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.pivot = new Vector2(0.5f, 0.5f);
            trackRect.offsetMin = new Vector2(8f, 6f);
            trackRect.offsetMax = new Vector2(-8f, -(laneHeaderHeight + 2f));

            Image trackImage = trackObject.AddComponent<Image>();
            trackImage.color = new Color(0.06f, 0.12f, 0.16f, 1f);
            trackImage.raycastTarget = false;

            // Shield fill.
            GameObject fillObject =
                CreateUIObject("Shield Health", trackRect);

            RectTransform fillRect =
                fillObject.GetComponent<RectTransform>();

            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0.5f, 0f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.sprite = solidSprite;
            fillImage.color = healthyColour;
            fillImage.raycastTarget = false;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Vertical;
            fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            fillImage.fillAmount = 1f;

            // Soft halo behind the shield fill.
            GameObject glowObject =
                CreateUIObject("Shield Glow", trackRect);

            RectTransform glowRect =
                glowObject.GetComponent<RectTransform>();

            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.offsetMin = new Vector2(-3f, -3f);
            glowRect.offsetMax = new Vector2(3f, 3f);

            Image glowImage = glowObject.AddComponent<Image>();
            glowImage.color = new Color(0.05f, 0.95f, 1f, 0.12f);
            glowImage.raycastTarget = false;

            // Render glow behind the fill.
            glowObject.transform.SetAsFirstSibling();

            laneShieldFills[i] = fillImage;
            laneShieldGlows[i] = glowImage;

            // Header: "ROOM 1" + shield percentage.
            Text label = CreateText(
                "Room Label",
                laneRect,
                14,
                TextAnchor.MiddleCenter,
                textColour
            );

            label.text = "ROOM " + (i + 1);

            SetTextRect(
                label,
                new Vector2(0.5f, 1f),
                new Vector2(0f, -5f),
                new Vector2(laneWidth, 18f)
            );

            // Breach warning, only visible when the shield is down.
            Text breach = CreateText(
                "Breach Text",
                laneRect,
                11,
                TextAnchor.MiddleCenter,
                criticalColour
            );

            breach.text = "BREACH";

            SetTextRect(
                breach,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -(laneHeaderHeight * 0.5f)),
                new Vector2(laneWidth, 14f)
            );

            breach.gameObject.SetActive(false);

            laneLabels[i] = label;
            laneBreachTexts[i] = breach;
        }

        // ---------- Bottom Main Room shield bar ----------
        GameObject mainBarObject =
            CreateUIObject("Main Room Shield Bar", minimapRoot);

        RectTransform mainBarRect =
            mainBarObject.GetComponent<RectTransform>();

        mainBarRect.anchorMin = Vector2.zero;
        mainBarRect.anchorMax = Vector2.zero;
        mainBarRect.pivot = Vector2.zero;

        mainBarRect.sizeDelta = new Vector2(
            panelSize.x - sidePadding * 2f,
            mainBarHeight
        );

        mainBarRect.anchoredPosition =
            new Vector2(sidePadding, mainBarBottom);

        mainRoomBackground = mainBarObject.AddComponent<Image>();
        mainRoomBackground.color = laneBackgroundColour;
        mainRoomBackground.raycastTarget = false;

        mainBorder = CreateBorder(
            mainBarRect,
            1f,
            new Color(0.05f, 0.5f, 0.6f, 0.35f)
        );

        GameObject mainGlowObject =
            CreateUIObject("Main Room Shield Glow", mainBarRect);

        RectTransform mainGlowRect =
            mainGlowObject.GetComponent<RectTransform>();

        mainGlowRect.anchorMin = Vector2.zero;
        mainGlowRect.anchorMax = Vector2.one;
        mainGlowRect.offsetMin = new Vector2(-2f, -2f);
        mainGlowRect.offsetMax = new Vector2(2f, 2f);

        mainRoomShieldGlow = mainGlowObject.AddComponent<Image>();
        mainRoomShieldGlow.color =
            new Color(0.05f, 0.95f, 1f, 0.12f);
        mainRoomShieldGlow.raycastTarget = false;

        GameObject mainFillObject =
            CreateUIObject("Main Room Shield Health", mainBarRect);

        RectTransform mainFillRect =
            mainFillObject.GetComponent<RectTransform>();

        mainFillRect.anchorMin = Vector2.zero;
        mainFillRect.anchorMax = Vector2.one;
        mainFillRect.offsetMin = new Vector2(3f, 3f);
        mainFillRect.offsetMax = new Vector2(-3f, -3f);
        mainFillRect.pivot = new Vector2(0f, 0.5f);

        mainRoomShieldFill = mainFillObject.AddComponent<Image>();
        mainRoomShieldFill.sprite = solidSprite;
        mainRoomShieldFill.color = healthyColour;
        mainRoomShieldFill.raycastTarget = false;
        mainRoomShieldFill.type = Image.Type.Filled;
        mainRoomShieldFill.fillMethod = Image.FillMethod.Horizontal;
        mainRoomShieldFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        mainRoomShieldFill.fillAmount = 1f;

        // Main room text sits on top of the fill.
        mainRoomText = CreateText(
            "Main Room Text",
            mainBarRect,
            12,
            TextAnchor.MiddleCenter,
            Color.white
        );

        mainRoomText.text = "MAIN ROOM  100%";

        SetTextRect(
            mainRoomText,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(panelSize.x - sidePadding * 2f, mainBarHeight)
        );
    }

    // =========================================================
    // SHIELD HEALTH
    // =========================================================

    private void UpdateShieldVisuals()
    {
        for (int i = 0; i < 4; i++)
        {
            Shield shield =
                roomShields != null && i < roomShields.Length
                    ? roomShields[i]
                    : null;

            UpdateRoomShieldVisual(i, shield);
        }

        UpdateMainRoomShieldVisual(mainRoomShield);
    }

    private void UpdateRoomShieldVisual(int index, Shield shield)
    {
        if (laneShieldFills[index] == null)
            return;

        if (shield == null || shield.IsDestroyed)
        {
            laneShieldFills[index].fillAmount = 0f;
            laneShieldFills[index].color = fallenColour;
            laneShieldGlows[index].color = fallenColour;
            laneBackgrounds[index].color = fallenColour;

            if (laneLabels[index] != null)
                laneLabels[index].color = new Color(0.45f, 0.5f, 0.55f, 1f);

            if (laneBreachTexts[index] != null)
            {
                laneBreachTexts[index].gameObject.SetActive(true);

                float blink =
                    (Mathf.Sin(Time.unscaledTime * 5f) + 1f) * 0.5f;

                laneBreachTexts[index].color = new Color(
                    criticalColour.r,
                    criticalColour.g,
                    criticalColour.b,
                    Mathf.Lerp(0.45f, 1f, blink)
                );
            }

            return;
        }

        float health = GetHealthPercentage(shield);
        Color healthColour = GetHealthGradient(health);

        // Critical shields flicker so they catch the eye.
        float criticalPulse = 1f;

        if (health < 0.25f)
        {
            criticalPulse = Mathf.Lerp(
                0.55f,
                1f,
                (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.5f
            );
        }

        laneShieldFills[index].fillAmount = health;
        laneShieldFills[index].color = new Color(
            healthColour.r,
            healthColour.g,
            healthColour.b,
            criticalPulse
        );

        laneShieldGlows[index].color =
            new Color(healthColour.r, healthColour.g, healthColour.b, 0.16f);

        laneBackgrounds[index].color = laneBackgroundColour;

        if (laneLabels[index] != null)
            laneLabels[index].color = textColour;

        if (laneBreachTexts[index] != null)
            laneBreachTexts[index].gameObject.SetActive(false);
    }

    private void UpdateMainRoomShieldVisual(Shield shield)
    {
        if (mainRoomShieldFill == null)
            return;

        if (shield == null || shield.IsDestroyed)
        {
            mainRoomShieldFill.fillAmount = 0f;
            mainRoomShieldFill.color = fallenColour;
            mainRoomShieldGlow.color = fallenColour;
            mainRoomBackground.color = fallenColour;

            if (mainRoomText != null)
            {
                if (!lastMainBreached)
                {
                    mainRoomText.text = "MAIN ROOM BREACHED";
                    lastMainBreached = true;
                    lastMainPercent = -1;
                }

                float blink =
                    (Mathf.Sin(Time.unscaledTime * 5f) + 1f) * 0.5f;

                mainRoomText.color = new Color(
                    criticalColour.r,
                    criticalColour.g,
                    criticalColour.b,
                    Mathf.Lerp(0.5f, 1f, blink)
                );
            }

            return;
        }

        float health = GetHealthPercentage(shield);
        Color healthColour = GetHealthGradient(health);

        float criticalPulse = 1f;

        if (health < 0.25f)
        {
            criticalPulse = Mathf.Lerp(
                0.55f,
                1f,
                (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.5f
            );
        }

        mainRoomShieldFill.fillAmount = health;
        mainRoomShieldFill.color = new Color(
            healthColour.r,
            healthColour.g,
            healthColour.b,
            criticalPulse
        );

        mainRoomShieldGlow.color =
            new Color(healthColour.r, healthColour.g, healthColour.b, 0.16f);

        mainRoomBackground.color = laneBackgroundColour;

        if (mainRoomText != null)
        {
            int percent = Mathf.RoundToInt(health * 100f);

            if (percent != lastMainPercent || lastMainBreached)
            {
                lastMainPercent = percent;
                lastMainBreached = false;
                mainRoomText.text = "MAIN ROOM  " + percent + "%";
            }

            mainRoomText.color = Color.white;
        }
    }

    private float GetHealthPercentage(Shield shield)
    {
        if (shield == null || shield.MaxHealth <= 0)
            return 0f;

        return Mathf.Clamp01((float)shield.CurrentHealth / shield.MaxHealth);
    }

    private Color GetHealthGradient(float health)
    {
        if (health <= 0f)
            return fallenColour;

        if (health >= 0.5f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1f, 0.5f, health));
            return Color.Lerp(healthyColour, warningColour, t);
        }

        float lowT = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 0f, health));
        return Color.Lerp(warningColour, criticalColour, lowT);
    }

    // =========================================================
    // FIND ENEMIES
    // =========================================================

    private void ScanForEnemies()
    {
        EnemyHealth[] enemies =
            FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        HashSet<EnemyHealth> seenThisScan =
            new HashSet<EnemyHealth>();

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            EnemyMover mover = GetEnemyMover(enemy);

            if (mover == null)
                continue;

            int roomIndex = mover.CurrentRoomIndex;

            if (roomIndex < 1 || roomIndex > 4)
                continue;

            int arrayIndex = roomIndex - 1;

            if (IsRoomShieldBroken(arrayIndex))
            {
                RemoveEnemyDot(enemy);
                continue;
            }

            seenThisScan.Add(enemy);

            if (!enemyDots.TryGetValue(enemy, out EnemyDot dot))
            {
                dot = CreateEnemyDot(laneAreas[arrayIndex]);
                dot.roomIndex = roomIndex;
                enemyDots.Add(enemy, dot);
            }

            // If an enemy changes rooms, move its dot into the
            // appropriate lane instead of leaving it in the old lane.
            if (dot.roomIndex != roomIndex)
            {
                dot.root.SetParent(laneAreas[arrayIndex], false);
                dot.roomIndex = roomIndex;
            }
        }

        enemiesToRemove.Clear();

        foreach (KeyValuePair<EnemyHealth, EnemyDot> pair in enemyDots)
        {
            if (pair.Key == null ||
                pair.Key.IsDead ||
                !seenThisScan.Contains(pair.Key))
            {
                enemiesToRemove.Add(pair.Key);
            }
        }

        foreach (EnemyHealth enemy in enemiesToRemove)
            RemoveEnemyDot(enemy);
    }

    private EnemyMover GetEnemyMover(EnemyHealth enemy)
    {
        EnemyMover mover = enemy.GetComponent<EnemyMover>();

        if (mover == null)
            mover = enemy.GetComponentInParent<EnemyMover>();

        return mover;
    }

    private bool IsRoomShieldBroken(int arrayIndex)
    {
        return roomShields != null &&
               arrayIndex >= 0 &&
               arrayIndex < roomShields.Length &&
               roomShields[arrayIndex] != null &&
               roomShields[arrayIndex].IsDestroyed;
    }

    // =========================================================
    // CREATE ENEMY DOTS
    // =========================================================

    private EnemyDot CreateEnemyDot(RectTransform parent)
    {
        GameObject rootObject = new GameObject(
            "Enemy Threat Dot",
            typeof(RectTransform)
        );

        RectTransform root =
            rootObject.GetComponent<RectTransform>();

        root.SetParent(parent, false);
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(enemyDotSize, enemyDotSize);

        // Bright red outer glow.
        GameObject glowObject = new GameObject(
            "Red Glow",
            typeof(RectTransform),
            typeof(Image)
        );

        RectTransform glowRect =
            glowObject.GetComponent<RectTransform>();

        glowRect.SetParent(root, false);
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.pivot = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(enemyGlowSize, enemyGlowSize);

        Image glow = glowObject.GetComponent<Image>();
        glow.sprite = circleSprite;
        glow.color = new Color(1f, 0f, 0f, 0.95f);
        glow.raycastTarget = false;

        // Solid red centre.
        GameObject coreObject = new GameObject(
            "Bright Red Core",
            typeof(RectTransform),
            typeof(Image)
        );

        RectTransform coreRect =
            coreObject.GetComponent<RectTransform>();

        coreRect.SetParent(root, false);
        coreRect.anchorMin = new Vector2(0.5f, 0.5f);
        coreRect.anchorMax = new Vector2(0.5f, 0.5f);
        coreRect.pivot = new Vector2(0.5f, 0.5f);
        coreRect.sizeDelta = new Vector2(enemyDotSize, enemyDotSize);

        Image core = coreObject.GetComponent<Image>();
        core.sprite = circleSprite;
        core.color = enemyRed;
        core.raycastTarget = false;

        // Tiny white centre for visibility.
        GameObject centreObject = new GameObject(
            "Hot Centre",
            typeof(RectTransform),
            typeof(Image)
        );

        RectTransform centreRect =
            centreObject.GetComponent<RectTransform>();

        centreRect.SetParent(root, false);
        centreRect.anchorMin = new Vector2(0.3f, 0.3f);
        centreRect.anchorMax = new Vector2(0.7f, 0.7f);
        centreRect.offsetMin = Vector2.zero;
        centreRect.offsetMax = Vector2.zero;

        Image centre = centreObject.GetComponent<Image>();
        centre.sprite = circleSprite;
        centre.color = Color.white;
        centre.raycastTarget = false;

        return new EnemyDot
        {
            root = root,
            glow = glow,
            core = core,
            centre = centre,
            roomIndex = 0,
            pulseOffset = Random.Range(0f, 6.28f),
            sideOffset = Random.Range(-dotCentreOffset, dotCentreOffset)
        };
    }

    private void CreateCircleSprite()
    {
        const int textureSize = 64;

        Texture2D texture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );

        texture.name = "SecurityMinimap Circle";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 centre = new Vector2(
            (textureSize - 1) * 0.5f,
            (textureSize - 1) * 0.5f
        );

        float radius = textureSize * 0.48f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    centre
                );

                float alpha = Mathf.Clamp01(radius - distance + 0.5f);

                texture.SetPixel(
                    x,
                    y,
                    new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        texture.Apply();

        circleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            100f
        );

        circleSprite.name = "SecurityMinimap Circle Sprite";
    }

    // Filled images need a sprite to honour fillAmount reliably.
    private void CreateSolidSprite()
    {
        Texture2D white = Texture2D.whiteTexture;

        solidSprite = Sprite.Create(
            white,
            new Rect(0f, 0f, white.width, white.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        solidSprite.name = "SecurityMinimap Solid Sprite";
    }

    // =========================================================
    // POSITION DOTS USING AUTOMATIC ROOM BOUNDS
    // =========================================================

    private bool TryGetRoomBounds(int roomIndex, out Bounds bounds)
    {
        bounds = new Bounds();

        if (roomObjects == null ||
            roomIndex < 0 ||
            roomIndex >= roomObjects.Length ||
            roomObjects[roomIndex] == null)
        {
            return false;
        }

        GameObject room = roomObjects[roomIndex];

        Renderer[] renderers =
            room.GetComponentsInChildren<Renderer>(true);

        bool foundBounds = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer is ParticleSystemRenderer)
                continue;

            if (!foundBounds)
            {
                bounds = renderer.bounds;
                foundBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        // If there are no renderers, use colliders.
        if (!foundBounds)
        {
            Collider[] colliders =
                room.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                if (col == null)
                    continue;

                if (!foundBounds)
                {
                    bounds = col.bounds;
                    foundBounds = true;
                }
                else
                {
                    bounds.Encapsulate(col.bounds);
                }
            }
        }

        if (!foundBounds)
            return false;

        bounds.Expand(Mathf.Max(0f, roomBoundsPadding) * 2f);
        return true;
    }

    private void PositionEnemyDot(
        EnemyDot dot,
        int roomArrayIndex,
        EnemyHealth enemy)
    {
        if (dot == null || dot.root == null || enemy == null)
            return;

        if (roomArrayIndex < 0 || roomArrayIndex >= laneAreas.Length)
            return;

        if (!TryGetRoomBounds(roomArrayIndex, out Bounds bounds))
            return;

        Vector3 enemyPosition = enemy.transform.position;

        // Convert the enemy's position into its percentage across
        // the room's automatically detected world-space bounds.
        float x = Mathf.InverseLerp(
            bounds.min.x,
            bounds.max.x,
            enemyPosition.x
        );

        float z = Mathf.InverseLerp(
            bounds.min.z,
            bounds.max.z,
            enemyPosition.z
        );

        if (invertRoomDepth != null &&
            roomArrayIndex < invertRoomDepth.Length &&
            invertRoomDepth[roomArrayIndex])
        {
            z = 1f - z;
        }

        Rect laneRect = laneAreas[roomArrayIndex].rect;

        // Horizontal: stay near the centre of the lane. Each dot has its
        // own small fixed offset, plus a gentle nudge from the enemy's real
        // left/right position. Clamped so dots never touch the lane edges.
        float maxSide = Mathf.Max(
            0f,
            laneRect.width * 0.5f - enemyGlowSize * 0.5f
        );

        float mappedX =
            dot.sideOffset +
            (x - 0.5f) * laneRect.width * dotSideTracking;

        mappedX = Mathf.Clamp(mappedX, -maxSide, maxSide);

        // Vertical: use the area between the bottom of the lane and the
        // room label header so dots never overlap the text.
        float bottomEdge = -laneRect.height * 0.5f + 6f + enemyDotSize * 0.5f;
        float topEdge =
            laneRect.height * 0.5f - laneHeaderHeight - 2f - enemyDotSize * 0.5f;

        float mappedY = Mathf.Lerp(bottomEdge, topEdge, z);

        dot.root.anchoredPosition = new Vector2(mappedX, mappedY);
    }

    // =========================================================
    // UPDATE DOT POSITIONS AND PULSE
    // =========================================================

    private void UpdateEnemyDots()
    {
        for (int i = 0; i < laneEnemyCount.Length; i++)
            laneEnemyCount[i] = 0;

        int totalThreats = 0;

        foreach (KeyValuePair<EnemyHealth, EnemyDot> pair in enemyDots)
        {
            EnemyHealth enemy = pair.Key;
            EnemyDot dot = pair.Value;

            if (enemy == null || dot == null || dot.root == null)
                continue;

            if (enemy.IsDead)
            {
                dot.root.gameObject.SetActive(false);
                continue;
            }

            EnemyMover mover = GetEnemyMover(enemy);

            if (mover == null)
            {
                dot.root.gameObject.SetActive(false);
                continue;
            }

            int roomIndex = mover.CurrentRoomIndex;

            if (roomIndex < 1 || roomIndex > 4)
            {
                dot.root.gameObject.SetActive(false);
                continue;
            }

            int arrayIndex = roomIndex - 1;

            if (IsRoomShieldBroken(arrayIndex))
            {
                dot.root.gameObject.SetActive(false);
                continue;
            }

            if (dot.roomIndex != roomIndex)
            {
                dot.root.SetParent(laneAreas[arrayIndex], false);
                dot.roomIndex = roomIndex;
            }

            dot.root.gameObject.SetActive(true);

            laneEnemyCount[arrayIndex]++;
            totalThreats++;

            PositionEnemyDot(dot, arrayIndex, enemy);

            float pulse =
                (Mathf.Sin(Time.unscaledTime * enemyPulseSpeed +
                           dot.pulseOffset) + 1f) * 0.5f;

            if (dot.glow != null)
            {
                dot.glow.color = new Color(
                    1f,
                    0f,
                    0f,
                    Mathf.Lerp(0.55f, 1f, pulse)
                );

                float glowSize = Mathf.Lerp(
                    enemyGlowSize * 0.9f,
                    enemyGlowSize * 1.25f,
                    pulse
                );

                dot.glow.rectTransform.sizeDelta =
                    new Vector2(glowSize, glowSize);
            }

            if (dot.core != null)
            {
                dot.core.color = Color.Lerp(
                    enemyRed,
                    new Color(1f, 0.2f, 0.2f, 1f),
                    pulse * 0.25f
                );
            }
        }

        UpdateLaneAlerts();
        UpdateThreatCounter(totalThreats);
    }

    // Lane borders pulse red while enemies are inside the room.
    private void UpdateLaneAlerts()
    {
        float pulse =
            (Mathf.Sin(Time.unscaledTime * enemyPulseSpeed) + 1f) * 0.5f;

        for (int i = 0; i < 4; i++)
        {
            if (laneBorders[i] == null)
                continue;

            Shield shield =
                roomShields != null && i < roomShields.Length
                    ? roomShields[i]
                    : null;

            bool offline = shield == null || shield.IsDestroyed;

            if (offline)
            {
                SetBorderColour(laneBorders[i], new Color(
                    criticalColour.r,
                    criticalColour.g,
                    criticalColour.b,
                    0.35f
                ));
            }
            else if (laneEnemyCount[i] > 0)
            {
                SetBorderColour(laneBorders[i], new Color(
                    enemyRed.r,
                    enemyRed.g,
                    enemyRed.b,
                    Mathf.Lerp(0.4f, 1f, pulse)
                ));
            }
            else
            {
                SetBorderColour(laneBorders[i], new Color(0.05f, 0.5f, 0.6f, 0.35f));
            }
        }
    }

    private void UpdateThreatCounter(int totalThreats)
    {
        if (threatText == null)
            return;

        if (totalThreats > 0)
        {
            float pulse =
                (Mathf.Sin(Time.unscaledTime * enemyPulseSpeed) + 1f) * 0.5f;

            threatText.color = new Color(
                enemyRed.r,
                enemyRed.g,
                enemyRed.b,
                Mathf.Lerp(0.6f, 1f, pulse)
            );
        }
        else
        {
            threatText.color = healthyColour;
        }

        if (totalThreats == lastThreatCount)
            return;

        lastThreatCount = totalThreats;

        threatText.text = totalThreats > 0
            ? "THREATS: " + totalThreats
            : "ALL CLEAR";
    }

    private void RemoveEnemyDot(EnemyHealth enemy)
    {
        if (!enemyDots.TryGetValue(enemy, out EnemyDot dot))
            return;

        if (dot != null && dot.root != null)
            Destroy(dot.root.gameObject);

        enemyDots.Remove(enemy);
    }
}