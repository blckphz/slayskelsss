using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CraftItem(CraftingSO recipe)
    {
        if (recipe == null) return;

        if (recipe.CanCraft())
        {
            // 1. Deduct all ingredients
            foreach (var ingredient in recipe.ingredients)
            {
                InventoryManager.Instance.ConsumeResources(ingredient.item, ingredient.amount);
            }

            // 2. Add the result
            InventoryManager.Instance.AddItem(recipe.resultItem, recipe.resultCount);

        }
        else
        {
            Debug.Log("<color=red>[Crafting]</color> Missing required materials.");
        }
    }
}