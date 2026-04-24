using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq; // Required for filtering

public class CraftUIManager : MonoBehaviour
{
    public static CraftUIManager Instance;

    [Header("Recipe Database")]
    public List<CraftingSO> allRecipes = new List<CraftingSO>();
    private List<CraftingSO> filteredRecipes = new List<CraftingSO>();

    [Header("Search Settings")]
    public TMP_InputField searchInputField;

    [Header("Pagination Settings")]
    public int recipesPerPage = 10;
    private int currentPage = 0;

    [Header("UI Grids")]
    public Transform bgGridParent;
    public Transform iconGridParent;

    [Header("Selection UI")]
    public TextMeshProUGUI selectedNameText;
    public TextMeshProUGUI selectedDescriptionText;
    public Image selectedIconImage;
    public Button craftButton;

    [Header("Visual Settings")]
    [Range(0, 1)] public float normalAlpha = 0.5f;
    [Range(0, 1)] public float selectedAlpha = 1.0f;

    [Header("Input System")]
    public InputActionReference nextPageAction;
    public InputActionReference prevPageAction;

    private CraftingSO selectedRecipe;
    private int selectedSlotIndex = -1;

    private List<Transform> bgSlots = new List<Transform>();
    private List<Transform> iconSlots = new List<Transform>();

    private void Awake()
    {
        Instance = this;

        foreach (Transform child in bgGridParent)
        {
            bgSlots.Add(child);
        }

        foreach (Transform child in iconGridParent)
        {
            iconSlots.Add(child);
        }
    }

    private void OnEnable()
    {
        if (nextPageAction != null)
        {
            nextPageAction.action.performed += OnNextPage;
            nextPageAction.action.Enable();
        }

        if (prevPageAction != null)
        {
            prevPageAction.action.performed += OnPrevPage;
            prevPageAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (nextPageAction != null)
        {
            nextPageAction.action.performed -= OnNextPage;
            nextPageAction.action.Disable();
        }

        if (prevPageAction != null)
        {
            prevPageAction.action.performed -= OnPrevPage;
            prevPageAction.action.Disable();
        }
    }

    private void Start()
    {
        craftButton.onClick.AddListener(OnCraftClicked);

        // Initialize filtered list with everything
        filteredRecipes = new List<CraftingSO>(allRecipes);

        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
        }

        RefreshGrid();
        SelectRecipe(null);
    }

    private void OnNextPage(InputAction.CallbackContext ctx) => NextPage();
    private void OnPrevPage(InputAction.CallbackContext ctx) => PrevPage();

    public void OnSearchInputChanged(string value)
    {
        currentPage = 0; // Reset to first page on new search

        if (string.IsNullOrEmpty(value))
        {
            filteredRecipes = new List<CraftingSO>(allRecipes);
        }
        else
        {
            // Case-insensitive search through result item names
            filteredRecipes = allRecipes.Where(recipe =>
                recipe.resultItem != null &&
                recipe.resultItem.itemName.ToLower().Contains(value.ToLower())
            ).ToList();
        }

        selectedSlotIndex = -1;
        SelectRecipe(null);
        RefreshGrid();
    }

    public void NextPage()
    {
        if ((currentPage + 1) * recipesPerPage < filteredRecipes.Count)
        {
            currentPage++;
            selectedSlotIndex = -1;
            RefreshGrid();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            selectedSlotIndex = -1;
            RefreshGrid();
        }
    }

    public void RefreshGrid()
    {
        int startIndex = currentPage * recipesPerPage;
        int endIndex = Mathf.Min(startIndex + recipesPerPage, filteredRecipes.Count);
        int slotIndex = 0;

        for (int i = startIndex; i < endIndex; i++)
        {
            CraftingSO recipe = filteredRecipes[i];
            Transform bgSlot = bgSlots[slotIndex];
            Transform iconSlot = iconSlots[slotIndex];

            bgSlot.gameObject.SetActive(true);
            iconSlot.gameObject.SetActive(true);

            int capturedIndex = slotIndex;
            Button btn = bgSlot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    selectedSlotIndex = capturedIndex;
                    SelectRecipe(recipe);
                });
            }

            Image icon = iconSlot.GetComponent<Image>();
            if (icon != null)
            {
                if (recipe.resultItem != null && recipe.resultItem.icon != null)
                {
                    icon.sprite = recipe.resultItem.icon;
                    icon.enabled = true;
                }
                else icon.enabled = false;

                icon.raycastTarget = false;
            }

            slotIndex++;
        }

        for (int i = slotIndex; i < bgSlots.Count; i++)
        {
            bgSlots[i].gameObject.SetActive(false);
            iconSlots[i].gameObject.SetActive(false);
        }

        UpdateSlotVisuals();
    }

    public void SelectRecipe(CraftingSO recipe)
    {
        selectedRecipe = recipe;

        if (recipe == null)
        {
            selectedSlotIndex = -1;
            selectedNameText.text = "Select a Recipe";
            selectedDescriptionText.text = "";
            selectedIconImage.enabled = false;
            craftButton.interactable = false;
            UpdateSlotVisuals();
            return;
        }

        selectedNameText.text = recipe.resultItem != null ? recipe.resultItem.itemName : "Unknown Item";
        selectedDescriptionText.text = "Requires:" + GetIngredientsString(recipe);

        if (recipe.resultItem != null && recipe.resultItem.icon != null)
        {
            selectedIconImage.sprite = recipe.resultItem.icon;
            selectedIconImage.enabled = true;
        }
        else selectedIconImage.enabled = false;

        craftButton.interactable = recipe.CanCraft();
        UpdateSlotVisuals();
    }

    private void UpdateSlotVisuals()
    {
        for (int i = 0; i < bgSlots.Count; i++)
        {
            Image img = bgSlots[i].GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = (i == selectedSlotIndex) ? selectedAlpha : normalAlpha;
                img.color = c;
            }
        }
    }

    private string GetIngredientsString(CraftingSO recipe)
    {
        string text = "";
        foreach (var ing in recipe.ingredients)
        {
            int current = InventoryManager.Instance.GetTotalCount(ing.item);
            text += $"\n- {ing.item.itemName}: {current}/{ing.amount}";
        }
        return text;
    }

    private void OnCraftClicked()
    {
        if (selectedRecipe != null && selectedRecipe.CanCraft())
        {
            selectedRecipe.Craft();
            SelectRecipe(selectedRecipe); // Refresh UI states
            RefreshGrid();
        }
    }
}