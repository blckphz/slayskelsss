using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class invToolTip : MonoBehaviour
{
    public static invToolTip Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;

    [Header("Background Settings")]
    [SerializeField] private RectTransform backgroundRect;

    [Header("Dynamic Scaling Settings")]
    [SerializeField] private float baseHeight = 80f;
    [SerializeField] private float heightPerLine = 25f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
            transform.position = Mouse.current.position.ReadValue();
    }

    // SHOW TOOLTIP
    public void ShowToolTip(ItemData item, float currentDurability)
    {
        if (item == null) return;

        titleText.text = $"<size=120%>{item.itemName}</size>";
        typeText.text = $"<size=90%>{item.itemType}</size>";

        // Wrapped in both <i> (cursive) and <color=#C0C0C0> (silver)
        descriptionText.text = $"<size=80%><color=#C0C0C0><i>{GenerateDescription(item, currentDurability)}</i></color></size>";

        iconImage.gameObject.SetActive(item.icon != null);
        if (item.icon != null) iconImage.sprite = item.icon;

        gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();

        int totalLines = descriptionText.textInfo.lineCount;
        float calculatedHeight = baseHeight + (totalLines * heightPerLine);

        if (backgroundRect != null)
        {
            backgroundRect.sizeDelta = new Vector2(backgroundRect.sizeDelta.x, calculatedHeight);
        }
    }

    // HIDE TOOLTIP
    public void HideToolTip()
    {
        gameObject.SetActive(false);
    }

    // GENERATE DESCRIPTION
    private string GenerateDescription(ItemData item, float currentDurability)
    {
        string desc = "";

        // Wrap the content in <i> tags so ONLY this text is cursive
        string content = "";

        // =====================================================
        // BASE ITEM DESCRIPTION
        // =====================================================
        if (!string.IsNullOrWhiteSpace(item.ItemDescription))
        {
            content += item.ItemDescription;
        }

        // =====================================================
        // USEABLE ITEM / ABILITY INFO
        // =====================================================
        if (item is UseableItem u && u.abilityToExecute != null)
        {
            if (!string.IsNullOrEmpty(content))
                content += "\n\n";

            if (u.abilityToExecute is IItemDescriptionProvider p)
            {
                content += p.GetDetailedDescription();
            }
            else
            {
                if (u.abilityToExecute is offensiveRanged r)
                    content += $"Damage: {r.damage}\n";

                content += $"Use Rate: {u.useRate}s";
            }
        }
        else if (item is IItemDescriptionProvider ip)
        {
            if (!string.IsNullOrEmpty(content))
                content += "\n\n";

            content += ip.GetDetailedDescription();
        }

        // =====================================================
        // DURABILITY
        // =====================================================
        if (!string.IsNullOrEmpty(content))
            content += "\n\n";

        if (item.IsUnbreakable)
        {
            content += "Unbreakable";
        }
        else
        {
            content += $"Durability: {currentDurability}/{item.maxDurability}";
        }

        // Apply italics to the accumulated content block
        return $"<i>{content.Trim()}</i>";
    }
}