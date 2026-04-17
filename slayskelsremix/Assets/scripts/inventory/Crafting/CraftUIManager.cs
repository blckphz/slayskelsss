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

    [Header("Input System")]
    public InputActionReference nextPageAction;
    public InputActionReference prevPageAction;

    private CraftingSO selectedRecipe;

    private List<Transform> bgSlots = new List<Transform>();
    private List<Transform> iconSlots = new List<Transform>();

    private void Awake()
    {
        Instance = this;

        Debug.Log($"[CraftUI] Awake - Initializing Craft UI Manager");

        foreach (Transform child in bgGridParent)
        {
            bgSlots.Add(child);
            Debug.Log($"[CraftUI] BG Slot Cached: {child.name}");
        }

        foreach (Transform child in iconGridParent)
        {
            iconSlots.Add(child);
            Debug.Log($"[CraftUI] ICON Slot Cached: {child.name}");
        }

        Debug.Log($"[CraftUI] Total Recipes: {allRecipes.Count}");
        Debug.Log($"[CraftUI] Total BG Slots: {bgSlots.Count}");
        Debug.Log($"[CraftUI] Total ICON Slots: {iconSlots.Count}");
    }

    private void OnEnable()
    {
        Debug.Log("[CraftUI] OnEnable - Binding Input Actions");

        if (nextPageAction != null)
        {
            nextPageAction.action.performed += OnNextPage;
            nextPageAction.action.Enable();
            Debug.Log("[CraftUI] Next Page Input ENABLED");
        }
        else Debug.LogWarning("[CraftUI] Next Page Action NOT ASSIGNED");

        if (prevPageAction != null)
        {
            prevPageAction.action.performed += OnPrevPage;
            prevPageAction.action.Enable();
            Debug.Log("[CraftUI] Prev Page Input ENABLED");
        }
        else Debug.LogWarning("[CraftUI] Prev Page Action NOT ASSIGNED");
    }

    private void OnDisable()
    {
        Debug.Log("[CraftUI] OnDisable - Unbinding Input Actions");

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
        Debug.Log("[CraftUI] Start - Initializing UI");

        craftButton.onClick.AddListener(OnCraftClicked);

        RefreshGrid();
        SelectRecipe(null);
    }

    // ---------------- INPUT ----------------

    private void OnNextPage(InputAction.CallbackContext ctx)
    {
        Debug.Log("[CraftUI] INPUT: Next Page Triggered");
        NextPage();
    }

    private void OnPrevPage(InputAction.CallbackContext ctx)
    {
        Debug.Log("[CraftUI] INPUT: Prev Page Triggered");
        PrevPage();
    }

    // ---------------- PAGINATION ----------------

    public void NextPage()
    {
        Debug.Log($"[CraftUI] NextPage called - Current Page: {currentPage}");

        if ((currentPage + 1) * recipesPerPage < allRecipes.Count)
        {
            currentPage++;
            Debug.Log($"[CraftUI] Page changed -> {currentPage}");
            RefreshGrid();
        }
        else
        {
            Debug.LogWarning("[CraftUI] NextPage blocked (end of list)");
        }
    }

    public void PrevPage()
    {
        Debug.Log($"[CraftUI] PrevPage called - Current Page: {currentPage}");

        if (currentPage > 0)
        {
            currentPage--;
            Debug.Log($"[CraftUI] Page changed -> {currentPage}");
            RefreshGrid();
        }
        else
        {
            Debug.LogWarning("[CraftUI] PrevPage blocked (already at 0)");
        }
    }

    // ---------------- GRID ----------------

    public void RefreshGrid()
    {

        int startIndex = currentPage * recipesPerPage;
        int endIndex = Mathf.Min(startIndex + recipesPerPage, allRecipes.Count);

        Debug.Log($"[CraftUI] Showing recipes {startIndex} → {endIndex - 1}");

        int slotIndex = 0;

        for (int i = startIndex; i < endIndex; i++)
        {
            CraftingSO recipe = allRecipes[i];

            Transform bgSlot = bgSlots[slotIndex];
            Transform iconSlot = iconSlots[slotIndex];


            bgSlot.gameObject.SetActive(true);
            iconSlot.gameObject.SetActive(true);

            // BUTTON
            Button btn = bgSlot.GetComponent<Button>();

            if (btn == null)
            {
                Debug.LogError($"[CraftUI] Missing Button on BG slot: {bgSlot.name}");
            }
            else
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[CraftUI] CLICKED: {recipe.resultItem?.itemName}");
                    SelectRecipe(recipe);
                });
            }

            // ICON
            Image icon = iconSlot.GetComponent<Image>();

            if (icon == null)
            {
                Debug.LogError($"[CraftUI] Missing Image on ICON slot: {iconSlot.name}");
            }
            else
            {
                if (recipe.resultItem != null && recipe.resultItem.icon != null)
                {
                    icon.sprite = recipe.resultItem.icon;
                    icon.enabled = true;

                }
                else
                {
                    icon.sprite = null;
                    icon.enabled = false;

                    Debug.LogWarning($"[CraftUI] Missing icon for recipe index {i}");
                }

                icon.raycastTarget = false;
            }

            slotIndex++;
        }

        // Hide unused slots
        for (int i = slotIndex; i < bgSlots.Count; i++)
        {
            bgSlots[i].gameObject.SetActive(false);
            iconSlots[i].gameObject.SetActive(false);
        }

    }

    // ---------------- SELECTION ----------------

    public void SelectRecipe(CraftingSO recipe)
    {
        selectedRecipe = recipe;

        if (recipe == null)
        {
            Debug.Log("[CraftUI] Selection cleared");

            selectedNameText.text = "Select a Recipe";
            selectedDescriptionText.text = "";
            selectedIconImage.enabled = false;
            craftButton.interactable = false;
            return;
        }

        Debug.Log($"[CraftUI] Selected Recipe: {recipe.resultItem?.itemName}");

        selectedNameText.text = recipe.resultItem != null
            ? recipe.resultItem.itemName
            : "Unknown Item";

        selectedDescriptionText.text = "Requires:" + GetIngredientsString(recipe);

        if (recipe.resultItem != null && recipe.resultItem.icon != null)
        {
            selectedIconImage.sprite = recipe.resultItem.icon;
            selectedIconImage.enabled = true;

            Debug.Log("[CraftUI] Selection icon updated");
        }
        else
        {
            selectedIconImage.sprite = null;
            selectedIconImage.enabled = false;

            Debug.LogWarning("[CraftUI] Selection icon missing");
        }

        craftButton.interactable = recipe.CanCraft();

        Debug.Log($"[CraftUI] CanCraft = {craftButton.interactable}");
    }

    private string GetIngredientsString(CraftingSO recipe)
    {
        string text = "";

        foreach (var ing in recipe.ingredients)
        {
            int current = InventoryManager.Instance.GetTotalCount(ing.item);
            string color = current >= ing.amount ? "white" : "red";

            text += $"\n- {ing.item.itemName}: {current}/{ing.amount}";
        }

        return text;
    }

    // ---------------- CRAFT ----------------

    private void OnCraftClicked()
    {
        Debug.Log("[CraftUI] Craft Button Clicked");

        if (selectedRecipe != null && selectedRecipe.CanCraft())
        {
            Debug.Log($"[CraftUI] Crafting: {selectedRecipe.resultItem?.itemName}");

            selectedRecipe.Craft();

            SelectRecipe(selectedRecipe);
            RefreshGrid();
        }
        else
        {
            Debug.LogWarning("[CraftUI] Craft failed (null or CanCraft = false)");
        }
    }
}