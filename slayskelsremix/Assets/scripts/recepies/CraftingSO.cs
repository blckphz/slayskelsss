using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Ingredient
{
    public ItemData item;
    public int amount;
}

// Enum for recipe category
public enum RecipeType
{
    Deco,
    Functionality,
    Tool
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Crafting/Recipe")]
public class CraftingSO : ScriptableObject
{
    [Header("Category")]
    public RecipeType recipeType;

    [Header("Output")]
    public ItemData resultItem;
    public int resultCount = 1;

    [Header("Requirements")]
    public List<Ingredient> ingredients = new List<Ingredient>();

    // Optional: How long it takes to craft
    public float craftTime = 0f;

    public bool CanCraft()
    {
        if (InventoryManager.Instance == null) return false;

        foreach (var ingredient in ingredients)
        {
            if (InventoryManager.Instance.GetTotalCount(ingredient.item) < ingredient.amount)
            {
                return false;
            }
        }
        return true;
    }

    public void Craft()
    {
        if (!CanCraft()) return;

        // 1. Consume Ingredients
        foreach (var ingredient in ingredients)
        {
            InventoryManager.Instance.RemoveItem(ingredient.item, ingredient.amount);
        }

        // 2. Add Result
        InventoryManager.Instance.AddItem(resultItem, resultCount);

        Debug.Log($"<color=green>[Crafting]</color> Created {resultCount}x {resultItem.itemName}");
    }
}