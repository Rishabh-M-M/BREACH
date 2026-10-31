using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectBreach.Core;

/// <summary>
/// UI slot for one TrapGroup. TrapGroup handles Power allocation/refunds
/// and activation of its own trap tiles.
/// </summary>
public class TrapEquipmentSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI slotText;

    [Header("Trap")]
    [SerializeField] private TrapGroup targetTrap;

    [Header("Display")]
    [SerializeField] private string equipmentName = "TRAP 1";

    [Header("Button Colour Tint")]
    [SerializeField] private Color normalColour = new Color(0.051f, 0.110f, 0.169f);
    [SerializeField] private Color highlightedColour = new Color(0.094f, 0.212f, 0.302f);
    [SerializeField] private Color pressedColour = new Color(0f, 0.75f, 1f);
    [SerializeField] private Color selectedColour = new Color(0.059f, 0.259f, 0.388f);
    [SerializeField] private Color disabledColour = new Color(0.149f, 0.149f, 0.169f);

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        ApplyButtonColours();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);
    }

    private void Update()
    {
        Refresh();
    }

    private void ApplyButtonColours()
    {
        if (button == null)
            return;

        ColorBlock colours = button.colors;
        colours.normalColor = normalColour;
        colours.highlightedColor = highlightedColour;
        colours.pressedColor = pressedColour;
        colours.selectedColor = selectedColour;
        colours.disabledColor = disabledColour;
        colours.colorMultiplier = 1f;
        colours.fadeDuration = 0.1f;
        button.colors = colours;
    }

    private void OnClicked()
    {
        if (targetTrap == null)
            return;

        targetTrap.Toggle();
        Refresh();

        if (button != null)
            button.OnDeselect(null);
    }

    public void Refresh()
    {
        if (button == null)
            return;

        if (targetTrap == null)
        {
            button.interactable = false;
            if (slotText != null)
                slotText.text =
                    $"<color=#B0C4DE>{equipmentName}</color>\n" +
                    "<color=#777777>— EMPTY —</color>";
            return;
        }

        button.interactable = true;

        if (targetTrap.IsGroupActive)
        {
            if (slotText != null)
                slotText.text =
                    $"<color=#B0C4DE>{equipmentName}</color>\n" +
                    "<color=#33FF66>○ ON</color>\n" +
                    $"<color=#55AAFF>POWER {targetTrap.PowerCost}</color>";
        }
        else
        {
            if (slotText != null)
                slotText.text =
                    $"<color=#B0C4DE>{equipmentName}</color>\n" +
                    "<color=#FF3333>○ OFF</color>\n" +
                    $"<color=#55AAFF>POWER {targetTrap.PowerCost}</color>";
        }
    }

    public void SetTrap(TrapGroup trap, string displayName)
    {
        targetTrap = trap;
        equipmentName = displayName;
        Refresh();
    }

    public void SetEmpty(string displayName)
    {
        targetTrap = null;
        equipmentName = displayName;
        Refresh();
    }
}
