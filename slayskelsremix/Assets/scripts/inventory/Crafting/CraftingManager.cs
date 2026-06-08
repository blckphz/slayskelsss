using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("UI Settings")]
    [SerializeField] private GameObject craftingUI;
    [SerializeField] private InputAction toggleCrafting;

    [Header("Recipe Data")]
    public List<CraftingSO> startingRecipes = new List<CraftingSO>();
    public List<CraftingSO> knownRecipes = new List<CraftingSO>();
    public CraftingDatabase recipeDatabase;

    private string savePath;

    public List<CraftingSO> ActiveRecipes { get; private set; } = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/recipes.json";
    }

    private void Start()
    {
        if (File.Exists(savePath))
        {
            LoadRecipes();
        }
        else
        {
            foreach (var recipe in startingRecipes)
            {
                if (recipe != null && !knownRecipes.Contains(recipe))
                    knownRecipes.Add(recipe);
            }

            SaveRecipes();
        }

        ClearActiveRecipes();
    }

    private void OnEnable()
    {
        toggleCrafting.Enable();
        toggleCrafting.performed += OnToggleCrafting;
    }

    private void OnDisable()
    {
        toggleCrafting.performed -= OnToggleCrafting;
        toggleCrafting.Disable();
    }

    private void OnToggleCrafting(InputAction.CallbackContext context)
    {
        ToggleCraftingUI();
    }

    // ✅ NEW CLEAN CONTROL METHOD
    public void SetCraftingUI(bool state)
    {
        if (craftingUI == null) return;

        if (craftingUI.activeSelf == state) return;

        craftingUI.SetActive(state);

        if (state)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            CraftUIManager.Instance?.RefreshGrid();
        }
        else
        {
            ClearActiveRecipes();
        }
    }

    public void ToggleCraftingUI()
    {
        SetCraftingUI(!craftingUI.activeSelf);
    }

    public void SetActiveRecipes(List<CraftingSO> recipes)
    {
        ActiveRecipes = recipes != null
            ? new List<CraftingSO>(recipes)
            : new List<CraftingSO>();

        CraftUIManager.Instance?.RefreshGrid();
    }

    public void ClearActiveRecipes()
    {
        ActiveRecipes = new List<CraftingSO>(knownRecipes);
        CraftUIManager.Instance?.RefreshGrid();
    }

    public bool LearnRecipe(CraftingSO recipe)
    {
        if (recipe == null) return false;

        if (knownRecipes.Contains(recipe))
            return false;

        knownRecipes.Add(recipe);
        SaveRecipes();

        CraftUIManager.Instance?.RefreshGrid();
        return true;
    }

    public void CraftItem(CraftingSO recipe)
    {
        if (recipe == null) return;

        foreach (var ingredient in recipe.ingredients)
        {
            var items = InventoryManager.Instance.GetItems(ingredient.item);

            int validCount = 0;

            foreach (var instance in items)
            {
                if (instance.CurrentDurability >= instance.MaxDurability)
                    validCount += instance.StackCount;
            }

            if (validCount < ingredient.amount)
                return;
        }

        foreach (var ingredient in recipe.ingredients)
        {
            InventoryManager.Instance.RemoveItemsWithCondition(
                ingredient.item,
                ingredient.amount,
                (item) => item.CurrentDurability >= item.MaxDurability
            );
        }

        InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultCount);

        CraftUIManager.Instance?.RefreshGrid();
    }

    public void SaveRecipes()
    {
        RecipeSaveData data = new RecipeSaveData();

        foreach (var r in knownRecipes)
        {
            if (r != null)
                data.learnedRecipeNames.Add(r.name);
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    public void LoadRecipes()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        RecipeSaveData data = JsonUtility.FromJson<RecipeSaveData>(json);

        knownRecipes.Clear();

        if (recipeDatabase == null)
            return;

        foreach (string recipeName in data.learnedRecipeNames)
        {
            CraftingSO found = recipeDatabase.GetRecipeByName(recipeName);

            if (found != null)
                knownRecipes.Add(found);
        }
    }
}

[System.Serializable]
public class RecipeSaveData
{
    public List<string> learnedRecipeNames = new List<string>();
}