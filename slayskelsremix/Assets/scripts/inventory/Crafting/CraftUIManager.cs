using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class CraftUIManager : MonoBehaviour
{
    public static CraftUIManager Instance;

    [Header("Recipe Database")]
    public List<CraftingSO> allRecipes = new List<CraftingSO>();

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
    [Range(0, 1)] public float normalAlpha = 0.5f; // Initial/Default transparency
    [Range(0, 1)] public float selectedAlpha = 1.0f; // Full transparency (opaque)

    [Header("Input System")]
    public InputActionReference nextPageAction;
    public InputActionReference prevPageAction;

    private CraftingSO selectedRecipe;
    private int selectedSlotIndex = -1; // Tracks which slot in the current view is highlighted

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
        RefreshGrid();
        SelectRecipe(null);
    }

    private void OnNextPage(InputAction.CallbackContext ctx) => NextPage();
    private void OnPrevPage(InputAction.CallbackContext ctx) => PrevPage();

    public void NextPage()
    {
        if ((currentPage + 1) * recipesPerPage < allRecipes.Count)
        {
            currentPage++;
            selectedSlotIndex = -1; // Reset selection visual on page change
            RefreshGrid();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            selectedSlotIndex = -1; // Reset selection visual on page change
            RefreshGrid();
        }
    }

    public void RefreshGrid()
    {
        int startIndex = currentPage * recipesPerPage;
        int endIndex = Mathf.Min(startIndex + recipesPerPage, allRecipes.Count);
        int slotIndex = 0;

        for (int i = startIndex; i < endIndex; i++)
        {
            CraftingSO recipe = allRecipes[i];
            Transform bgSlot = bgSlots[slotIndex];
            Transform iconSlot = iconSlots[slotIndex];

            bgSlot.gameObject.SetActive(true);
            iconSlot.gameObject.SetActive(true);

            // Handle Selection Logic
            int capturedIndex = slotIndex;
            Button btn = bgSlot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    selectedSlotIndex = capturedIndex; // Store which slot was clicked
                    SelectRecipe(recipe);
                });
            }

            // ICON
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

        // Refresh the alphas whenever a selection is made
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
                // If this is the selected index, set alpha to 1.0 (100%), otherwise set to normalAlpha
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
            SelectRecipe(selectedRecipe);
            RefreshGrid();
        }
    }
}