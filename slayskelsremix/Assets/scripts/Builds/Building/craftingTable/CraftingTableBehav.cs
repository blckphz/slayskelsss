using System.Collections.Generic;
using UnityEngine;

public class CraftingTableBehav : MonoBehaviour, IInteractable
{
    [Header("Recipes available at this table")]
    public CraftingSO[] availableRecipes;

    private CraftingManager crafting;

    private void Start()
    {
        crafting = CraftingManager.Instance;
    }

    public void Interact(InventoryManager playerInventory)
    {
        if (crafting == null) return;

        crafting.SetRuntimeRecipes(new List<CraftingSO>(availableRecipes));
        crafting.ToggleCraftingUI();
    }

    public string GetPrompt()
    {
        return "[E] Open Crafting";
    }

    public void OnFocus() { }
    public void OnLoseFocus() { }
}