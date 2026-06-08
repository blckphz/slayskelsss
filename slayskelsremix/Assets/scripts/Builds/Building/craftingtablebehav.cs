using System.Collections.Generic;
using UnityEngine;

public class craftingtablebehav : MonoBehaviour, IInteractable
{
    [Header("Table Recipes")]
    public CraftingSO[] craftingSO;

    public string prompt = "Open Crafting Table";

    public void Interact(InventoryManager playerInventory)
    {
        if (CraftingManager.Instance == null) return;

        CraftingManager.Instance.SetActiveRecipes(new List<CraftingSO>(craftingSO));

        CraftingManager.Instance.ToggleCraftingUI();

        InteractionUI.Instance?.Hide();
    }

    public string GetPrompt() => prompt;

    public void OnFocus() { }

    public void OnLoseFocus()
    {
        CraftingManager.Instance?.ClearActiveRecipes();
    }
}