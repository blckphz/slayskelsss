using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftUIManager : MonoBehaviour
{
    public static CraftUIManager Instance;

    [Header("Recipe Database")]
    public List<CraftingSO> allRecipes = new List<CraftingSO>();

    [Header("Pagination Settings")]
    public int recipesPerPage = 10;
    private int currentPage = 0;

    [Header("UI Grid References")]
    public Transform recipeGridParent; // Object with GridLayoutGroup
    public GameObject recipeSlotPrefab; // Button prefab for the recipe icon

    [Header("Selection Panel")]
    public TextMeshProUGUI selectedNameText;
    public TextMeshProUGUI selectedDescriptionText;
    public Image selectedIconImage;
    public Button craftButton;

    [Header("Navigation UI")]
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI pageText;

    private CraftingSO selectedRecipe;
    private List<GameObject> activeSlots = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Hook up button events
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
        craftButton.onClick.AddListener(OnCraftClicked);

        RefreshGrid();
        SelectRecipe(null); // Clear selection on start
    }

    // ---------------- PAGINATION ----------------

    public void NextPage()
    {
        if ((currentPage + 1) * recipesPerPage < allRecipes.Count)
        {
            currentPage++;
            RefreshGrid();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshGrid();
        }
    }

    // ---------------- GRID LOGIC ----------------

    public void RefreshGrid()
    {
        // 1. Clear current slots
        foreach (GameObject slot in activeSlots) Destroy(slot);
        activeSlots.Clear();

        // 2. Calculate range for current page
        int startIndex = currentPage * recipesPerPage;
        int endIndex = Mathf.Min(startIndex + recipesPerPage, allRecipes.Count);

        // 3. Spawn icons for this page
        for (int i = startIndex; i < endIndex; i++)
        {
            CraftingSO recipe = allRecipes[i];
            GameObject slot = Instantiate(recipeSlotPrefab, recipeGridParent);
            activeSlots.Add(slot);

            // Setup Icon (assuming prefab has an Image component or child named "Icon")
            Image icon = slot.GetComponent<Image>();
            if (icon == null) icon = slot.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = recipe.resultItem.icon;

            // Setup Click Action
            Button btn = slot.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectRecipe(recipe));
        }

        // 4. Update Page Text
        int totalPages = Mathf.CeilToInt((float)allRecipes.Count / recipesPerPage);
        if (pageText != null) pageText.text = $"Page {currentPage + 1} / {totalPages}";

        // Update nav buttons interactability
        prevButton.interactable = (currentPage > 0);
        nextButton.interactable = (endIndex < allRecipes.Count);
    }

    // ---------------- SELECTION ----------------

    public void SelectRecipe(CraftingSO recipe)
    {
        selectedRecipe = recipe;

        if (recipe == null)
        {
            selectedNameText.text = "Select a Recipe";
            selectedDescriptionText.text = "";
            selectedIconImage.enabled = false;
            craftButton.interactable = false;
            return;
        }

        selectedNameText.text = recipe.resultItem.itemName;
        selectedDescriptionText.text = "Requires: " + GetIngredientsString(recipe);
        selectedIconImage.sprite = recipe.resultItem.icon;
        selectedIconImage.enabled = true;

        // Auto-check if we can craft to enable/disable button
        craftButton.interactable = recipe.CanCraft();
    }

    private string GetIngredientsString(CraftingSO recipe)
    {
        string text = "";
        foreach (var ing in recipe.ingredients)
        {
            int current = InventoryManager.Instance.GetTotalCount(ing.item);
            string color = current >= ing.amount ? "white" : "red";
            text += $"\n- <color={color}>{ing.item.itemName}: {current}/{ing.amount}</color>";
        }
        return text;
    }

    // ---------------- CRAFT ACTION ----------------

    private void OnCraftClicked()
    {
        if (selectedRecipe != null && selectedRecipe.CanCraft())
        {
            // Use the Craft logic from your ScriptableObject
            selectedRecipe.Craft();

            // Refresh selection and grid to update counts/interactability
            SelectRecipe(selectedRecipe);
            RefreshGrid();
        }
    }
}