using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingDatabase", menuName = "Inventory/Crafting Database")]
public class CraftingDatabase : ScriptableObject
{
    public List<CraftingSO> allRecipes = new List<CraftingSO>();

    public CraftingSO GetRecipeByName(string recipeName)
    {
        return allRecipes.Find(r => r.name == recipeName);
    }
}