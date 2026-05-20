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

    // UPDATED: now accepts current durability
    public void ShowToolTip(ItemData item, float currentDurability)
    {
        if (item == null) return;

        titleText.text = $"<size=120%>{item.itemName}</size>";
        typeText.text = $"<size=90%>{item.itemType}</size>";
        descriptionText.text = $"<size=80%>{GenerateDescription(item, currentDurability)}</size>";

        iconImage.gameObject.SetActive(item.icon != null);

        if (item.icon != null)
            iconImage.sprite = item.icon;

        gameObject.SetActive(true);

        // Force text to update before measuring lines
        Canvas.ForceUpdateCanvases();

        int totalLines = descriptionText.textInfo.lineCount;
        float calculatedHeight = baseHeight + (totalLines * heightPerLine);

        if (backgroundRect != null)
        {
            backgroundRect.sizeDelta = new Vector2(
                backgroundRect.sizeDelta.x,
                calculatedHeight
            );
        }
    }

    public void HideToolTip()
    {
        gameObject.SetActive(false);
    }

    private string GenerateDescription(ItemData item, float currentDurability)
    {
        string desc = "";

        if (item is UseableItem u && u.abilityToExecute != null)
        {
            if (u.abilityToExecute is IItemDescriptionProvider p)
            {
                desc += p.GetDetailedDescription();
            }
            else
            {
                if (u.abilityToExecute is offensiveRanged r)
                    desc += $"Damage: {r.damage}\n";

                desc += $"Use Rate: {u.useRate}s\n";
            }
        }
        else if (item is IItemDescriptionProvider ip)
        {
            desc += ip.GetDetailedDescription();
        }

        // Show CURRENT durability instead of max only
        if (item.IsUnbreakable)
        {
            desc += "\nUnbreakable";
        }
        else
        {
            desc += $"\nDurability: {currentDurability}/{item.maxDurability}";
        }

        return desc;
    }
}