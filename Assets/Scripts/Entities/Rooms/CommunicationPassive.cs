using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectBreach.Core;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Communication room passive (design doc §2.1): while the room is online,
    /// shows a small always-on-screen minimap window. Each external room gets a
    /// row showing its barrier HP, structure (turret/trap) status, and live
    /// enemy count. The whole window hides when the Communication room falls,
    /// and also hides while the security camera is open.
    ///
    /// Enemy positions are intentionally NOT shown — only the number of enemies
    /// per room. Enemies are attributed by their spawn room reference
    /// (<see cref="EnemyMover.SpawnRoom"/>) for reliable counting.
    /// </summary>
    public class CommunicationPassive : RoomPassive
    {
        [Header("Minimap Window")]
        [SerializeField] private float windowWidth = 230f;
        [SerializeField] private float titleHeight = 26f;
        [SerializeField] private float rowHeight = 30f;

        [Header("Colours")]
        [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color rowColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
        [SerializeField] private Color fallenColor = new Color(0.75f, 0.12f, 0.12f, 0.55f);
        [SerializeField] private Color hpFullColor = new Color(0.35f, 0.85f, 1f, 0.9f);
        [SerializeField] private Color onlineColor = new Color(0.3f, 0.9f, 0.3f, 0.95f);
        [SerializeField] private Color offlineColor = new Color(0.45f, 0.45f, 0.45f, 0.9f);

        [Header("Room Icons (optional)")]
        [Tooltip("Assign a sprite per room type to show an icon next to the short name. Leave empty for text only.")]
        [SerializeField] private Sprite iconComms;
        [SerializeField] private Sprite iconGenerator;
        [SerializeField] private Sprite iconArmoury;
        [SerializeField] private Sprite iconRegen;

        private Canvas _canvas;
        private RectTransform _window;
        private readonly List<MinimapRow> _rows = new List<MinimapRow>();

        private struct MinimapRow
        {
            public Room room;
            public Image hpFill;
            public Image statusDot;
            public Text enemyText;
            public Image fallenOverlay;
        }

        protected override void Awake()
        {
            base.Awake();
            LoadIcons();
            BuildWindow();
        }

        protected override void Update()
        {
            if (_window == null)
                return;

            // Hidden while the security camera is open (so the radar never
            // overlaps the camera view) and when the Communication room falls.
            bool visible = IsEnabled && !CameraUIController.IsSecurityCameraOpen;

            _window.gameObject.SetActive(visible);
            if (visible)
                RefreshAll();
        }

        // ============================================================
        // WINDOW BUILD
        // ============================================================

        private void BuildWindow()
        {
            _canvas = FindFirstObjectByType<Canvas>();
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("CommunicationMinimapCanvas");
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
            }

            _window = CreatePanel("CommunicationMinimap", _canvas.transform, panelColor);
            _window.anchorMin = new Vector2(1f, 1f);
            _window.anchorMax = new Vector2(1f, 1f);
            _window.pivot = new Vector2(1f, 1f);
            _window.anchoredPosition = new Vector2(-12f, -12f);

            // Title.
            Text title = CreateText(_window, "MAP", 14, Color.white, TextAnchor.MiddleLeft);
            SetTopLeft(title.rectTransform, 6f, 0f, windowWidth - 12f, titleHeight);

            // One row per external room (skip blank / leftover room names).
            int index = 0;
            foreach (Room room in FindObjectsByType<Room>(FindObjectsSortMode.None))
            {
                if (room == null || string.IsNullOrWhiteSpace(room.RoomName))
                    continue;

                BuildRow(room, index);
                index++;
            }

            // Legend explaining each minimap element.
            string legendText =
                "\u25A0 green: turret/trap online\n" +
                "\u25AC blue bar: barrier HP\n" +
                "number: enemies in this room";

            Text legend = CreateText(
                _window,
                legendText,
                11,
                new Color(1f, 1f, 1f, 0.85f),
                TextAnchor.MiddleLeft);
            SetTopLeft(
                legend.rectTransform,
                6f,
                titleHeight + index * rowHeight + 4f,
                windowWidth - 12f,
                46f);

            float height = titleHeight + index * rowHeight + 54f;
            _window.sizeDelta = new Vector2(windowWidth, height);
        }

        private void BuildRow(Room room, int index)
        {
            RectTransform row = CreatePanel("Row_" + room.RoomName, _window, rowColor);
            SetTopLeft(row, 0f, titleHeight + index * rowHeight, windowWidth, rowHeight);

            float left = 8f;

            // Shortened room name (left), with an optional icon before it.
            (string label, Sprite icon) info = GetRoomInfo(room);
            float nameX = left;
            float nameWidth = 80f;

            if (info.icon != null)
            {
                Image icon = CreateImage(row, Color.white);
                SetTopLeft(icon.rectTransform, nameX, (rowHeight - 16f) * 0.5f, 16f, 16f);
                icon.sprite = info.icon;
                icon.type = Image.Type.Simple;
                nameX += 20f;
                nameWidth -= 20f;
            }

            Text nameText = CreateText(row, info.label, 13, Color.white, TextAnchor.MiddleLeft);
            SetTopLeft(nameText.rectTransform, nameX, 0f, nameWidth, rowHeight);

            // Barrier HP bar (middle).
            Image hpBar = CreateImage(row, hpFullColor);
            SetTopLeft(hpBar.rectTransform, left + 84f, (rowHeight - 10f) * 0.5f, 66f, 10f);

            // Enemy count (right-aligned, red for visibility).
            Text enemyText = CreateText(row, "0", 13, new Color(1f, 0.35f, 0.35f, 1f), TextAnchor.MiddleRight);
            SetTopLeft(enemyText.rectTransform, left + 154f, 0f, 40f, rowHeight);

            // Structure status dot (far right).
            Image statusDot = CreateImage(row, offlineColor);
            SetTopLeft(statusDot.rectTransform, left + 200f, (rowHeight - 12f) * 0.5f, 12f, 12f);

            // Fallen overlay (covers the whole row).
            Image fallenOverlay = CreateImage(row, fallenColor);
            SetTopLeft(fallenOverlay.rectTransform, 0f, 0f, windowWidth, rowHeight);

            _rows.Add(new MinimapRow
            {
                room = room,
                hpFill = hpBar,
                statusDot = statusDot,
                enemyText = enemyText,
                fallenOverlay = fallenOverlay
            });
        }

        // ============================================================
        // REFRESH
        // ============================================================

        private void RefreshAll()
        {
            foreach (MinimapRow row in _rows)
            {
                if (row.room == null)
                    continue;

                bool fallen = row.room.State != RoomState.Active;

                // Barrier HP.
                Shield shield = GetShield(row.room);
                if (row.hpFill != null)
                {
                    float ratio = 1f;
                    if (shield != null)
                        ratio = shield.IsDestroyed
                            ? 0f
                            : Mathf.Clamp01((float)shield.CurrentHealth / Mathf.Max(1, shield.MaxHealth));
                    row.hpFill.fillAmount = ratio;
                    row.hpFill.color = fallen ? Color.gray : hpFullColor;
                }

                // Structure status (any turret/trap online?).
                bool anyOnline = false;
                foreach (DefensiveStructure s in row.room.Structures)
                {
                    if (s != null && s.IsActive) { anyOnline = true; break; }
                }
                if (row.statusDot != null)
                {
                    row.statusDot.color = anyOnline ? onlineColor : offlineColor;
                    row.statusDot.gameObject.SetActive(!fallen);
                }

                // Enemy count.
                int count = CountEnemies(row.room);
                if (row.enemyText != null)
                    row.enemyText.text = count.ToString();

                // Fallen overlay.
                if (row.fallenOverlay != null)
                {
                    row.fallenOverlay.gameObject.SetActive(fallen);
                    row.fallenOverlay.color = fallenColor;
                }
            }
        }

        private Shield GetShield(Room room)
        {
            foreach (Shield s in FindObjectsByType<Shield>(FindObjectsSortMode.None))
            {
                if (s != null && s.AssignedRoom == room)
                    return s;
            }
            return null;
        }

        private int CountEnemies(Room room)
        {
            int count = 0;
            foreach (EnemyMover mover in FindObjectsByType<EnemyMover>(FindObjectsSortMode.None))
            {
                if (mover == null)
                    continue;

                EnemyHealth health = mover.GetComponent<EnemyHealth>();
                if (health != null && health.IsDead)
                    continue;

                if (mover.SpawnRoom == room)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Loads room icon sprites from Resources/RoomIcons automatically, so
        /// icons work without dragging sprites onto this runtime-added
        /// component. An assigned serialized sprite always wins over the
        /// Resources fallback.
        /// </summary>
        private void LoadIcons()
        {
            iconComms = ResolveIcon(iconComms, "comms");
            iconGenerator = ResolveIcon(iconGenerator, "generator");
            iconArmoury = ResolveIcon(iconArmoury, "armory");
            iconRegen = ResolveIcon(iconRegen, "regen");
        }

        /// <summary>
        /// Resolves a room icon sprite. An assigned sprite wins; otherwise the
        /// sprite is loaded from Assets/Art/Images/{fileName}.png at editor time.
        /// </summary>
        private static Sprite ResolveIcon(Sprite assigned, string fileName)
        {
            if (assigned != null)
                return assigned;

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Art/Images/" + fileName + ".png");
#else
            return null;
#endif
        }

        /// <summary>
        /// Returns the shortened display label (and optional icon sprite) for a
        /// room, based on its normalized name. Unknown rooms keep their full name.
        /// </summary>
        private (string label, Sprite icon) GetRoomInfo(Room room)
        {
            string key = Normalize(room.RoomName);
            switch (key)
            {
                case "corridor":   return ("Comms", iconComms);
                case "serverroom": return ("Generator", iconGenerator);
                case "shop":       return ("Armory", iconArmoury);
                case "airlock":    return ("Regen", iconRegen);
                default:           return (room.RoomName, null);
            }
        }

        private static string Normalize(string name)
        {
            return name == null ? "" : name.Replace(" ", "").ToLowerInvariant();
        }

        // ============================================================
        // UGUI HELPERS
        // ============================================================

        private static Font GetFont()
        {
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null)
                f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return f;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);

            Image img = go.GetComponent<Image>();
            img.color = color;

            return rt;
        }

        private static Text CreateText(RectTransform parent, string content, int size, Color color, TextAnchor anchor)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);

            Text text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.font = GetFont();
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        private static Image CreateImage(RectTransform parent, Color color)
        {
            GameObject go = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);

            Image img = go.GetComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;

            return img;
        }

        /// <summary>Places a rect with its top-left at (x, y) inside the parent, sized (w, h).</summary>
        private static void SetTopLeft(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
        }
    }
}
