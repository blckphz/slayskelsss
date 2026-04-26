using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for Hover events
using TMPro;

public class CraftUIManager : MonoBehaviour
{
    public static CraftUIManager Instance;

    [Header("UI Grids")]
    public Transform bgGridParent;
    public Transform iconGridParent;

    [Header("Visual Settings")]
    public float normalAlpha = 0.5f;
    public float hoverAlpha = 0.85f;    // New Hover state
    public float selectedAlpha = 1.0f;

    [Header("Selection Display")]
    public TextMeshProUGUI infoText;
    public Image largePreviewIcon;
    public Button craftButton;

    private List<Transform> bgSlots = new List<Transform>();
    private List<Transform> iconSlots = new List<Transform>();
    private CraftingSO selectedRecipe;
    private int currentSelectedIndex = -1; // Track which index is active

    private void Awake()
    {
        Instance = this;
        foreach (Transform child in bgGridParent) bgSlots.Add(child);
        foreach (Transform child in iconGridParent) iconSlots.Add(child);

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(() => {
                if (selectedRecipe != null) CraftingManager.Instance.CraftItem(selectedRecipe);
            });
        }
    }

    public void RefreshGrid()
    {
        if (CraftingManager.Instance == null) return;
        var recipes = CraftingManager.Instance.knownRecipes;
        currentSelectedIndex = -1; // Reset selection on refresh

        for (int i = 0; i < bgSlots.Count; i++)
        {
            if (i < recipes.Count)
            {
                int index = i;
                CraftingSO recipe = recipes[index];

                bgSlots[i].gameObject.SetActive(true);
                iconSlots[i].gameObject.SetActive(true);

                // Set initial alpha
                SetAlpha(bgSlots[i], normalAlpha);

                Image icon = iconSlots[i].GetComponent<Image>();
                if (icon != null) icon.sprite = recipe.resultItem.icon;

                // --- HOVER & CLICK LOGIC ---
                Button btn = bgSlots[i].GetComponent<Button>();
                if (btn == null) btn = iconSlots[i].GetComponent<Button>();

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectRecipe(recipe, index));

                    // Add EventTrigger for Hover
                    AddHoverEvents(bgSlots[i].gameObject, index);
                }
            }
            else
            {
                bgSlots[i].gameObject.SetActive(false);
                iconSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void AddHoverEvents(GameObject obj, int index)
    {
        // Ensure EventTrigger component exists
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = obj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        // Pointer Enter (Mouse Over)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnHoverEnter(index); });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit (Mouse Leave)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnHoverExit(index); });
        trigger.triggers.Add(entryExit);
    }

    private void OnHoverEnter(int index)
    {
        // Only change to 85% if it's NOT the currently selected one (which is 100%)
        if (index != currentSelectedIndex)
        {
            SetAlpha(bgSlots[index], hoverAlpha);
        }
    }

    private void OnHoverExit(int index)
    {
        // Return to normal if not selected, otherwise keep at 100%
        if (index != currentSelectedIndex)
        {
            SetAlpha(bgSlots[index], normalAlpha);
        }
    }

    public void SelectRecipe(CraftingSO recipe, int selectedIndex)
    {
        selectedRecipe = recipe;
        currentSelectedIndex = selectedIndex;

        if (largePreviewIcon != null) largePreviewIcon.sprite = recipe.resultItem.icon;

        // Reset all and set selected to 100%
        for (int i = 0; i < bgSlots.Count; i++)
        {
            SetAlpha(bgSlots[i], (i == selectedIndex) ? selectedAlpha : normalAlpha);
        }

        UpdateInfoText();
    }

    private void SetAlpha(Transform slot, float alpha)
    {
        Image img = slot.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    public void UpdateInfoText()
    {
        if (infoText == null || selectedRecipe == null) return;

        string txt = $"<b>{selectedRecipe.resultItem.itemName}</b>\n\n";
        foreach (var ing in selectedRecipe.ingredients)
        {
            int owned = InventoryManager.Instance.GetTotalCount(ing.item);
            string color = owned >= ing.amount ? "white" : "red";
            txt += $"<color={color}>{ing.item.itemName}: {owned}/{ing.amount}</color>\n";
        }
        infoText.text = txt;
    }
}