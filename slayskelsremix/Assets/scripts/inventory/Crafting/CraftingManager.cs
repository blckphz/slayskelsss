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
            Debug.Log("<color=yellow>[Crafting]</color> No save found. Loading starter recipes.");

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
        toggleCrafting.performed -= OnToggleCrafting;
        toggleCrafting.Disable();
    }

    private void OnToggleCrafting(InputAction.CallbackContext context)
    {
        ToggleCraftingUI();
    }

    public void ToggleCraftingUI()
    {
        if (craftingUI == null) return;

        bool isActive = !craftingUI.activeSelf;
        craftingUI.SetActive(isActive);

        if (isActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            CraftUIManager.Instance?.RefreshGrid();
            Debug.Log("<color=white>[Crafting]</color> Opened.");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // =====================================================
    // LEARN RECIPE
    // =====================================================
    public bool LearnRecipe(CraftingSO recipe)
    {
        if (recipe == null) return false;

        if (knownRecipes.Contains(recipe))
        {
            Debug.Log("<color=white>[Crafting]</color> Already known.");
            return false;
        }

        knownRecipes.Add(recipe);
        SaveRecipes();

        Debug.Log($"<color=green>[Crafting]</color> Learned {recipe.name}");

        CraftUIManager.Instance?.RefreshGrid();
        return true;
    }

    // =====================================================
    // CRAFT ITEM
    // =====================================================
    public void CraftItem(CraftingSO recipe)
    {
        if (recipe == null) return;

        if (!recipe.CanCraft())
        {
            Debug.Log("<color=red>[Crafting]</color> Missing materials.");
            return;
        }

        // consume ingredients
        foreach (var ingredient in recipe.ingredients)
        {
            InventoryManager.Instance.RemoveItem(ingredient.item, ingredient.amount);
        }

        InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultCount);

        Debug.Log($"<color=lime>[Crafting]</color> Crafted {recipe.resultItem.itemName}");

        CraftUIManager.Instance?.RefreshGrid();
    }

    // =====================================================
    // SAVE
    // =====================================================
    public void SaveRecipes()
    {
        RecipeSaveData data = new RecipeSaveData();

        foreach (var r in knownRecipes)
        {
            if (r != null)
                data.learnedRecipeNames.Add(r.name);
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        Debug.Log("<color=grey>[Crafting]</color> Saved recipes.");
    }

    // =====================================================
    // LOAD
    // =====================================================
    public void LoadRecipes()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        RecipeSaveData data = JsonUtility.FromJson<RecipeSaveData>(json);

        knownRecipes.Clear();

        if (recipeDatabase == null)
        {
            Debug.LogError("<color=red>[Crafting]</color> Missing database!");
            return;
        }

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