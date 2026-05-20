using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CraftUIManager : MonoBehaviour
{
    public static CraftUIManager Instance;

    [Header("UI Grids")]
    public Transform bgGridParent;
    public Transform iconGridParent;

    [Header("Visual Settings")]
    public float normalAlpha = 0.5f;
    public float hoverAlpha = 0.85f;
    public float selectedAlpha = 1.0f;

    [Header("Selection Display")]
    public TextMeshProUGUI infoText;
    public Image largePreviewIcon;
    public Button craftButton;

    [Header("Search")]
    public TMP_InputField searchField;

    private List<Transform> bgSlots = new List<Transform>();
    private List<Transform> iconSlots = new List<Transform>();

    private CraftingSO selectedRecipe;
    private int currentSelectedIndex = -1;

    private RecipeType? currentFilter = null; // null = All
    private string currentSearch = "";

    private void Awake()
    {
        Instance = this;

        foreach (Transform child in bgGridParent) bgSlots.Add(child);
        foreach (Transform child in iconGridParent) iconSlots.Add(child);

        Debug.Log("[CraftUI] Awake - Slots loaded: " + bgSlots.Count);

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(() =>
            {
                Debug.Log("[CraftUI] Craft button pressed");

                if (selectedRecipe != null)
                {
                    Debug.Log("[CraftUI] Crafting: " + selectedRecipe.name);
                    CraftingManager.Instance.CraftItem(selectedRecipe);
                }
                else
                {
                    Debug.LogWarning("[CraftUI] No recipe selected!");
                }
            });
        }

        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchChanged);
        }
    }

    // =========================
    // SEARCH
    // =========================
    private void OnSearchChanged(string value)
    {
        currentSearch = value.ToLower().Trim();
        RefreshGrid();
    }

    // =========================
    // FILTER BUTTON CALLS
    // =========================
    public void SetFilter(int filter)
    {
        RecipeType newFilter = filter switch
        {
            1 => RecipeType.Deco,
            2 => RecipeType.Functionality,
            _ => RecipeType.Tool
        };

        Debug.Log("[CraftUI] Filter button pressed: " + newFilter);

        if (currentFilter != null && currentFilter == newFilter)
        {
            currentFilter = null;
            Debug.Log("[CraftUI] Filter toggled OFF → showing ALL recipes");
        }
        else
        {
            currentFilter = newFilter;
            Debug.Log("[CraftUI] Filter set to: " + currentFilter);
        }

        RefreshGrid();
    }

    // =========================
    // GRID
    // =========================
    public void RefreshGrid()
    {
        if (CraftingManager.Instance == null)
        {
            Debug.LogError("[CraftUI] CraftingManager missing!");
            return;
        }

        Debug.Log("[CraftUI] RefreshGrid called");

        List<CraftingSO> recipes = new List<CraftingSO>();

        foreach (var r in CraftingManager.Instance.knownRecipes)
        {
            if (r == null) continue;

            // TYPE FILTER
            if (currentFilter != null && r.recipeType != currentFilter)
                continue;

            // SEARCH FILTER
            if (!string.IsNullOrEmpty(currentSearch))
            {
                string recipeName = r.name.ToLower();
                string itemName = r.resultItem.itemName.ToLower();

                if (!recipeName.Contains(currentSearch) &&
                    !itemName.Contains(currentSearch))
                    continue;
            }

            recipes.Add(r);
        }

        Debug.Log("[CraftUI] Recipes after filter: " + recipes.Count);

        currentSelectedIndex = -1;
        selectedRecipe = null;

        for (int i = 0; i < bgSlots.Count; i++)
        {
            if (i < recipes.Count)
            {
                CraftingSO recipe = recipes[i];

                bgSlots[i].gameObject.SetActive(true);
                iconSlots[i].gameObject.SetActive(true);

                SetAlpha(bgSlots[i], normalAlpha);

                Image icon = iconSlots[i].GetComponent<Image>();
                if (icon != null)
                    icon.sprite = recipe.resultItem.icon;

                Button btn = bgSlots[i].GetComponent<Button>();
                if (btn == null)
                    btn = iconSlots[i].GetComponent<Button>();

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    int index = i;

                    btn.onClick.AddListener(() =>
                    {
                        Debug.Log("[CraftUI] Selected recipe: " + recipe.name);
                        SelectRecipe(recipe, index);
                    });

                    AddHoverEvents(bgSlots[i].gameObject, i);
                }
            }
            else
            {
                bgSlots[i].gameObject.SetActive(false);
                iconSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // =========================
    // HOVER EVENTS
    // =========================
    private void AddHoverEvents(GameObject obj, int index)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null) trigger = obj.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        enter.callback.AddListener((data) =>
        {
            OnHoverEnter(index);
        });
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        exit.callback.AddListener((data) =>
        {
            OnHoverExit(index);
        });
        trigger.triggers.Add(exit);
    }

    private void OnHoverEnter(int index)
    {
        if (index != currentSelectedIndex)
            SetAlpha(bgSlots[index], hoverAlpha);
    }

    private void OnHoverExit(int index)
    {
        if (index != currentSelectedIndex)
            SetAlpha(bgSlots[index], normalAlpha);
    }

    // =========================
    // SELECT RECIPE
    // =========================
    public void SelectRecipe(CraftingSO recipe, int selectedIndex)
    {
        selectedRecipe = recipe;
        currentSelectedIndex = selectedIndex;

        if (largePreviewIcon != null)
            largePreviewIcon.sprite = recipe.resultItem.icon;

        for (int i = 0; i < bgSlots.Count; i++)
        {
            SetAlpha(bgSlots[i], (i == selectedIndex) ? selectedAlpha : normalAlpha);
        }

        UpdateInfoText();
    }

    // =========================
    // UI HELPERS
    // =========================
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