using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Items/Useable Item")]
public class UseableItem : ItemData
{
    [Header("Ability to Trigger")]
    public Ability abilityToExecute;

    public override void Use(Transform caster, Transform targetAnchor)
    {
        // 1. GLOBAL SEARCH: Check Inventory + Hotbar
        int currentCount = GetCurrentCount();

        if (currentCount <= 0)
        {
            Debug.Log($"<color=red>[Item System]</color> Cannot use {itemName} (ID: {itemID}). Count is 0 in both Inventory and Hotbar!");
            return;
        }

        if (abilityToExecute != null)
        {
            Debug.Log($"<color=cyan>[Item System]</color> Executing Ability: {abilityToExecute.name} for item {itemName}");

            // Trigger the execution (the throw/spawn logic)
            abilityToExecute.Execute(caster, targetAnchor, true);

            // 2. SMART REMOVAL: Remove from wherever it is currently sitting
            InventoryManager.Instance.RemoveItem(this, 1);

            // Final check for the console
            int remaining = GetCurrentCount();
            Debug.Log($"<color=green>[Item System]</color> Successfully used {itemName}. <color=yellow>Total Remaining (Global): {remaining}</color>");
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[Item System]</color> {itemName} has no Ability assigned!");
        }
    }

    private int GetCurrentCount()
    {
        if (InventoryManager.Instance == null) return 0;
        int totalFound = 0;

        // Check Inventory
        foreach (var slot in InventoryManager.Instance.inventory)
            if (slot.item != null && slot.item.itemID == this.itemID) totalFound += slot.count;

        // Check Hotbar (Now using the .count field we just added)
        foreach (var hSlot in InventoryManager.Instance.hotbarData)
            if (hSlot.item != null && hSlot.item.itemID == this.itemID) totalFound += hSlot.count;

        return totalFound;
    }
}