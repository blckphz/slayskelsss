using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Useable Item")]
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
            Debug.Log($"<color=red>[Item System]</color> Cannot use {itemName} (ID: {itemID}). Count is 0!");
            return;
        }

        if (abilityToExecute != null)
        {
            Debug.Log($"<color=cyan>[Item System]</color> Attempting Ability: {abilityToExecute.name}");

            // Trigger the execution and capture if it actually fired
            // We pass 'true' for isHolding as per your existing logic
            bool hasExecuted = abilityToExecute.Execute(caster, targetAnchor, true);

            if (hasExecuted)
            {
                // 2. SMART REMOVAL: Only remove if the stone was actually thrown
                InventoryManager.Instance.RemoveItem(this, 1);

                int remaining = GetCurrentCount();
                Debug.Log($"<color=green>[Item System]</color> Successfully used {itemName}. Remaining: {remaining}");
            }
            else
            {
                Debug.Log("<color=orange>[Item System]</color> Ability failed to execute. Item not consumed.");
            }
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
        {
            if (slot.item != null && slot.item.itemID == this.itemID)
                totalFound += slot.count;
        }

        // Check Hotbar
        foreach (var hSlot in InventoryManager.Instance.hotbarData)
        {
            if (hSlot.item != null && hSlot.item.itemID == this.itemID)
                totalFound += hSlot.count;
        }

        return totalFound;
    }
}