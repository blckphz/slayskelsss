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

    [Header("Rotation Settings")]
    public float maxRotation = 8f;
    public float rotationLerpSpeed = 12f;

    [Header("Selection Display")]
    public TextMeshProUGUI infoText;
    public Image largePreviewIcon;
    public Button craftButton;

    [Header("Search")]
    public TMP_InputField searchField;

    [Header("Audio")]
    public AudioClip craftSfx;
    public AudioClip selectSfx;

    private List<Transform> bgSlots = new List<Transform>();
    private List<RectTransform> bgRects = new List<RectTransform>();
    private List<Quaternion> targetRotations = new List<Quaternion>();
    private List<Transform> iconSlots = new List<Transform>();

    private CraftingSO selectedRecipe;
    private int currentSelectedIndex = -1;

    private RecipeType? currentFilter = null;
    private string currentSearch = "";

    private void Awake()
    {
        Instance = this;

        foreach (Transform child in bgGridParent)
        {
            bgSlots.Add(child);
            bgRects.Add(child.GetComponent<RectTransform>());
            targetRotations.Add(Quaternion.identity);
        }

        foreach (Transform child in iconGridParent)
        {
            iconSlots.Add(child);
        }

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(() =>
            {
                if (selectedRecipe != null)
                {
                    CraftingManager.Instance.CraftItem(selectedRecipe);

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayUISound(craftSfx);
                    }
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

    private void Update()
    {
        for (int i = 0; i < bgRects.Count; i++)
        {
            if (bgRects[i] != null)
            {
                bgRects[i].rotation = Quaternion.Lerp(
                    bgRects[i].rotation,
                    targetRotations[i],
                    Time.unscaledDeltaTime * rotationLerpSpeed
                );
            }
        }
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshInfoOnly;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshInfoOnly;
        }
    }

    private void RefreshInfoOnly()
    {
        UpdateInfoText();
    }

    private void OnSearchChanged(string value)
    {
        currentSearch = value.ToLower().Trim();
        RefreshGrid();
    }

    public void SetFilter(int filter)
    {
        RecipeType newFilter = filter switch
        {
            1 => RecipeType.Deco,
            2 => RecipeType.Functionality,
            _ => RecipeType.Tool
        };

        currentFilter =
            (currentFilter != null && currentFilter == newFilter)
            ? null
            : newFilter;

        RefreshGrid();
    }

    public void RefreshGrid()
    {
        if (CraftingManager.Instance == null) return;

        CraftingSO previousSelected = selectedRecipe;

        List<CraftingSO> source = CraftingManager.Instance.ActiveRecipes;
        List<CraftingSO> recipes = new List<CraftingSO>();

        foreach (var r in source)
        {
            if (r == null) continue;

            if (currentFilter != null && r.recipeType != currentFilter)
                continue;

            if (!string.IsNullOrEmpty(currentSearch))
            {
                if (!r.name.ToLower().Contains(currentSearch) &&
                    !r.resultItem.itemName.ToLower().Contains(currentSearch))
                {
                    continue;
                }
            }

            recipes.Add(r);
        }

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
                targetRotations[i] = Quaternion.identity;

                Image icon = iconSlots[i].GetComponent<Image>();

                if (icon != null)
                {
                    icon.sprite = recipe.resultItem.icon;
                }

                Button btn =
                    bgSlots[i].GetComponent<Button>() ??
                    iconSlots[i].GetComponent<Button>();

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();

                    int index = i;

                    btn.onClick.AddListener(() =>
                    {
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

        if (previousSelected != null)
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i] == previousSelected)
                {
                    SelectRecipe(previousSelected, i);
                    break;
                }
            }
        }
    }

    private void AddHoverEvents(GameObject obj, int index)
    {
        EventTrigger trigger =
            obj.GetComponent<EventTrigger>() ??
            obj.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry enter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };

        enter.callback.AddListener((data) => OnHoverEnter(index));
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };

        exit.callback.AddListener((data) => OnHoverExit(index));
        trigger.triggers.Add(exit);
    }

    private void OnHoverEnter(int index)
    {
        if (index != currentSelectedIndex)
        {
            SetAlpha(bgSlots[index], hoverAlpha);

            targetRotations[index] = Quaternion.Euler(
                0f,
                0f,
                Random.Range(-maxRotation, maxRotation)
            );
        }
    }

    private void OnHoverExit(int index)
    {
        if (index != currentSelectedIndex)
        {
            SetAlpha(bgSlots[index], normalAlpha);
            targetRotations[index] = Quaternion.identity;
        }
    }

    public void SelectRecipe(CraftingSO recipe, int selectedIndex)
    {
        selectedRecipe = recipe;
        currentSelectedIndex = selectedIndex;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISound(selectSfx);
        }

        if (largePreviewIcon != null)
        {
            largePreviewIcon.sprite = recipe.resultItem.icon;
        }

        for (int i = 0; i < bgSlots.Count; i++)
        {
            SetAlpha(
                bgSlots[i],
                (i == selectedIndex) ? selectedAlpha : normalAlpha
            );

            targetRotations[i] = Quaternion.identity;
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
        if (infoText == null || selectedRecipe == null)
            return;

        string txt =
            $"<b>{selectedRecipe.resultItem.itemName}</b>\n\n";

        foreach (var ing in selectedRecipe.ingredients)
        {
            int owned = 0;

            var items = InventoryManager.Instance.GetItems(ing.item);

            foreach (var instance in items)
            {
                if (instance.CurrentDurability >= instance.MaxDurability)
                {
                    owned += instance.StackCount;
                }
            }

            string color =
                owned >= ing.amount ? "white" : "red";

            txt +=
                $"<color={color}>{ing.item.itemName}: {owned}/{ing.amount}</color>\n";
        }

        infoText.text = txt;
    }
}