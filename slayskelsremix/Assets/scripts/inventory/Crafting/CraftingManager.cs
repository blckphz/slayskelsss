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
    public CraftingDatabase recipeDatabase; // Assign your database here

    private string savePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        savePath = Application.persistentDataPath + "/recipes.json";
    }

    private void Start()
    {
        // Load known recipes or give starting ones if new game
        if (File.Exists(savePath))
        {
            LoadRecipes();
        }
        else
        {
            Debug.Log("<color=yellow>[Crafting]</color> No save found. Giving starting recipes.");
            foreach (var recipe in startingRecipes)
            {
                if (recipe != null && !knownRecipes.Contains(recipe))
                    knownRecipes.Add(recipe);
            }
            SaveRecipes();
        }
    }

    private void OnEnable()
    {
        toggleCrafting.Enable();
        toggleCrafting.performed += OnToggleCrafting;
    }

    private void OnDisable()
    {
        toggleCrafting.Disable();
        toggleCrafting.performed -= OnToggleCrafting;
    }

    private void OnToggleCrafting(InputAction.CallbackContext context) => ToggleCraftingUI();

    public void ToggleCraftingUI()
    {
        if (craftingUI != null)
        {
            bool isActive = !craftingUI.activeSelf;
            craftingUI.SetActive(isActive);

            if (isActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                CraftUIManager.Instance?.RefreshGrid();
                Debug.Log("<color=white>[Crafting]</color> Menu Opened.");
            }
        }
    }

    public bool LearnRecipe(CraftingSO recipe)
    {
        if (recipe == null) return false;

        if (!knownRecipes.Contains(recipe))
        {
            knownRecipes.Add(recipe);
            Debug.Log($"<color=green>[Crafting]</color> Learned: {recipe.name}");
            SaveRecipes();
            CraftUIManager.Instance?.RefreshGrid();
            return true;
        }
        Debug.Log("<color=white>[Crafting]</color> Recipe already known.");
        return false;
    }

    public void CraftItem(CraftingSO recipe)
    {
        if (recipe == null) return;

        if (recipe.CanCraft())
        {
            foreach (var ingredient in recipe.ingredients)
            {
                // Take resources from the Inventory
                InventoryManager.Instance.ConsumeResources(ingredient.item, ingredient.amount);
            }
            InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultCount);
            Debug.Log($"<color=lime>[Crafting]</color> Successfully crafted {recipe.resultItem.itemName}.");
            CraftUIManager.Instance?.RefreshGrid();
        }
        else
        {
            Debug.Log("<color=red>[Crafting]</color> Missing materials for " + recipe.resultItem.itemName);
        }
    }

    // --- JSON PERSISTENCE ---

    public void SaveRecipes()
    {
        RecipeSaveData data = new RecipeSaveData();
        foreach (var r in knownRecipes)
        {
            if (r != null) data.learnedRecipeNames.Add(r.name);
        }
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("<color=grey>[System]</color> Recipes saved to " + savePath);
    }

    public void LoadRecipes()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        RecipeSaveData data = JsonUtility.FromJson<RecipeSaveData>(json);

        knownRecipes.Clear();
        if (recipeDatabase != null)
        {
            foreach (string rName in data.learnedRecipeNames)
            {
                // Find recipe in your database by name
                CraftingSO found = recipeDatabase.GetRecipeByName(rName);
                if (found != null) knownRecipes.Add(found);
            }
            Debug.Log($"<color=grey>[System]</color> Loaded {knownRecipes.Count} recipes.");
        }
        else
        {
            Debug.LogError("<color=red>[Crafting]</color> RecipeDatabase is missing on CraftingManager!");
        }
    }
}

[System.Serializable]
public class RecipeSaveData
{
    public List<string> learnedRecipeNames = new List<string>();
}